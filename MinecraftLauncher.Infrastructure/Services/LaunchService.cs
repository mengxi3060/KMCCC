using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Infrastructure.Services;

public class LaunchService : ILaunchService
{
    public Task<LaunchResult> LaunchGame(LaunchOptions options)
    {
        return Task.FromResult(new LaunchResult
        {
            Success = false,
            Error = "游戏启动功能尚未实现",
            StartedAt = DateTime.UtcNow
        });
    }

    public Task<LaunchResult> LaunchWithOfflineAuth(string versionId, string playerName)
    {
        return Task.FromResult(new LaunchResult
        {
            Success = false,
            Error = "游戏启动功能尚未实现",
            StartedAt = DateTime.UtcNow
        });
    }

    public Task<LaunchResult> LaunchWithYggdrasilAuth(string versionId, string email, string password)
    {
        return Task.FromResult(new LaunchResult
        {
            Success = false,
            Error = "游戏启动功能尚未实现",
            StartedAt = DateTime.UtcNow
        });
    }
}
