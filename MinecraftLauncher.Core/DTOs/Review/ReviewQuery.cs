using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Review;

public class ReviewQuery
{
    public ReviewStatus? Status { get; set; }
    public ResourceType? Type { get; set; }
    public string? Keyword { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReviewQueueResult
{
    public IEnumerable<ReviewQueueItem> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public class ReviewQueueItem
{
    public Guid ResourceId { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string AuthorName { get; set; }
    public int AuthorViolationCount { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; }
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CompatibilityInfo> Compatibilities { get; set; }
}
