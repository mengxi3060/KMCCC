using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Review;
using MinecraftLauncher.Infrastructure.Data;
using System.Text.Json;

namespace MinecraftLauncher.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReviewService> _logger;
    private readonly INotificationService _notificationService;
    
    public ReviewService(
        AppDbContext context,
        ILogger<ReviewService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }
    
    public async Task<ReviewQueueResult> GetReviewQueue(ReviewQuery query)
    {
        var resourcesQuery = _context.Resources
            .Include(r => r.Author)
            .ThenInclude(u => u.Profile)
            .Include(r => r.Compatibilities)
            .Where(r => r.Status == ResourceStatus.Pending);
        
        if (query.Status.HasValue)
        {
            resourcesQuery = query.Status.Value switch
            {
                ReviewStatus.Pending => resourcesQuery.Where(r => r.Status == ResourceStatus.Pending),
                ReviewStatus.InReview => resourcesQuery.Where(r => r.Status == ResourceStatus.Pending),
                _ => resourcesQuery
            };
        }
        
        if (query.Type.HasValue)
        {
            resourcesQuery = resourcesQuery.Where(r => r.Type == query.Type.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            resourcesQuery = resourcesQuery.Where(r => 
                r.Name.Contains(query.Keyword) ||
                r.Description.Contains(query.Keyword));
        }
        
        if (query.DateFrom.HasValue)
        {
            resourcesQuery = resourcesQuery.Where(r => r.CreatedAt >= query.DateFrom.Value);
        }
        
        if (query.DateTo.HasValue)
        {
            resourcesQuery = resourcesQuery.Where(r => r.CreatedAt <= query.DateTo.Value);
        }
        
        resourcesQuery = resourcesQuery.OrderBy(r => r.CreatedAt);
        
        var totalCount = await resourcesQuery.CountAsync();
        
        var resources = await resourcesQuery
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReviewQueueItem
            {
                ResourceId = r.Id,
                Name = r.Name,
                Type = r.Type,
                AuthorName = r.Author.Profile != null ? r.Author.Profile.DisplayName : r.Author.Username,
                AuthorViolationCount = r.Author.ViolationCount,
                Description = r.Description,
                Tags = JsonSerializer.Deserialize<List<string>>(r.Tags ?? "[]") ?? new List<string>(),
                FileSize = r.FileSize,
                CreatedAt = r.CreatedAt,
                Compatibilities = r.Compatibilities.Select(c => new CompatibilityInfo
                {
                    GameVersion = c.GameVersion,
                    LoaderType = c.LoaderType.ToString(),
                    IsVerified = c.IsVerified
                }).ToList()
            })
            .ToListAsync();
        
        return new ReviewQueueResult
        {
            Items = resources,
            TotalCount = totalCount,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }
    
    public async Task<ReviewDetail> GetReviewDetail(Guid resourceId)
    {
        var resource = await _context.Resources
            .Include(r => r.Author)
            .ThenInclude(u => u.Profile)
            .Include(r => r.Compatibilities)
            .Include(r => r.Comments)
            .FirstOrDefaultAsync(r => r.Id == resourceId);
        
        if (resource == null)
        {
            return null;
        }
        
        var reviewHistory = await _context.ReviewRecords
            .Include(r => r.Reviewer)
            .Where(r => r.ResourceId == resourceId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewHistoryItem
            {
                Id = r.Id,
                ReviewerName = r.Reviewer.Profile != null ? r.Reviewer.Profile.DisplayName : r.Reviewer.Username,
                Action = r.Action.ToString(),
                Comment = r.Comment,
                CheckResults = r.CheckResults != null ? 
                    JsonSerializer.Deserialize<Dictionary<string, bool>>(r.CheckResults) : null,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
        
        return new ReviewDetail
        {
            ResourceId = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            AuthorId = resource.AuthorId,
            AuthorName = resource.Author.Profile?.DisplayName ?? resource.Author.Username,
            AuthorEmail = resource.Author.Email,
            AuthorViolationCount = resource.Author.ViolationCount,
            Description = resource.Description,
            Tags = JsonSerializer.Deserialize<List<string>>(resource.Tags ?? "[]") ?? new List<string>(),
            Screenshots = JsonSerializer.Deserialize<List<string>>(resource.Screenshots ?? "[]") ?? new List<string>(),
            Copyright = resource.Copyright ?? string.Empty,
            FilePath = resource.FilePath ?? string.Empty,
            FileSize = resource.FileSize,
            Status = resource.Status,
            CreatedAt = resource.CreatedAt,
            Compatibilities = resource.Compatibilities.Select(c => new CompatibilityInfo
            {
                GameVersion = c.GameVersion,
                LoaderType = c.LoaderType.ToString(),
                IsVerified = c.IsVerified
            }).ToList(),
            ReviewHistory = reviewHistory
        };
    }
    
    public async Task<ReviewActionResult> ApproveResource(Guid resourceId, ReviewComment comment, Guid reviewerId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);
        
        if (resource == null)
        {
            return new ReviewActionResult
            {
                Success = false,
                Message = "Resource not found"
            };
        }
        
        if (resource.Status != ResourceStatus.Pending)
        {
            return new ReviewActionResult
            {
                Success = false,
                Message = "Resource is not in pending status"
            };
        }
        
        try
        {
            resource.Status = ResourceStatus.Approved;
            resource.ApprovedAt = DateTime.UtcNow;
            
            var reviewRecord = new ReviewRecord
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                ReviewerId = reviewerId,
                Action = ReviewAction.Approve,
                Comment = comment?.Message,
                CheckResults = comment?.CheckResults != null ? 
                    JsonSerializer.Serialize(comment.CheckResults) : null,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ReviewRecords.Add(reviewRecord);
            
            await _context.SaveChangesAsync();
            
            await _notificationService.SendNotificationAsync(
                resource.AuthorId,
                "资源审核通过",
                $"您的资源「{resource.Name}」已通过审核",
                NotificationType.ReviewApproved,
                resourceId.ToString()
            );
            
            _logger.LogInformation("Resource approved: {ResourceId} by reviewer {ReviewerId}", resourceId, reviewerId);
            
            return new ReviewActionResult
            {
                Success = true,
                Message = "Resource approved successfully",
                ResourceId = resourceId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve resource: {ResourceId}", resourceId);
            
            return new ReviewActionResult
            {
                Success = false,
                Message = $"Failed to approve resource: {ex.Message}"
            };
        }
    }
    
    public async Task<ReviewActionResult> RejectResource(Guid resourceId, RejectReason reason, Guid reviewerId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);
        
        if (resource == null)
        {
            return new ReviewActionResult
            {
                Success = false,
                Message = "Resource not found"
            };
        }
        
        if (resource.Status != ResourceStatus.Pending)
        {
            return new ReviewActionResult
            {
                Success = false,
                Message = "Resource is not in pending status"
            };
        }
        
        try
        {
            resource.Status = ResourceStatus.Rejected;
            
            var reviewRecord = new ReviewRecord
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                ReviewerId = reviewerId,
                Action = ReviewAction.Reject,
                Comment = reason?.Reason + (reason?.Details != null ? 
                    "\n详细说明:\n" + string.Join("\n", reason.Details) : ""),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ReviewRecords.Add(reviewRecord);
            
            await _context.SaveChangesAsync();
            
            await _notificationService.SendNotificationAsync(
                resource.AuthorId,
                "资源审核未通过",
                $"您的资源「{resource.Name}」未通过审核\n原因: {reason?.Reason}",
                NotificationType.ReviewRejected,
                resourceId.ToString()
            );
            
            _logger.LogInformation("Resource rejected: {ResourceId} by reviewer {ReviewerId}", resourceId, reviewerId);
            
            return new ReviewActionResult
            {
                Success = true,
                Message = "Resource rejected successfully",
                ResourceId = resourceId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject resource: {ResourceId}", resourceId);
            
            return new ReviewActionResult
            {
                Success = false,
                Message = $"Failed to reject resource: {ex.Message}"
            };
        }
    }
    
    public async Task<ReviewActionResult> FreezeResource(Guid resourceId, FreezeReason reason, Guid reviewerId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);
        
        if (resource == null)
        {
            return new ReviewActionResult
            {
                Success = false,
                Message = "Resource not found"
            };
        }
        
        if (resource.Status != ResourceStatus.Approved)
        {
            return new ReviewActionResult
            {
                Success = false,
                Message = "Only approved resources can be frozen"
            };
        }
        
        try
        {
            resource.Status = ResourceStatus.Frozen;
            
            var reviewRecord = new ReviewRecord
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                ReviewerId = reviewerId,
                Action = ReviewAction.Freeze,
                Comment = reason?.Reason + (reason?.IsPermanent == true ? " (永久冻结)" : $" (解冻时间: {reason?.ExpiresAt})"),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ReviewRecords.Add(reviewRecord);
            
            await _context.SaveChangesAsync();
            
            await _notificationService.SendNotificationAsync(
                resource.AuthorId,
                "资源已被冻结",
                $"您的资源「{resource.Name}」已被冻结\n原因: {reason?.Reason}",
                NotificationType.ResourceFrozen,
                resourceId.ToString()
            );
            
            _logger.LogInformation("Resource frozen: {ResourceId} by reviewer {ReviewerId}", resourceId, reviewerId);
            
            return new ReviewActionResult
            {
                Success = true,
                Message = "Resource frozen successfully",
                ResourceId = resourceId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to freeze resource: {ResourceId}", resourceId);
            
            return new ReviewActionResult
            {
                Success = false,
                Message = $"Failed to freeze resource: {ex.Message}"
            };
        }
    }
    
    public async Task<IEnumerable<ReviewHistoryItem>> GetReviewHistory(Guid resourceId)
    {
        return await _context.ReviewRecords
            .Include(r => r.Reviewer)
            .Where(r => r.ResourceId == resourceId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewHistoryItem
            {
                Id = r.Id,
                ReviewerName = r.Reviewer.Profile != null ? r.Reviewer.Profile.DisplayName : r.Reviewer.Username,
                Action = r.Action.ToString(),
                Comment = r.Comment,
                CheckResults = r.CheckResults != null ? 
                    JsonSerializer.Deserialize<Dictionary<string, bool>>(r.CheckResults) : null,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }
}
