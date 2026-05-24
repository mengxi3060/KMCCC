namespace MinecraftLauncher.Core.Entities
{
    public enum NotificationType
    {
        ResourceApproved,
        ResourceRejected,
        ResourceSuspended,
        NewComment,
        NewReply,
        NewReview,
        NewReport,
        ViolationIssued,
        ViolationResolved,
        AccountWarning,
        SystemAnnouncement,
        Other
    }

    public enum NotificationStatus
    {
        Unread,
        Read,
        Archived
    }

    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
