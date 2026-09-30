using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Domain.Contracts;

public interface INotificationRepository
{
    Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);

    /// <summary>Danh sách thông báo của user, mới nhất trước, phân trang.</summary>
    Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetByRecipientAsync(
        Guid recipientId, int page, int pageSize, bool? isRead = null, CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken = default);

    /// <summary>Tất cả thông báo chưa đọc của user — dùng cho "đánh dấu tất cả đã đọc".</summary>
    Task<IReadOnlyList<NotificationEntity>> GetUnreadByRecipientAsync(Guid recipientId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Thêm nhiều thông báo cùng lúc (batch ingestion từ service khác).</summary>
    void AddRange(IEnumerable<NotificationEntity> notifications);

    /// <summary>Admin đã xử lý báo cáo → đánh dấu mọi PendingReport của report này là đã đọc (rời hàng chờ).</summary>
    Task MarkPendingReportResolvedAsync(Guid reportId, CancellationToken cancellationToken = default);

    Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Thông báo pending duyệt cho Admin (PendingReport), cũ nhất lên đầu.</summary>
    Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetPendingForAdminAsync(
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountPendingForAdminAsync(CancellationToken cancellationToken = default);

    /// <summary>Báo cáo pending cũ nhất (dùng làm oldest trong payload queue count).</summary>
    Task<DateTime?> GetOldestPendingCreatedAtAsync(CancellationToken cancellationToken = default);
}
