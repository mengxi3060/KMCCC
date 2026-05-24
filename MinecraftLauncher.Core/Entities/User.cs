namespace MinecraftLauncher.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public DateTime? BanExpiry { get; set; }
        public string? BanReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public UserProfile? Profile { get; set; }
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    }
}
