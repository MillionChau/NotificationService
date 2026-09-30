using Notification.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Persistence;

public class NotificationDbContext : DbContext, INotificationDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}

/// <summary>Marker interface cho repository dùng chung DbContext.</summary>
public interface INotificationDbContext
{
    DbSet<NotificationEntity> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
