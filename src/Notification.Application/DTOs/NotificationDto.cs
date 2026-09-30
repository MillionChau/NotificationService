using Notification.Domain.Enums;

namespace Notification.Application.DTOs;

/// <summary>Thông báo hiển thị cho người nhận.</summary>
public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

/// <summary>Đếm thông báo chưa đọc của user — badge trên navbar.</summary>
public class UnreadCountDto
{
    public int UnreadCount { get; set; }
}

/// <summary>Kết quả đánh dấu đã đọc.</summary>
public class MarkReadResultDto
{
    public bool Success { get; set; }
    public int UnreadCountAfter { get; set; }
}

/// <summary>Báo cáo pending trong hàng chờ duyệt của Admin.</summary>
public class PendingReportDto
{
    public Guid ReportId { get; set; }
    public Guid PostId { get; set; }
    public Guid ReporterId { get; set; }

    /// <summary>Id của notification trong queue — dùng khi Admin bấm "mở" để mark read.</summary>
    public Guid NotificationId { get; set; }

    public string Content { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Số báo cáo đang chờ duyệt — badge cho Admin.</summary>
public class PendingReportCountDto
{
    public int PendingCount { get; set; }
    public DateTime? OldestPendingAt { get; set; }
}
