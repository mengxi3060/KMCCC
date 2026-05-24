using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class Violation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ViolationType Type { get; set; }
    public Guid? ResourceId { get; set; }
    public string Description { get; set; }
    public ViolationSeverity Severity { get; set; }
    public Guid HandledBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
    public User Handler { get; set; }
}
