using AutoMapper;
using MediatR;
using Notification.Application.Common.Interfaces;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using FluentValidation;
using Notification.Domain.Contracts;

namespace Notification.Application.Features.Notifications.Queries.GetMyNotifications;

/// <summary>
/// Danh sách thông báo của user hiện tại (FR-30: xem danh sách thông báo) —
/// mới nhất trước, phân trang, lọc theo trạng thái đã đọc.
/// </summary>
public record GetMyNotificationsQuery(int Page = 1, int PageSize = 20, bool? IsRead = null)
    : IRequest<ResponseModel<PagedResponse<NotificationDto>>>;

public class GetMyNotificationsQueryValidator : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, ResponseModel<PagedResponse<NotificationDto>>>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetMyNotificationsQueryHandler(
        INotificationRepository repository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _repository = repository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<ResponseModel<PagedResponse<NotificationDto>>> Handle(
        GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return ResponseModel<PagedResponse<NotificationDto>>.Error("Unauthorized");

        var (items, totalCount) = await _repository.GetByRecipientAsync(
            userId, request.Page, request.PageSize, request.IsRead, cancellationToken);

        var dtos = items.Select(_mapper.Map<NotificationDto>).ToList();

        return ResponseModel<PagedResponse<NotificationDto>>.Success(
            PagedResponse<NotificationDto>.Create(dtos, request.Page, request.PageSize, totalCount));
    }
}
