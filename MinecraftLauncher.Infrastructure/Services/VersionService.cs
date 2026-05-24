using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Infrastructure.Services;

public class VersionService : IVersionService
{
    public Task<IEnumerable<GameVersion>> GetInstalledVersions()
    {
        return Task.FromResult<IEnumerable<GameVersion>>(new List<GameVersion>());
    }

    public Task<GameVersion> GetVersionById(string versionId)
    {
        return Task.FromResult(new GameVersion
        {
            Id = versionId,
            Name = versionId,
            GameRootPath = string.Empty,
            InstallDate = DateTime.UtcNow,
            Size = 0,
            IsValid = false
        });
    }

    public Task<bool> ScanVersions(string gameRootPath)
    {
        return Task.FromResult(true);
    }
}
