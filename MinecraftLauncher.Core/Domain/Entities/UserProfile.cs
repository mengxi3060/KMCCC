namespace MinecraftLauncher.Core.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Bio { get; set; }
    public int UploadCount { get; set; }
    public int DownloadCount { get; set; }

    public User User { get; set; }
}
