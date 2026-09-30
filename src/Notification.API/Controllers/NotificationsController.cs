using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using Notification.Application.Features.Notifications.Commands.MarkAllAsRead;
using Notification.Application.Features.Notifications.Commands.MarkAsRead;
using Notification.Application.Features.Notifications.Queries.GetMyNotifications;
using Notification.Application.Features.Notifications.Queries.GetUnreadCount;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Danh sách thông báo của tôi (FR-30), mới nhất trước, lọc được theo đã đọc.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ResponseModel<PagedResponse<NotificationDto>>>> GetMy(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery(page, pageSize, isRead), cancellationToken);
        return Ok(result);
    }

    /// <summary>Số thông báo chưa đọc — badge navbar (FR-31).</summary>
    [HttpGet("me/unread-count")]
    public async Task<ActionResult<ResponseModel<UnreadCountDto>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Đánh dấu một thông báo đã đọc (FR-32).</summary>
    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<ResponseModel<MarkReadResultDto>>> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAsReadCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Đánh dấu tất cả đã đọc (FR-33).</summary>
    [HttpPut("read-all")]
    public async Task<ActionResult<ResponseModel<MarkReadResultDto>>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAllAsReadCommand(), cancellationToken);
        return Ok(result);
    }
}
