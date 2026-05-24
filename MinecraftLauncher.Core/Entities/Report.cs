namespace MinecraftLauncher.Core.Entities
{
    public enum ReportType
    {
        Inappropriate,
        Spam,
        Copyright,
        Malicious,
        Broken,
        Other
    }

    public enum ReportStatus
    {
        Pending,
        Investigating,
        Resolved,
        Dismissed
    }

    public class Report
    {
        public Guid Id { get; set; }
        public Guid ReporterId { get; set; }
        public Guid ResourceId { get; set; }
        public ReportType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public Guid? HandlerId { get; set; }
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        public User? Reporter { get; set; }
        public Resource? Resource { get; set; }
        public User? Handler { get; set; }
    }
}
