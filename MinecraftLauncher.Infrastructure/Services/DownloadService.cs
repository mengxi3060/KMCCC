using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.DTOs.Download;
using MinecraftLauncher.Core.DTOs.Resource;
using MinecraftLauncher.Core.Entities;
using MinecraftLauncher.Core.Interfaces;
using MinecraftLauncher.Infrastructure.Data;

namespace MinecraftLauncher.Infrastructure.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DownloadService> _logger;
        private readonly string _gameRootPath;
        
        public DownloadService(
            AppDbContext context,
            ILogger<DownloadService> logger,
            string gameRootPath = ".minecraft")
        {
            _context = context;
            _logger = logger;
            _gameRootPath = gameRootPath;
        }
        
        public async Task<DownloadInfo> GetDownloadInfo(Guid resourceId)
        {
            var resource = await _context.Resources
                .Include(r => r.Author)
                .FirstOrDefaultAsync(r => r.Id == resourceId && r.Status == Core.Domain.Enums.ResourceStatus.Approved);
            
            if (resource == null)
            {
                return null;
            }
            
            return new DownloadInfo
            {
                ResourceId = resource.Id,
                ResourceName = resource.Name,
                FilePath = resource.FilePath,
                FileSize = resource.FileSize,
                FileHash = resource.FileHash,
                AuthorName = resource.Author.Profile?.DisplayName ?? resource.Author.Username,
                DownloadCount = resource.DownloadCount
            };
        }
        
        public async Task<DownloadResult> DownloadResource(Guid resourceId, Guid userId, string targetPath)
        {
            var resource = await _context.Resources.FindAsync(resourceId);
            
            if (resource == null || resource.Status != Core.Domain.Enums.ResourceStatus.Approved)
            {
                return new DownloadResult
                {
                    Success = false,
                    Message = "Resource not found or not available"
                };
            }
            
            if (string.IsNullOrEmpty(resource.FilePath) || !File.Exists(resource.FilePath))
            {
                return new DownloadResult
                {
                    Success = false,
                    Message = "Resource file not found"
                };
            }
            
            try
            {
                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                
                File.Copy(resource.FilePath, targetPath, overwrite: true);
                
                resource.DownloadCount++;
                
                var profile = await _context.UserProfiles.FindAsync(userId);
                if (profile != null)
                {
                    profile.DownloadCount++;
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Resource downloaded: {resourceId} by user {userId}");
                
                return new DownloadResult
                {
                    Success = true,
                    Message = "Download completed successfully",
                    FilePath = targetPath,
                    FileSize = new FileInfo(targetPath).Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to download resource: {resourceId}");
                
                return new DownloadResult
                {
                    Success = false,
                    Message = $"Download failed: {ex.Message}"
                };
            }
        }
        
        public async Task<InstallResult> InstallResource(Guid resourceId, Guid userId, string gameRootPath)
        {
            var resource = await _context.Resources
                .Include(r => r.Compatibilities)
                .FirstOrDefaultAsync(r => r.Id == resourceId && r.Status == Core.Domain.Enums.ResourceStatus.Approved);
            
            if (resource == null)
            {
                return new InstallResult
                {
                    Success = false,
                    Message = "Resource not found or not available"
                };
            }
            
            try
            {
                var installedFiles = new List<InstalledFile>();
                
                var targetDir = GetTargetDirectory(gameRootPath, resource.Type);
                Directory.CreateDirectory(targetDir);
                
                var targetFileName = $"{resource.Name}{Path.GetExtension(resource.FilePath)}";
                var targetFilePath = Path.Combine(targetDir, targetFileName);
                
                if (File.Exists(targetFilePath))
                {
                    var backupPath = targetFilePath + $".backup_{DateTime.UtcNow:yyyyMMddHHmmss}";
                    File.Move(targetFilePath, backupPath);
                    
                    installedFiles.Add(new InstalledFile
                    {
                        SourcePath = targetFilePath,
                        TargetPath = backupPath,
                        Action = InstallAction.Backup,
                        FileSize = new FileInfo(backupPath).Length,
                        Timestamp = DateTime.UtcNow
                    });
                }
                
                File.Copy(resource.FilePath, targetFilePath, overwrite: true);
                
                installedFiles.Add(new InstalledFile
                {
                    SourcePath = resource.FilePath,
                    TargetPath = targetFilePath,
                    Action = InstallAction.Copy,
                    FileSize = new FileInfo(targetFilePath).Length,
                    Timestamp = DateTime.UtcNow
                });
                
                resource.DownloadCount++;
                var profile = await _context.UserProfiles.FindAsync(userId);
                if (profile != null)
                {
                    profile.DownloadCount++;
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Resource installed: {resourceId} to {targetFilePath}");
                
                return new InstallResult
                {
                    Success = true,
                    Message = "Resource installed successfully",
                    InstalledPath = targetFilePath,
                    InstalledFiles = installedFiles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to install resource: {resourceId}");
                
                return new InstallResult
                {
                    Success = false,
                    Message = $"Installation failed: {ex.Message}"
                };
            }
        }
        
        private string GetTargetDirectory(string gameRootPath, Core.Domain.Enums.ResourceType type)
        {
            return type switch
            {
                Core.Domain.Enums.ResourceType.Mod => Path.Combine(gameRootPath, "mods"),
                Core.Domain.Enums.ResourceType.Shader => Path.Combine(gameRootPath, "shaderpacks"),
                Core.Domain.Enums.ResourceType.TexturePack => Path.Combine(gameRootPath, "resourcepacks"),
                Core.Domain.Enums.ResourceType.Modpack => Path.Combine(gameRootPath, "modpacks"),
                _ => Path.Combine(gameRootPath, "resources")
            };
        }
    }
}
