using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IVersionService
{
    Task<IEnumerable<GameVersion>> GetInstalledVersions();
    Task<GameVersion> GetVersionById(string versionId);
    Task<bool> ScanVersions(string gameRootPath);
}
