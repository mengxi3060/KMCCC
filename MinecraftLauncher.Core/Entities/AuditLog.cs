namespace MinecraftLauncher.Core.Entities
{
    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Approve,
        Reject,
        Suspend,
        Restore,
        Login,
        Logout,
        PasswordChange,
        EmailChange,
        RoleChange,
        Ban,
        Unban,
        Other
    }

    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
