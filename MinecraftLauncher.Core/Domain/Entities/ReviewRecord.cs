using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class ReviewRecord
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid ReviewerId { get; set; }
    public ReviewAction Action { get; set; }
    public string? Comment { get; set; }
    public string? CheckResults { get; set; }
    public DateTime CreatedAt { get; set; }

    public Resource Resource { get; set; }
    public User Reviewer { get; set; }
}
