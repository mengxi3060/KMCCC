namespace MinecraftLauncher.Core.Entities
{
    public enum CommentStatus
    {
        Active,
        Hidden,
        Deleted
    }

    public class Comment
    {
        public Guid Id { get; set; }
        public Guid ResourceId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public CommentStatus Status { get; set; } = CommentStatus.Active;
        public int LikeCount { get; set; } = 0;
        public int DislikeCount { get; set; } = 0;
        public bool IsEdited { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Resource? Resource { get; set; }
        public User? User { get; set; }
        public Comment? Parent { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
