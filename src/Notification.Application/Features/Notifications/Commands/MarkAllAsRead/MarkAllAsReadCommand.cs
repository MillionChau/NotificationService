using MediatR;
using Notification.Application.Common.Interfaces;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using Notification.Domain.Contracts;

namespace Notification.Application.Features.Notifications.Commands.MarkAllAsRead;

/// <summary>Đánh dấu TẤT CẢ thông báo chưa đọc của user là đã đọc (FR-33).</summary>
public record MarkAllAsReadCommand : IRequest<ResponseModel<MarkReadResultDto>>;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, ResponseModel<MarkReadResultDto>>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public MarkAllAsReadCommandHandler(INotificationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResponseModel<MarkReadResultDto>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return ResponseModel<MarkReadResultDto>.Error("Unauthorized");

        var now = DateTime.UtcNow;
        var unread = await _repository.GetUnreadByRecipientAsync(userId, cancellationToken);

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }
        await _repository.SaveChangesAsync(cancellationToken);

        return ResponseModel<MarkReadResultDto>.Success(new MarkReadResultDto
        {
            Success = true,
            UnreadCountAfter = 0
        });
    }
}
