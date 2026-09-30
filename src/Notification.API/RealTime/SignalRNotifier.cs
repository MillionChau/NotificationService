using Notification.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Notification.API.Hubs;

namespace Notification.API.RealTime;

/// <summary>Cầu nối từ Application sang SignalR: đẩy payload tới group user-{id} hoặc admins.</summary>
public class SignalRNotifier : IRealTimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyUserAsync(object payload, CancellationToken cancellationToken = default)
    {
        if (payload is not NotificationPayload p)
            throw new ArgumentException("Payload phải là NotificationPayload", nameof(payload));

        return _hubContext.Clients
            .Group($"user-{p.RecipientId}")
            .SendAsync("notification-received", p, cancellationToken);
    }

    public Task NotifyAdminsAsync(object payload, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group("admins")
            .SendAsync("pending-report-received", payload, cancellationToken);
    }
}

/// <summary>Payload realtime gửi client — giống NotificationDto + RecipientId để route group.</summary>
public class NotificationPayload
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
