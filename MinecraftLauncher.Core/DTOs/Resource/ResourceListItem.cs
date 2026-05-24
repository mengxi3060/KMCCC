using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class ResourceListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string AuthorName { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CompatibilityInfo> Compatibilities { get; set; } = new();
}

public class CompatibilityInfo
{
    public string GameVersion { get; set; }
    public LoaderType? LoaderType { get; set; }
    public bool IsVerified { get; set; }
}

public class CommentInfo
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserAvatar { get; set; }
    public string Content { get; set; }
    public int LikeCount { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum SortBy
{
    Newest,
    Popular,
    Downloads,
    Rating
}
