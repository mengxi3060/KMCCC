namespace MinecraftLauncher.Core.DTOs.Review;

public class ReviewComment
{
    public string? Message { get; set; }
    public Dictionary<string, bool>? CheckResults { get; set; }
}

public class RejectReason
{
    public string? Reason { get; set; }
    public List<string>? Details { get; set; }
}

public class FreezeReason
{
    public string? Reason { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ReviewActionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? ResourceId { get; set; }
}

public class ReviewLog
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerUsername { get; set; }
    public string Action { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
