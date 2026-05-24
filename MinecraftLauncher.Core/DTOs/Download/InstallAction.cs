namespace MinecraftLauncher.Core.DTOs.Download;

public enum InstallAction
{
    Copy,
    Backup,
    Remove,
    Extract
}

public class InstalledFile
{
    public string SourcePath { get; set; }
    public string TargetPath { get; set; }
    public InstallAction Action { get; set; }
    public long FileSize { get; set; }
    public DateTime Timestamp { get; set; }
}
