using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class UploadRequest
{
    public Guid ResourceId { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public ResourceType Type { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Screenshots { get; set; } = new();
    public string Copyright { get; set; }
    public List<CompatibilityInfo> Compatibilities { get; set; } = new();
}

public class UploadInitResult
{
    public string UploadId { get; set; }
    public string UploadUrl { get; set; }
    public int ChunkSize { get; set; }
    public int TotalChunks { get; set; }
}

public class UploadProgress
{
    public string UploadId { get; set; }
    public Core.DTOs.Upload.UploadStatus Status { get; set; }
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
    public double ProgressPercentage { get; set; }
    public int ChunksUploaded { get; set; }
    public int TotalChunks { get; set; }
}

public class UploadCompleteResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileHash { get; set; }
    public long? FileSize { get; set; }
    public string? Error { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
