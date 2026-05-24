using MinecraftLauncher.Core.DTOs.Resource;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IResourceUploadService
{
    Task<UploadInitResult> InitializeUpload(UploadRequest request);
    Task<UploadProgress> GetUploadProgress(string uploadId);
    Task<UploadCompleteResult> CompleteUpload(string uploadId);
    Task<bool> CancelUpload(string uploadId);
}
