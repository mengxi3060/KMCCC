using MinecraftLauncher.Core.Domain.Entities;

namespace MinecraftLauncher.Core.DTOs.Comment;

public class AddCommentRequest
{
    public Guid ResourceId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; }
}

public class ReportReason
{
    public string Reason { get; set; }
    public string? Description { get; set; }
}
