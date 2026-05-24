using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Report;

public class ReportRequest
{
    public Guid ResourceId { get; set; }
    public ReportType Type { get; set; }
    public string Description { get; set; }
    public List<string>? EvidenceUrls { get; set; }
}

public class ReportResolution
{
    public ReportStatus Status { get; set; }
    public string? Comment { get; set; }
}
