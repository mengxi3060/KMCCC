using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public NotificationType Type { get; set; }
    public string? RelatedId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}
