using MediatR;
using FluentValidation;
using Notification.Application.Common.Exceptions;
using Notification.Application.Common.Interfaces;
using Notification.Application.Common.Models;
using Notification.Application.DTOs;
using Notification.Domain.Contracts;

namespace Notification.Application.Features.Notifications.Commands.MarkAsRead;

/// <summary>Đánh dấu một thông báo là đã đọc (FR-32) — chỉ người nhận mới được phép.</summary>
public record MarkAsReadCommand(Guid Id) : IRequest<ResponseModel<MarkReadResultDto>>;

public class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, ResponseModel<MarkReadResultDto>>
{
    private readonly INotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public MarkAsReadCommandHandler(INotificationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ResponseModel<MarkReadResultDto>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return ResponseModel<MarkReadResultDto>.Error("Unauthorized");

        var notification = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (notification == null)
            throw new NotFoundException(nameof(Notification.Domain.Entities.Notification), request.Id);

        if (notification.RecipientId != userId)
            throw new ForbiddenException("Bạn chỉ được đọc thông báo của chính mình.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);
        }

        var unreadAfter = await _repository.CountUnreadAsync(userId, cancellationToken);
        return ResponseModel<MarkReadResultDto>.Success(new MarkReadResultDto
        {
            Success = true,
            UnreadCountAfter = unreadAfter
        });
    }
}
