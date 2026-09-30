using Notification.Application.DTOs;

namespace Notification.Application.Common.Interfaces;

/// <summary>
/// Đẩy thông báo realtime tới client qua SignalR hub.
/// Application layer chỉ biết interface này — SignalR nằm ở Infrastructure/API.
/// </summary>
public interface IRealTimeNotifier
{
    /// <summary>Gửi tới đúng 1 user (group user-{userId}).</summary>
    Task NotifyUserAsync(object payload, CancellationToken cancellationToken = default);

    /// <summary>Gửi tới tất cả Admin đang kết nối (group admins) — dùng cho hàng chờ duyệt.</summary>
    Task NotifyAdminsAsync(object payload, CancellationToken cancellationToken = default);
}
