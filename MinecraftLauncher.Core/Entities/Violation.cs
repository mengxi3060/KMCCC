namespace MinecraftLauncher.Core.Entities
{
    public enum ViolationType
    {
        Warning,
        TemporaryBan,
        PermanentBan,
        ContentRemoval,
        AccountSuspension,
        Other
    }

    public class Violation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ResourceId { get; set; }
        public ViolationType Type { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Details { get; set; }
        public int Severity { get; set; } = 1;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? HandledBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Resource? Resource { get; set; }
        public User? Handler { get; set; }
    }
}
