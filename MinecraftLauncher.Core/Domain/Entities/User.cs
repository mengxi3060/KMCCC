using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public int ViolationCount { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public UserProfile Profile { get; set; }
    public ICollection<Resource> Resources { get; set; }
    public ICollection<Comment> Comments { get; set; }
}
