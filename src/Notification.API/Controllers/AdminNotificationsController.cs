using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using Notification.Application.Features.Notifications.Queries.GetPendingForAdmin;
using Notification.Application.Features.Notifications.Queries.GetPendingCountForAdmin;

namespace Notification.API.Controllers;

/// <summary>
/// Endpoint Admin cho hàng chờ duyệt báo cáo (bám UC-23 CommunityService):
/// xem queue + badge counter + đánh dấu đã xử lý.
/// </summary>
[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminNotificationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Hàng chờ báo cáo chờ duyệt — cũ nhất lên đầu, phân trang.</summary>
    [HttpGet("pending-reports")]
    public async Task<ActionResult<ResponseModel<PagedResponse<PendingReportDto>>>> GetPendingReports(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPendingForAdminQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Badge counter: số báo cáo đang chờ + báo cũ nhất đã chờ bao lâu.</summary>
    [HttpGet("pending-count")]
    public async Task<ActionResult<ResponseModel<PendingReportCountDto>>> GetPendingCount(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingCountForAdminQuery(), cancellationToken);
        return Ok(result);
    }
}
