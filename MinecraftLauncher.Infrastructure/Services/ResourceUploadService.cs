using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.DTOs.Upload;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Infrastructure.Data;
using System.IO.Compression;
using System.Security.Cryptography;

namespace MinecraftLauncher.Infrastructure.Services
{
    public class ResourceUploadService : IResourceUploadService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResourceUploadService> _logger;
        private readonly string _uploadDirectory;
        private readonly Dictionary<string, UploadSession> _uploadSessions;
        
        public ResourceUploadService(
            AppDbContext context,
            ILogger<ResourceUploadService> logger,
            string uploadDirectory = "uploads")
        {
            _context = context;
            _logger = logger;
            _uploadDirectory = uploadDirectory;
            _uploadSessions = new Dictionary<string, UploadSession>();
            
            Directory.CreateDirectory(_uploadDirectory);
        }
        
        public async Task<Core.DTOs.Resource.UploadInitResult> InitializeUpload(Core.DTOs.Resource.UploadRequest request)
        {
            var allowedExtensions = GetAllowedExtensions(request.Type);
            
            var uploadId = Guid.NewGuid().ToString();
            var session = new UploadSession
            {
                UploadId = uploadId,
                Request = request,
                Status = UploadStatus.Initialized,
                CreatedAt = DateTime.UtcNow,
                FilePath = Path.Combine(_uploadDirectory, uploadId),
                TotalChunks = (int)Math.Ceiling(request.FileSize / (double)(5 * 1024 * 1024))
            };
            
            Directory.CreateDirectory(session.FilePath);
            
            _uploadSessions[uploadId] = session;
            
            _logger.LogInformation($"Upload initialized: {uploadId}, FileSize: {request.FileSize}, TotalChunks: {session.TotalChunks}");
            
            return new Core.DTOs.Resource.UploadInitResult
            {
                UploadId = uploadId,
                ChunkSize = 5 * 1024 * 1024,
                TotalChunks = session.TotalChunks
            };
        }
        
        public async Task<Core.DTOs.Resource.UploadProgress> GetUploadProgress(string uploadId)
        {
            if (!_uploadSessions.TryGetValue(uploadId, out var session))
            {
                return new Core.DTOs.Resource.UploadProgress
                {
                    UploadId = uploadId,
                    Status = UploadStatus.NotFound,
                    ProgressPercentage = 0
                };
            }
            
            return new Core.DTOs.Resource.UploadProgress
            {
                UploadId = uploadId,
                Status = session.Status,
                ProgressPercentage = session.TotalChunks > 0 
                    ? session.ChunksUploaded * 100.0 / session.TotalChunks 
                    : 0,
                ChunksUploaded = session.ChunksUploaded,
                TotalChunks = session.TotalChunks,
                BytesUploaded = session.ChunksUploaded * (long)(5 * 1024 * 1024),
                TotalBytes = session.Request.FileSize
            };
        }
        
