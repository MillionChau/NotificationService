using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Notification.API.DTOs;
using Notification.API.Hubs;
using Notification.Domain.Contracts;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.API.Controllers;

/// <summary>
/// Endpoint nội bộ cho microservice khác (CommunityService...) đẩy thông báo vào hệ thống.
/// Bảo vệ bằng header X-Api-Key (mạng nội bộ Docker, không cần JWT).
/// Vừa lưu DB vừa broadcast realtime qua SignalR ngay tại đây.
/// </summary>
[ApiController]
[Route("api/internal/notifications")]
public class InternalNotificationsController : ControllerBase
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly IConfiguration _configuration;

    public InternalNotificationsController(
        INotificationRepository repository,
        IHubContext<NotificationsHub> hubContext,
        IConfiguration configuration)
    {
        _repository = repository;
        _hubContext = hubContext;
        _configuration = configuration;
    }

    /// <summary>Tạo thông báo từ service khác + broadcast realtime tới người nhận.</summary>
    [HttpPost]
    public async Task<IActionResult> Ingest(
        [FromBody] IngestNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsApiKeyValid())
            return Unauthorized(new { message = "Invalid X-Api-Key" });

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(new { message = "Items is required" });

        var created = new List<NotificationEntity>();

        foreach (var item in request.Items)
        {
            var notification = new NotificationEntity
            {
                RecipientId = item.RecipientId,
                Type = (NotificationType)item.Type,
                Content = item.Content,
                LinkUrl = item.LinkUrl,
                SourceService = item.SourceService,
                RelatedEntityId = item.RelatedEntityId,
                RelatedPostId = item.RelatedPostId,
                ActorId = item.ActorId,
                ActorName = item.ActorName,
                CreatedAt = DateTime.UtcNow
            };
            created.Add(notification);
        }

        _repository.AddRange(created);
        await _repository.SaveChangesAsync(cancellationToken);

        // Broadcast realtime — mỗi thông báo tới đúng group user-{recipientId}
        foreach (var n in created)
        {
            await _hubContext.Clients
                .Group($"user-{n.RecipientId}")
                .SendAsync("notification-received", ToPayload(n), cancellationToken);

            // Admin queue: PendingReport đồng thời broadcast vào group admins
            if (n.Type == NotificationType.PendingReport)
            {
                await _hubContext.Clients
                    .Group("admins")
                    .SendAsync("pending-report-received", ToPayload(n), cancellationToken);
            }
        }

        return Ok(new { createdCount = created.Count });
    }

    /// <summary>Map entity → payload realtime cho client.</summary>
    private static object ToPayload(NotificationEntity n) => new
    {
        n.Id,
        n.RecipientId,
        Type = n.Type.ToString(),
        n.Content,
        n.LinkUrl,
        n.RelatedEntityId,
        n.ActorName,
        n.IsRead,
        n.CreatedAt
    };

    /// <summary>Admin đã review xong báo cáo → đánh dấu các notification PendingReport của report đó là đã đọc.</summary>
    [HttpPut("resolve-pending")]
    public async Task<IActionResult> ResolvePending(
        [FromBody] ResolvePendingReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsApiKeyValid())
            return Unauthorized(new { message = "Invalid X-Api-Key" });

        await _repository.MarkPendingReportResolvedAsync(request.ReportId, cancellationToken);
        return Ok(new { resolved = true });
    }

    private bool IsApiKeyValid()
    {
        var configured = _configuration["ApiKey:Key"];
        if (string.IsNullOrEmpty(configured)) return false;

        var provided = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(provided)) return false;

        // Constant-time compare chống timing attack
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(configured), Encoding.UTF8.GetBytes(provided));
    }
}
