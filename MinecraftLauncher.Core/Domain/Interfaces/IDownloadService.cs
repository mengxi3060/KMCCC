using MinecraftLauncher.Core.DTOs.Resource;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IDownloadService
{
    Task<DownloadInfo> GetDownloadInfo(Guid resourceId);
    Task<DownloadResult> DownloadResource(Guid resourceId, Guid userId, string targetPath);
    Task<InstallResult> InstallResource(Guid resourceId, Guid userId, string gameRootPath);
}
