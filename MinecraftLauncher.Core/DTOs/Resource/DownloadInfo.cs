namespace MinecraftLauncher.Core.DTOs.Resource;

public class DownloadInfo
{
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; }
    public string AuthorName { get; set; }
    public int DownloadCount { get; set; }
}

public class DownloadResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public long? FileSize { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
}

public class InstallResult
{
    public bool Success { get; set; }
    public string? InstalledPath { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public List<Download.InstalledFile>? InstalledFiles { get; set; }
}
