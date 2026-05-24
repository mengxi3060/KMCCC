namespace MinecraftLauncher.Core.DTOs.Upload;

public enum UploadStatus
{
    Initialized,
    Uploading,
    Validating,
    Completed,
    Failed,
    Cancelled,
    NotFound
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class MalwareScanResult
{
    public bool IsClean { get; set; }
    public string? Threat { get; set; }
}
