using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface ILaunchService
{
    Task<LaunchResult> LaunchGame(LaunchOptions options);
    Task<LaunchResult> LaunchWithOfflineAuth(string versionId, string playerName);
    Task<LaunchResult> LaunchWithYggdrasilAuth(string versionId, string email, string password);
}
