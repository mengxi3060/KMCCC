namespace MinecraftLauncher.Core.DTOs.Launch;

public class LaunchResult
{
    public bool Success { get; set; }
    public int ProcessId { get; set; }
    public string? Error { get; set; }
    public DateTime StartedAt { get; set; }
}
