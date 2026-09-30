using MediatR;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using FluentValidation;
using Notification.Domain.Contracts;

namespace Notification.Application.Features.Notifications.Queries.GetPendingForAdmin;

/// <summary>
/// Hàng chờ "báo cáo chờ duyệt" cho Admin (bám UC-23 CommunityService) —
/// chỉ trả notification type PendingReport, cũ nhất lên đầu, phân trang.
/// </summary>
public record GetPendingForAdminQuery(int Page = 1, int PageSize = 20)
    : IRequest<ResponseModel<PagedResponse<PendingReportDto>>>;

public class GetPendingForAdminQueryValidator : AbstractValidator<GetPendingForAdminQuery>
{
    public GetPendingForAdminQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetPendingForAdminQueryHandler : IRequestHandler<GetPendingForAdminQuery, ResponseModel<PagedResponse<PendingReportDto>>>
{
    private readonly INotificationRepository _repository;

    public GetPendingForAdminQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseModel<PagedResponse<PendingReportDto>>> Handle(
        GetPendingForAdminQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPendingForAdminAsync(
            request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(n => new PendingReportDto
        {
            NotificationId = n.Id,
            ReportId = n.RelatedEntityId ?? Guid.Empty,
            PostId = n.RelatedPostId ?? Guid.Empty,
            ReporterId = n.ActorId ?? Guid.Empty,
            Content = n.Content,
            LinkUrl = n.LinkUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
        }).ToList();

        return ResponseModel<PagedResponse<PendingReportDto>>.Success(
            PagedResponse<PendingReportDto>.Create(dtos, request.Page, request.PageSize, totalCount));
    }
}
