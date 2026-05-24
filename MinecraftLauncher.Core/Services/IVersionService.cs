using MinecraftLauncher.Core.Models;

namespace MinecraftLauncher.Core.Services
{
    public interface IVersionService
    {
        Task<IEnumerable<GameVersion>> GetInstalledVersions();
        Task<GameVersion> GetVersionById(string versionId);
        Task<bool> ScanVersions(string gameRootPath);
    }
}
