using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Infrastructure.Services;

public class VersionService : IVersionService
{
    private List<GameVersion> _versions = new();

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
        if (string.IsNullOrEmpty(gameRootPath) || !Directory.Exists(gameRootPath))
            return Task.FromResult(false);

        var versionsDir = Path.Combine(gameRootPath, "versions");
        if (!Directory.Exists(versionsDir))
            return Task.FromResult(false);

        _versions.Clear();
        foreach (var dir in Directory.GetDirectories(versionsDir))
        {
            var name = Path.GetFileName(dir);
            var jarFile = Path.Combine(dir, $"{name}.jar");
            var jsonFile = Path.Combine(dir, $"{name}.json");

            if (File.Exists(jarFile) && File.Exists(jsonFile))
            {
                var fi = new FileInfo(jarFile);
                _versions.Add(new GameVersion
                {
                    Id = name,
                    Name = name,
                    GameRootPath = gameRootPath,
                    InstallDate = fi.LastWriteTimeUtc,
                    Size = fi.Length,
                    IsValid = true
                });
            }
        }

        return Task.FromResult(_versions.Count > 0);
    }
}
