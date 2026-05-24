using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class Resource
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public Guid AuthorId { get; set; }
    public string Description { get; set; }
    public string? Tags { get; set; }
    public string? Screenshots { get; set; }
    public string? Copyright { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? FileHash { get; set; }
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }
    public ResourceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool IsTopped { get; set; }
    public bool IsRecommended { get; set; }

    public User Author { get; set; }
    public ICollection<ResourceCompatibility> Compatibilities { get; set; }
    public ICollection<Comment> Comments { get; set; }
}
