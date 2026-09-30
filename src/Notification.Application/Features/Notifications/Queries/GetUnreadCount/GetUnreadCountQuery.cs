using MediatR;
using Notification.Application.Common.Interfaces;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using Notification.Domain.Contracts;

namespace Notification.Application.Features.Notifications.Queries.GetUnreadCount;

/// <summary>Đếm thông báo chưa đọc của user hiện tại — badge trên navbar (FR-31).</summary>
public record GetUnreadCountQuery : IRequest<ResponseModel<UnreadCountDto>>;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, ResponseModel<UnreadCountDto>>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadCountQueryHandler(INotificationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResponseModel<UnreadCountDto>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return ResponseModel<UnreadCountDto>.Error("Unauthorized");

        var count = await _repository.CountUnreadAsync(userId, cancellationToken);
        return ResponseModel<UnreadCountDto>.Success(new UnreadCountDto { UnreadCount = count });
    }
}
