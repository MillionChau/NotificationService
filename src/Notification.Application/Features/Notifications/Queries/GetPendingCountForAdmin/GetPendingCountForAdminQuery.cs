using MediatR;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using Notification.Domain.Contracts;

namespace Notification.Application.Features.Notifications.Queries.GetPendingCountForAdmin;

/// <summary>Số báo cáo đang chờ duyệt + báo cũ nhất đã chờ bao lâu — badge cho Admin (UC-23).</summary>
public record GetPendingCountForAdminQuery : IRequest<ResponseModel<PendingReportCountDto>>;

public class GetPendingCountForAdminQueryHandler : IRequestHandler<GetPendingCountForAdminQuery, ResponseModel<PendingReportCountDto>>
{
    private readonly INotificationRepository _repository;

    public GetPendingCountForAdminQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseModel<PendingReportCountDto>> Handle(
        GetPendingCountForAdminQuery request, CancellationToken cancellationToken)
    {
        var count = await _repository.CountPendingForAdminAsync(cancellationToken);
        var oldest = await _repository.GetOldestPendingCreatedAtAsync(cancellationToken);

        return ResponseModel<PendingReportCountDto>.Success(new PendingReportCountDto
        {
            PendingCount = count,
            OldestPendingAt = oldest
        });
    }
}
