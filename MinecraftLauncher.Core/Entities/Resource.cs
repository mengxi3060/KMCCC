using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Entities
{
    public class Resource
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ResourceType Type { get; set; }
        public Guid AuthorId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Tags { get; set; }
        public string? Screenshots { get; set; }
        public string? Copyright { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public string? FileHash { get; set; }
        public ResourceStatus Status { get; set; } = ResourceStatus.Pending;
        public int DownloadCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public bool IsTopped { get; set; } = false;
        public bool IsRecommended { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        
        public User? Author { get; set; }
        public ICollection<ResourceCompatibility> Compatibilities { get; set; } = new List<ResourceCompatibility>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<ReviewRecord> ReviewRecords { get; set; } = new List<ReviewRecord>();
    }
}
