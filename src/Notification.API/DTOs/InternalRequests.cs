namespace Notification.API.DTOs;

/// <summary>Một thông báo gửi từ service khác vào NotificationService.</summary>
public class NotificationIngestItem
{
    public Guid RecipientId { get; set; }
    public int Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string SourceService { get; set; } = "community";
    public Guid? RelatedEntityId { get; set; }
    public Guid? RelatedPostId { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
}

/// <summary>Batch ingestion — 1 request tạo nhiều thông báo (vd 1 like → notify author, notify group).</summary>
public class IngestNotificationsRequest
{
    public List<NotificationIngestItem> Items { get; set; } = new();
}

/// <summary>Admin đã xử lý báo cáo → gỡ khỏi hàng chờ PendingReport.</summary>
public class ResolvePendingReportRequest
{
    public Guid ReportId { get; set; }
}
