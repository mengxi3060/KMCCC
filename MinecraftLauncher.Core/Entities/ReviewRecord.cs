namespace MinecraftLauncher.Core.Entities
{
    public enum ReviewAction
    {
        Submitted,
        Approved,
        Rejected,
        Suspended,
        Restored,
        Updated
    }

    public class ReviewRecord
    {
        public Guid Id { get; set; }
        public Guid ResourceId { get; set; }
        public Guid ReviewerId { get; set; }
        public ReviewAction Action { get; set; }
        public string? Comments { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Resource? Resource { get; set; }
        public User? Reviewer { get; set; }
    }
}
