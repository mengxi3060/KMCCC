namespace MinecraftLauncher.Core.Entities
{
    public class UserProfile
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public int Reputation { get; set; } = 0;
        public int TotalDownloads { get; set; } = 0;
        public int TotalResources { get; set; } = 0;
        public int TotalComments { get; set; } = 0;
        public int TotalLikes { get; set; } = 0;
        public DateTime? LastActiveAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
