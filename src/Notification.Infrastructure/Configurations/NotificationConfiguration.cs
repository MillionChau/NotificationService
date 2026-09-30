using NotificationEntity = Notification.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Content)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.LinkUrl)
            .HasMaxLength(500);

        builder.Property(n => n.SourceService)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.ActorName)
            .HasMaxLength(200);

        builder.Property(n => n.Type)
            .HasConversion<int>();

        // Truy vấn chính: thông báo của user, mới nhất trước (FR-30)
        builder.HasIndex(n => new { n.RecipientId, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_Recipient_CreatedAt");

        // Badge unread nhanh
        builder.HasIndex(n => new { n.RecipientId, n.IsRead })
            .HasDatabaseName("IX_Notifications_Recipient_IsRead");

        // Hàng chờ Admin: chỉ lấy PendingReport chưa đọc, cũ nhất trước
        builder.HasIndex(n => new { n.Type, n.IsRead, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_Type_IsRead_CreatedAt")
            .HasFilter("\"IsRead\" = false AND \"Type\" = 100");
    }
}
