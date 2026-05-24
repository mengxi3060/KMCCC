using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class ResourceDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; }
    public string AuthorAvatar { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Screenshots { get; set; } = new();
    public string Copyright { get; set; }
    public long FileSize { get; set; }
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }
    public ResourceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsTopped { get; set; }
    public bool IsRecommended { get; set; }
    public List<CompatibilityInfo> Compatibilities { get; set; } = new();
    public List<CommentInfo> Comments { get; set; } = new();
}
