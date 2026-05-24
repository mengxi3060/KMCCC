using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid ReporterId { get; set; }
    public ReportType Type { get; set; }
    public string Description { get; set; }
    public string? EvidenceUrls { get; set; }
    public ReportStatus Status { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Resource Resource { get; set; }
    public User Reporter { get; set; }
}
