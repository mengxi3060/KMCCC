namespace MinecraftLauncher.Core.DTOs.Auth;

public class UserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsBanned { get; set; }
}
