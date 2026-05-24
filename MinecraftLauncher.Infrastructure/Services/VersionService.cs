using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Infrastructure.Services;

public class VersionService : IVersionService
{
    private List<GameVersion> _versions = new();

    public VersionService()
    {
        _versions = GetDefaultVersions();
    }

    public Task<IEnumerable<GameVersion>> GetInstalledVersions()
    {
        return Task.FromResult(_versions.AsEnumerable());
    }

    public Task<GameVersion> GetVersionById(string versionId)
    {
        var v = _versions.FirstOrDefault(v => v.Id == versionId);
        return Task.FromResult(v ?? new GameVersion
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
        if (_versions.Count == 0)
        {
            _versions = GetDefaultVersions();
        }
        return Task.FromResult(_versions.Count > 0);
    }

    private static List<GameVersion> GetDefaultVersions()
    {
        var now = DateTime.UtcNow;
        return new List<GameVersion>
        {
            new() { Id = "1.21.4", Name = "1.21.4", IsValid = true, InstallDate = now, Size = 312_000_000 },
            new() { Id = "1.21.3", Name = "1.21.3", IsValid = true, InstallDate = now, Size = 308_000_000 },
            new() { Id = "1.21.2", Name = "1.21.2", IsValid = true, InstallDate = now, Size = 305_000_000 },
            new() { Id = "1.21.1", Name = "1.21.1", IsValid = true, InstallDate = now, Size = 300_000_000 },
            new() { Id = "1.21", Name = "1.21", IsValid = true, InstallDate = now, Size = 298_000_000 },
            new() { Id = "1.20.6", Name = "1.20.6", IsValid = true, InstallDate = now, Size = 290_000_000 },
            new() { Id = "1.20.4", Name = "1.20.4", IsValid = true, InstallDate = now, Size = 285_000_000 },
            new() { Id = "1.20.2", Name = "1.20.2", IsValid = true, InstallDate = now, Size = 280_000_000 },
            new() { Id = "1.20.1", Name = "1.20.1", IsValid = true, InstallDate = now, Size = 278_000_000 },
            new() { Id = "1.20", Name = "1.20", IsValid = true, InstallDate = now, Size = 275_000_000 },
            new() { Id = "1.19.4", Name = "1.19.4", IsValid = true, InstallDate = now, Size = 260_000_000 },
            new() { Id = "1.19.2", Name = "1.19.2", IsValid = true, InstallDate = now, Size = 255_000_000 },
            new() { Id = "1.18.2", Name = "1.18.2", IsValid = true, InstallDate = now, Size = 240_000_000 },
            new() { Id = "1.17.1", Name = "1.17.1", IsValid = true, InstallDate = now, Size = 230_000_000 },
            new() { Id = "1.16.5", Name = "1.16.5", IsValid = true, InstallDate = now, Size = 220_000_000 },
            new() { Id = "1.12.2", Name = "1.12.2", IsValid = true, InstallDate = now, Size = 190_000_000 },
            new() { Id = "1.8.9", Name = "1.8.9", IsValid = true, InstallDate = now, Size = 150_000_000 },
        };
    }
}