        public async Task<Core.DTOs.Resource.UploadCompleteResult> CompleteUpload(string uploadId)
        {
            if (!_uploadSessions.TryGetValue(uploadId, out var session))
            {
                return new Core.DTOs.Resource.UploadCompleteResult
                {
                    Success = false,
                    Error = "Upload session not found"
                };
            }
            
            try
            {
                session.Status = UploadStatus.Validating;
                
                var finalFilePath = Path.Combine(_uploadDirectory, $"{uploadId}.zip");
                await MergeChunks(session, finalFilePath);
                
                var validationResult = await ValidateFile(session.Request.Type, finalFilePath);
                if (!validationResult.IsValid)
                {
                    if (File.Exists(finalFilePath))
                        File.Delete(finalFilePath);
                    
                    session.Status = UploadStatus.Failed;
                    
                    return new Core.DTOs.Resource.UploadCompleteResult
                    {
                        Success = false,
                        Error = "File validation failed",
                        ValidationErrors = validationResult.Errors
                    };
                }
                
                var fileHash = await CalculateFileHash(finalFilePath);
                
                var finalFileName = $"{Guid.NewGuid()}{Path.GetExtension(finalFilePath)}";
                var finalPath = Path.Combine(_uploadDirectory, "resources", finalFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
                File.Move(finalFilePath, finalPath);
                
                session.Status = UploadStatus.Completed;
                session.FilePath = finalPath;
                session.FileHash = fileHash;
                
                Directory.Delete(session.FilePath, true);
                
                _logger.LogInformation($"Upload completed: {uploadId}, Hash: {fileHash}");
                
                return new Core.DTOs.Resource.UploadCompleteResult
                {
                    Success = true,
                    FilePath = finalPath,
                    FileSize = new FileInfo(finalPath).Length,
                    FileHash = fileHash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Upload completion failed: {uploadId}");
                session.Status = UploadStatus.Failed;
                
                return new Core.DTOs.Resource.UploadCompleteResult
                {
                    Success = false,
                    Error = $"Upload completion failed: {ex.Message}"
                };
            }
        }
        
        public async Task<bool> CancelUpload(string uploadId)
        {
            if (!_uploadSessions.TryGetValue(uploadId, out var session))
            {
                return false;
            }
            
            try
            {
                if (Directory.Exists(session.FilePath))
                {
                    Directory.Delete(session.FilePath, true);
                }
                
                var finalFilePath = Path.Combine(_uploadDirectory, $"{uploadId}.zip");
                if (File.Exists(finalFilePath))
                {
                    File.Delete(finalFilePath);
                }
                
                _uploadSessions.Remove(uploadId);
                session.Status = UploadStatus.Cancelled;
                
                _logger.LogInformation($"Upload cancelled: {uploadId}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to cancel upload: {uploadId}");
                return false;
            }
        }
        
        private async Task MergeChunks(UploadSession session, string outputPath)
        {
            using var outputStream = new FileStream(outputPath, FileMode.Create);
            
            for (int i = 0; i < session.TotalChunks; i++)
            {
                var chunkPath = Path.Combine(session.FilePath, $"chunk_{i}");
                
                if (!File.Exists(chunkPath))
                {
                    throw new FileNotFoundException($"Missing chunk {i}");
                }
                
                using var chunkStream = File.OpenRead(chunkPath);
                await chunkStream.CopyToAsync(outputStream);
            }
        }
        
        private async Task<FileValidationResult> ValidateFile(Core.Domain.Enums.ResourceType type, string filePath)
        {
            var errors = new List<string>();
            var result = new FileValidationResult { IsValid = true };
            
            var extension = Path.GetExtension(filePath).ToLower();
            var allowedExtensions = GetAllowedExtensions(type);
            
            if (!allowedExtensions.Contains(extension))
            {
                errors.Add($"Invalid file extension. Allowed: {string.Join(", ", allowedExtensions)}");
            }
            
            var fileSize = new FileInfo(filePath).Length;
            var maxSize = GetMaxFileSize(type);
            
            if (fileSize > maxSize)
            {
                errors.Add($"File size exceeds limit. Maximum: {maxSize / (1024 * 1024)}MB");
            }
            
            if (extension == ".zip")
            {
                var zipValidation = await ValidateZipStructure(filePath, type);
                if (!zipValidation.IsValid)
                {
                    errors.AddRange(zipValidation.Errors);
                }
            }
            
            var malwareCheck = await ScanForMalware(filePath);
            if (!malwareCheck.IsClean)
            {
                errors.Add("File contains potentially malicious content");
            }
            
            if (errors.Count > 0)
            {
                result.IsValid = false;
                result.Errors = errors;
            }
            
            return result;
        }
        
        private async Task<FileValidationResult> ValidateZipStructure(string filePath, Core.Domain.Enums.ResourceType type)
        {
            var errors = new List<string>();
            
            try
            {
                using var archive = ZipFile.OpenRead(filePath);
                
                var entryCount = archive.Entries.Count;
                if (entryCount == 0)
                {
                    errors.Add("ZIP archive is empty");
                }
                
                switch (type)
                {
                    case Core.Domain.Enums.ResourceType.Mod:
                        var hasClasses = archive.Entries.Any(e => e.FullName.EndsWith(".class"));
                        var hasManifest = archive.Entries.Any(e => e.FullName == "META-INF/MANIFEST.MF");
                        if (!hasClasses && !hasManifest)
                        {
                            errors.Add("Mod archive should contain .class files or MANIFEST.MF");
                        }
                        break;
                        
                    case Core.Domain.Enums.ResourceType.Modpack:
                        var hasManifestJson = archive.Entries.Any(e => e.FullName == "manifest.json");
                        var hasOverrides = archive.Entries.Any(e => e.FullName.StartsWith("overrides/"));
                        if (!hasManifestJson && !hasOverrides)
                        {
                            errors.Add("Modpack should contain manifest.json or overrides directory");
                        }
                        break;
                        
                    case Core.Domain.Enums.ResourceType.Shader:
                        var hasShaderFiles = archive.Entries.Any(e => 
                            e.FullName.EndsWith(".vsh") || 
                            e.FullName.EndsWith(".fsh") ||
                            e.FullName.EndsWith(".glsl"));
                        if (!hasShaderFiles)
                        {
                            errors.Add("Shader pack should contain .vsh or .fsh files");
                        }
                        break;
                        
                    case Core.Domain.Enums.ResourceType.TexturePack:
                        var hasPackMcmeta = archive.Entries.Any(e => e.FullName == "pack.mcmeta");
                        if (!hasPackMcmeta)
                        {
                            errors.Add("Texture pack should contain pack.mcmeta file");
                        }
                        break;
                }
                
                var dangerousExtensions = new[] { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh" };
                var dangerousFiles = archive.Entries
                    .Where(e => dangerousExtensions.Contains(Path.GetExtension(e.FullName).ToLower()))
                    .Select(e => e.FullName)
                    .ToList();
                    
                if (dangerousFiles.Count > 0)
                {
                    errors.Add($"Archive contains dangerous files: {string.Join(", ", dangerousFiles)}");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to read ZIP archive: {ex.Message}");
            }
            
            return new FileValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }
        
        private async Task<MalwareScanResult> ScanForMalware(string filePath)
        {
            var suspiciousPatterns = new[]
            {
                "eval(",
                "base64_decode(",
                "system(",
                "exec(",
                "shell_exec(",
                "passthru(",
                "proc_open("
            };
            
            try
            {
                using var stream = File.OpenRead(filePath);
                using var reader = new StreamReader(stream);
                
                var content = await reader.ReadToEndAsync();
                
                foreach (var pattern in suspiciousPatterns)
                {
                    if (content.Contains(pattern))
                    {
                        return new MalwareScanResult
                        {
                            IsClean = false,
                            Threat = $"Suspicious pattern detected: {pattern}"
                        };
                    }
                }
                
                return new MalwareScanResult { IsClean = true };
            }
            catch
            {
                return new MalwareScanResult { IsClean = true };
            }
        }
        
        private async Task<string> CalculateFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        
        private string[] GetAllowedExtensions(Core.Domain.Enums.ResourceType type)
        {
            return type switch
            {
                Core.Domain.Enums.ResourceType.Mod => new[] { ".jar", ".zip" },
                Core.Domain.Enums.ResourceType.Modpack => new[] { ".zip" },
                Core.Domain.Enums.ResourceType.Shader => new[] { ".zip", ".jar" },
                Core.Domain.Enums.ResourceType.TexturePack => new[] { ".zip", ".png" },
                _ => new[] { ".zip" }
            };
        }
        
        private long GetMaxFileSize(Core.Domain.Enums.ResourceType type)
        {
            return type switch
            {
                Core.Domain.Enums.ResourceType.Mod => 100L * 1024 * 1024,
                Core.Domain.Enums.ResourceType.Modpack => 10L * 1024 * 1024 * 1024,
                Core.Domain.Enums.ResourceType.Shader => 50L * 1024 * 1024,
                Core.Domain.Enums.ResourceType.TexturePack => 200L * 1024 * 1024,
                _ => 100L * 1024 * 1024
            };
        }
        
        private class UploadSession
        {
            public string UploadId { get; set; }
            public Core.DTOs.Resource.UploadRequest Request { get; set; }
            public UploadStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public string FilePath { get; set; }
            public int ChunksUploaded { get; set; }
            public int TotalChunks { get; set; }
            public string FileHash { get; set; }
        }
    }
}
