using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

/// <summary>
/// Một thông báo gửi tới một user (SRS FR-29 → FR-34: nhận/xem/đọc/xóa thông báo).
/// Loại thông báo + dữ liệu đính kèm (link điều hướng client) + trạng thái đã đọc.
/// </summary>
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>User nhận thông báo.</summary>
    public Guid RecipientId { get; set; }

    /// <summary>Loại thông báo (like/comment/reply/share/report...).</summary>
    public NotificationType Type { get; set; }

    /// <summary>Nội dung hiển thị — đặt bởi service gửi (có ngữ cảnh).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Đường dẫn client điều hướng khi bấm vào thông báo (vd /post/{id}).</summary>
    public string? LinkUrl { get; set; }

    /// <summary>Service phát sinh sự kiện (community / identity / quality...).</summary>
    public string SourceService { get; set; } = string.Empty;

    /// <summary>Id đối tượng chính liên quan (postId / commentId / reportId) — để dedupe.</summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>Bài viết liên quan (khi type là like/comment/reply/share/report trên bài).</summary>
    public Guid? RelatedPostId { get; set; }

    /// <summary>User thực hiện hành động (người like / người comment / người report...).</summary>
    public Guid? ActorId { get; set; }

    /// <summary>Tên hiển thị của actor — lưu sẵn để client không phải gọi service khác.</summary>
    public string? ActorName { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời điểm đọc — null nếu chưa đọc.</summary>
    public DateTime? ReadAt { get; set; }
}
