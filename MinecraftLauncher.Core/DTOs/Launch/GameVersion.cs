namespace MinecraftLauncher.Core.DTOs.Launch;

public class GameVersion
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string GameRootPath { get; set; }
    public DateTime InstallDate { get; set; }
    public long Size { get; set; }
    public bool IsValid { get; set; }
}
