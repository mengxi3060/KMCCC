namespace MinecraftLauncher.Core.DTOs.Review;

public enum ReviewStatus
{
    Pending = 0,
    InReview = 1,
    Approved = 2,
    Rejected = 3,
    Frozen = 4
}

public class CompatibilityInfo
{
    public string GameVersion { get; set; }
    public string LoaderType { get; set; }
    public bool IsVerified { get; set; }
}

public class ReviewHistoryItem
{
    public Guid Id { get; set; }
    public string ReviewerName { get; set; }
    public string Action { get; set; }
    public string? Comment { get; set; }
    public Dictionary<string, bool>? CheckResults { get; set; }
    public DateTime CreatedAt { get; set; }
}
