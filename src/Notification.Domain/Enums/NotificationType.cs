namespace Notification.Domain.Enums;

/// <summary>
/// Loại thông báo — bám theo các use case tương tác trực tiếp với người dùng
/// (SRS CommunityService: like, comment, reply, share, report) + thông báo hệ thống.
/// </summary>
public enum NotificationType
{
    /// <summary>Có người like bài viết của bạn (FR-19).</summary>
    PostLiked = 1,

    /// <summary>Có người bình luận bài viết của bạn (FR-24).</summary>
    PostCommented = 2,

    /// <summary>Bình luận của bạn được trả lời (FR-25).</summary>
    CommentReplied = 3,

    /// <summary>Bài viết của bạn được chia sẻ (FR-22).</summary>
    PostShared = 4,

    /// <summary>Báo cáo vi phạm của bạn đã được xử lý (UC-23).</summary>
    ReportReviewed = 5,

    /// <summary>Bài viết của bạn bị ẩn do vi phạm / duyệt báo cáo.</summary>
    PostHidden = 6,

    /// <summary>Loại dự phòng cho thông báo hệ thống (chào mừng, bảo trì...).</summary>
    System = 99,

    /// <summary>Báo cáo vi phạm chờ Admin duyệt — hàng chờ quản trị (UC-23).</summary>
    PendingReport = 100
}
