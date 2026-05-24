using MinecraftLauncher.Core.Models;

namespace MinecraftLauncher.Core.Services.Launch
{
    public interface ILaunchService
    {
        Task<LaunchResult> LaunchGame(LaunchOptions options);
        Task<LaunchResult> LaunchWithOfflineAuth(string versionId, string playerName);
        Task<LaunchResult> LaunchWithYggdrasilAuth(string versionId, string email, string password);
        IEnumerable<Models.Version> GetAllVersions();
        Models.Version? GetVersion(string versionId);
    }
}
