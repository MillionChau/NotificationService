using Microsoft.EntityFrameworkCore;
using Notification.Domain.Contracts;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Repositories;

/// <summary>
/// Repository Notification — trả tuple thay vì IQueryable để giữ biên Application↔Infrastructure rõ ràng.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
        => await _context.Notifications.AddAsync(notification, cancellationToken);

    public async Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetByRecipientAsync(
        Guid recipientId, int page, int pageSize, bool? isRead = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == recipientId);

        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken = default)
        => await _context.Notifications
            .CountAsync(n => n.RecipientId == recipientId && !n.IsRead, cancellationToken);

    public async Task<IReadOnlyList<NotificationEntity>> GetUnreadByRecipientAsync(Guid recipientId, CancellationToken cancellationToken = default)
        => await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync(cancellationToken);

    public async Task<NotificationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public void AddRange(IEnumerable<NotificationEntity> notifications)
        => _context.Notifications.AddRange(notifications);

    public async Task MarkPendingReportResolvedAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var pendings = await _context.Notifications
            .Where(n => n.Type == NotificationType.PendingReport
                        && !n.IsRead
                        && n.RelatedEntityId == reportId)
            .ToListAsync(cancellationToken);

        foreach (var n in pendings)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    // --- Admin pending queue (PendingReport chưa đọc, cũ nhất trước) ---

    public async Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetPendingForAdminAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.Type == NotificationType.PendingReport && !n.IsRead);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountPendingForAdminAsync(CancellationToken cancellationToken = default)
        => await _context.Notifications
            .CountAsync(n => n.Type == NotificationType.PendingReport && !n.IsRead, cancellationToken);

    public async Task<DateTime?> GetOldestPendingCreatedAtAsync(CancellationToken cancellationToken = default)
        => await _context.Notifications
            .Where(n => n.Type == NotificationType.PendingReport && !n.IsRead)
            .OrderBy(n => n.CreatedAt)
            .Select(n => n.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
