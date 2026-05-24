namespace MinecraftLauncher.Core.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public int LikeCount { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }

    public Resource Resource { get; set; }
    public User User { get; set; }
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; }
}
