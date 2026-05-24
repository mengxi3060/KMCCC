using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Review;

public class ReviewDetail
{
    public Guid ResourceId { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; }
    public string AuthorEmail { get; set; }
    public int AuthorViolationCount { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Screenshots { get; set; }
    public string Copyright { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public ResourceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CompatibilityInfo> Compatibilities { get; set; }
    public List<ReviewHistoryItem> ReviewHistory { get; set; }
}

public class ReviewCheckItem
{
    public string Name { get; set; }
    public bool Passed { get; set; }
    public string? Message { get; set; }
}
