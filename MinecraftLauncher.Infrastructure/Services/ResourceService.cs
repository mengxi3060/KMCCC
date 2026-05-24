using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.DTOs.Resource;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Infrastructure.Data;
using System.Text.Json;

namespace MinecraftLauncher.Infrastructure.Services
{
    public class ResourceService : IResourceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResourceService> _logger;
        private readonly IResourceUploadService _uploadService;
        
        public ResourceService(
            AppDbContext context,
            ILogger<ResourceService> logger,
            IResourceUploadService uploadService)
        {
            _context = context;
            _logger = logger;
            _uploadService = uploadService;
        }
        
        public async Task<ResourceListResult> GetResources(ResourceBrowseQuery query)
        {
            var resourcesQuery = _context.Resources
                .Include(r => r.Author)
                .ThenInclude(u => u.Profile)
                .Include(r => r.Compatibilities)
                .Where(r => r.Status == Core.Domain.Enums.ResourceStatus.Approved);
            
            if (query.Type.HasValue)
            {
                resourcesQuery = resourcesQuery.Where(r => r.Type == query.Type.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(query.GameVersion))
            {
                resourcesQuery = resourcesQuery.Where(r => 
                    r.Compatibilities.Any(c => c.GameVersion == query.GameVersion));
            }
            
            if (query.Loader.HasValue)
            {
                resourcesQuery = resourcesQuery.Where(r => 
                    r.Compatibilities.Any(c => c.LoaderType == query.Loader.Value));
            }
            
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                resourcesQuery = resourcesQuery.Where(r => 
                    r.Name.Contains(query.Keyword) ||
                    r.Description.Contains(query.Keyword) ||
                    r.Tags.Contains(query.Keyword));
            }
            
            resourcesQuery = query.SortBy switch
            {
                SortBy.Newest => resourcesQuery.OrderByDescending(r => r.CreatedAt),
                SortBy.Popular => resourcesQuery.OrderByDescending(r => r.DownloadCount + r.LikeCount),
                SortBy.Downloads => resourcesQuery.OrderByDescending(r => r.DownloadCount),
                SortBy.Rating => resourcesQuery.OrderByDescending(r => r.LikeCount),
                _ => resourcesQuery.OrderByDescending(r => r.CreatedAt)
            };
            
            var totalCount = await resourcesQuery.CountAsync();
            
            var rawResources = await resourcesQuery
                .Skip(query.PageIndex * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var resources = rawResources.Select(r => new ResourceListItem
            {
                Id = r.Id,
                Name = r.Name,
                Type = r.Type,
                AuthorName = r.Author.Profile != null ? r.Author.Profile.DisplayName : r.Author.Username,
                Description = r.Description,
                Tags = JsonSerializer.Deserialize<List<string>>(r.Tags ?? "[]") ?? new List<string>(),
                DownloadCount = r.DownloadCount,
                LikeCount = r.LikeCount,
                CreatedAt = r.CreatedAt,
                Compatibilities = r.Compatibilities.Select(c => new CompatibilityInfo
                {
                    GameVersion = c.GameVersion,
                    LoaderType = c.LoaderType
                }).ToList()
            }).ToList();
            
            return new ResourceListResult
            {
                Resources = resources,
                TotalCount = totalCount,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };
        }
        
        public async Task<ResourceDetail> GetResourceDetail(Guid resourceId)
        {
            var resource = await _context.Resources
                .Include(r => r.Author)
                .ThenInclude(u => u.Profile)
                .Include(r => r.Compatibilities)
                .Include(r => r.Comments.Where(c => c.ParentId == null))
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == resourceId);
            
            if (resource == null)
            {
                return null;
            }
            
            return new ResourceDetail
            {
                Id = resource.Id,
                Name = resource.Name,
                Type = resource.Type,
                AuthorId = resource.AuthorId,
                AuthorName = resource.Author.Profile?.DisplayName ?? resource.Author.Username,
                AuthorAvatar = resource.Author.Profile?.Avatar,
                Description = resource.Description,
                Tags = JsonSerializer.Deserialize<List<string>>(resource.Tags ?? "[]") ?? new List<string>(),
                Screenshots = JsonSerializer.Deserialize<List<string>>(resource.Screenshots ?? "[]") ?? new List<string>(),
                Copyright = resource.Copyright,
                FileSize = resource.FileSize,
                DownloadCount = resource.DownloadCount,
                LikeCount = resource.LikeCount,
                Status = resource.Status,
                CreatedAt = resource.CreatedAt,
                UpdatedAt = resource.UpdatedAt ?? DateTime.UtcNow,
                IsTopped = resource.IsTopped,
                IsRecommended = resource.IsRecommended,
                Compatibilities = resource.Compatibilities.Select(c => new CompatibilityInfo
                {
                    GameVersion = c.GameVersion,
                    LoaderType = c.LoaderType,
                    IsVerified = c.IsVerified
                }).ToList(),
                Comments = resource.Comments.Select(c => new CommentInfo
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.Profile?.DisplayName ?? c.User.Username,
                    UserAvatar = c.User.Profile?.Avatar,
                    Content = c.Content,
                    LikeCount = c.LikeCount,
                    IsEdited = c.IsEdited,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };
        }
        
        public async Task<Resource> CreateResource(CreateResourceRequest request, Guid authorId)
        {
            var resource = new Resource
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Type = request.Type,
                AuthorId = authorId,
                Description = request.Description,
                Tags = JsonSerializer.Serialize(request.Tags ?? new List<string>()),
                Screenshots = JsonSerializer.Serialize(request.Screenshots ?? new List<string>()),
                Copyright = request.Copyright,
                FilePath = request.FilePath,
                FileSize = request.FileSize,
                FileHash = request.FileHash,
                DownloadCount = 0,
                LikeCount = 0,
                Status = Core.Domain.Enums.ResourceStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                IsTopped = false,
                IsRecommended = false,
                Compatibilities = new List<ResourceCompatibility>(),
                Comments = new List<Comment>()
            };
            
            if (request.Compatibilities != null)
            {
                foreach (var comp in request.Compatibilities)
                {
                    resource.Compatibilities.Add(new ResourceCompatibility
                    {
                        Id = Guid.NewGuid(),
                        ResourceId = resource.Id,
                        GameVersion = comp.GameVersion,
                        LoaderType = comp.LoaderType ?? Core.Domain.Enums.LoaderType.None,
                        IsVerified = false
                    });
                }
            }
            
            _context.Resources.Add(resource);
            
            var profile = await _context.UserProfiles.FindAsync(authorId);
            if (profile != null)
            {
                profile.UploadCount++;
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Resource created: {resource.Id} by user {authorId}");
            
            return resource;
        }
        
        public async Task<Resource> UpdateResource(Guid resourceId, UpdateResourceRequest request, Guid userId)
        {
            var resource = await _context.Resources
                .Include(r => r.Compatibilities)
                .FirstOrDefaultAsync(r => r.Id == resourceId);
            
            if (resource == null)
            {
                return null;
            }
            
            if (resource.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this resource");
            }
            
            if (resource.Status == Core.Domain.Enums.ResourceStatus.Frozen || 
                resource.Status == Core.Domain.Enums.ResourceStatus.Removed)
            {
                throw new InvalidOperationException("Cannot update a frozen or removed resource");
            }
            
            resource.Name = request.Name ?? resource.Name;
            resource.Description = request.Description ?? resource.Description;
            resource.Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : resource.Tags;
            resource.Screenshots = request.Screenshots != null ? JsonSerializer.Serialize(request.Screenshots) : resource.Screenshots;
            resource.Copyright = request.Copyright ?? resource.Copyright;
            resource.UpdatedAt = DateTime.UtcNow;
            
            if (request.Compatibilities != null)
            {
                _context.ResourceCompatibilities.RemoveRange(resource.Compatibilities);
                
                foreach (var comp in request.Compatibilities)
                {
                    resource.Compatibilities.Add(new ResourceCompatibility
                    {
                        Id = Guid.NewGuid(),
                        ResourceId = resource.Id,
                        GameVersion = comp.GameVersion,
                        LoaderType = comp.LoaderType ?? Core.Domain.Enums.LoaderType.None,
                        IsVerified = false
                    });
                }
            }
            
            if (resource.Status == Core.Domain.Enums.ResourceStatus.Approved)
            {
                resource.Status = Core.Domain.Enums.ResourceStatus.Pending;
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Resource updated: {resource.Id}");
            
            return resource;
        }
        
        public async Task<bool> DeleteResource(Guid resourceId, Guid userId)
        {
            var resource = await _context.Resources.FindAsync(resourceId);
            
            if (resource == null)
            {
                return false;
            }
            
            if (resource.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this resource");
            }
            
            resource.Status = Core.Domain.Enums.ResourceStatus.Removed;
            resource.UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(resource.FilePath) && File.Exists(resource.FilePath))
            {
                try
                {
                    File.Delete(resource.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to delete file: {resource.FilePath}");
                }
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Resource deleted: {resourceId}");
            
            return true;
        }
        
        public async Task<IEnumerable<Resource>> GetMyResources(Guid userId)
        {
            return await _context.Resources
                .Include(r => r.Compatibilities)
                .Where(r => r.AuthorId == userId && r.Status != Core.Domain.Enums.ResourceStatus.Removed)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
