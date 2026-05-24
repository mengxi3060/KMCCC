using KMCCC.Launcher;
using KMCCC.Authentication;
using MinecraftLauncher.Core.Services.Launch;
using MinecraftLauncher.Core.Models;

namespace MinecraftLauncher.Infrastructure.Services.Launch
{
    public class KMCCCLaunchService : ILaunchService
    {
        private readonly LauncherCore _launcherCore;
        private readonly string _gameRootPath;
        
        public KMCCCLaunchService(string gameRootPath, string javaPath)
        {
            _gameRootPath = gameRootPath;
            var options = new LauncherCoreCreationOption
            {
                GameRootPath = gameRootPath,
                JavaPath = javaPath
            };
            _launcherCore = LauncherCore.Create(options);
            
            _launcherCore.GameExit += OnGameExit;
            _launcherCore.GameLog += OnGameLog;
        }
        
        private void OnGameExit(LaunchHandle handle, int exitCode)
        {
            Console.WriteLine($"游戏已退出,退出码: {exitCode}");
        }
        
        private void OnGameLog(LaunchHandle handle, string log)
        {
            Console.WriteLine($"[Minecraft] {log}");
        }
        
        public async Task<LaunchResult> LaunchGame(LaunchOptions options)
        {
            var version = _launcherCore.GetVersion(options.VersionId);
            if (version == null)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorType = LaunchErrorType.InvalidVersion,
                    ErrorMessage = $"找不到版本: {options.VersionId}"
                };
            }
            
            var launchOptions = new KMCCC.Launcher.LaunchOptions
            {
                Version = version,
                Authenticator = options.Authenticator as IAuthenticator,
                MaxMemory = options.MaxMemory,
                MinMemory = options.MinMemory,
                Server = options.Server != null ? new KMCCC.Launcher.ServerInfo 
                { 
                    Address = options.Server.Address, 
                    Port = options.Server.Port 
                } : null,
                Size = options.Size != null ? new KMCCC.Launcher.WindowSize 
                { 
                    Width = options.Size.Width, 
                    Height = options.Size.Height,
                    FullScreen = options.Size.FullScreen
                } : null
            };
            
            var result = _launcherCore.Launch(launchOptions);
            
            return await Task.FromResult(new LaunchResult
            {
                Success = result.Success,
                ErrorType = ConvertErrorType(result.ErrorType),
                ErrorMessage = result.ErrorMessage,
                Handle = new LaunchHandleInfo 
                { 
                    ProcessId = result.Handle?.Id ?? 0,
                    IsRunning = result.Handle?.IsRunning ?? false
                }
            });
        }
        
        public async Task<LaunchResult> LaunchWithOfflineAuth(string versionId, string playerName)
        {
            var authenticator = new OfflineAuthenticator(playerName);
            return await LaunchGame(new LaunchOptions
            {
                VersionId = versionId,
                Authenticator = authenticator,
                MaxMemory = 2048
            });
        }
        
        public async Task<LaunchResult> LaunchWithYggdrasilAuth(string versionId, string email, string password)
        {
            try
            {
                var authenticator = new YggdrasilLogin(email, password, false);
                return await LaunchGame(new LaunchOptions
                {
                    VersionId = versionId,
                    Authenticator = authenticator,
                    MaxMemory = 2048
                });
            }
            catch (Exception ex)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorType = LaunchErrorType.AuthenticationFailed,
                    ErrorMessage = $"正版登录失败: {ex.Message}"
                };
            }
        }
        
        public IEnumerable<Models.Version> GetAllVersions()
        {
            var versions = _launcherCore.GetVersions();
            return versions.Select(v => new Models.Version
            {
                Id = v.Id,
                Name = v.Name,
                GameRootPath = v.GameRootPath,
                JarPath = v.JarPath,
                JsonPath = v.JsonPath,
                Size = v.Size,
                LastModified = v.LastModified,
                IsVirtual = v.IsVirtual,
                IsIntegrityCheckResult = v.IsIntegrityCheckResult,
                Libraries = v.Libraries
            });
        }
        
        public Models.Version? GetVersion(string versionId)
        {
            var version = _launcherCore.GetVersion(versionId);
            if (version == null)
                return null;
                
            return new Models.Version
            {
                Id = version.Id,
                Name = version.Name,
                GameRootPath = version.GameRootPath,
                JarPath = version.JarPath,
                JsonPath = version.JsonPath,
                Size = version.Size,
                LastModified = version.LastModified,
                IsVirtual = version.IsVirtual,
                IsIntegrityCheckResult = version.IsIntegrityCheckResult,
                Libraries = version.Libraries
            };
        }
        
        private LaunchErrorType ConvertErrorType(KMCCC.Launcher.ErrorType errorType)
        {
            return errorType switch
            {
                KMCCC.Launcher.ErrorType.NoJavaFound => LaunchErrorType.NoJavaFound,
                KMCCC.Launcher.ErrorType.AuthenticationFailed => LaunchErrorType.AuthenticationFailed,
                KMCCC.Launcher.ErrorType.InvalidVersion => LaunchErrorType.InvalidVersion,
                KMCCC.Launcher.ErrorType.InsufficientMemory => LaunchErrorType.InsufficientMemory,
                KMCCC.Launcher.ErrorType.GameCrashed => LaunchErrorType.GameCrashed,
                _ => LaunchErrorType.Unknown
            };
        }
    }
}
