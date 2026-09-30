namespace Notification.Application.Common.Interfaces;

/// <summary>Thông tin user hiện tại từ JWT (đọc bởi CurrentUserService ở tầng Infrastructure).</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    /// <summary>Role đầu tiên trong token (Admin/User) — dùng [Authorize(Roles="Admin")].</summary>
    string? Role { get; }
}
