using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Report;

public class ReportListItem
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; }
    public string ReporterName { get; set; }
    public ReportType Type { get; set; }
    public string Description { get; set; }
    public List<string>? EvidenceUrls { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportDetail
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; }
    public string ResourceAuthorName { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; }
    public ReportType Type { get; set; }
    public string Description { get; set; }
    public List<string>? EvidenceUrls { get; set; }
    public ReportStatus Status { get; set; }
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
