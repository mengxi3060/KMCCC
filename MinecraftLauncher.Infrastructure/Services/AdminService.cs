using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Admin;
using MinecraftLauncher.Core.DTOs.Resource;
using MinecraftLauncher.Infrastructure.Data;

namespace MinecraftLauncher.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminService> _logger;
    private readonly IAuditLogService _auditLogService;

    public AdminService(
        AppDbContext context,
        ILogger<AdminService> logger,
        IAuditLogService auditLogService)
    {
        _context = context;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<UserListResult> GetUsers(UserQuery query)
    {
        var queryable = _context.Users
            .Include(u => u.Profile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => 
                u.Username.Contains(query.Keyword) || 
                u.Email.Contains(query.Keyword) ||
                (u.Profile != null && u.Profile.DisplayName.Contains(query.Keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            queryable = queryable.Where(u => u.Role == query.Role);
        }

        if (query.IsBanned.HasValue)
        {
            queryable = queryable.Where(u => u.IsBanned == query.IsBanned.Value);
        }

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new UserListResult
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<bool> BanUser(Guid userId, BanRequest request, Guid adminId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return false;
        }

        user.IsBanned = true;
        user.BanReason = request.Reason;
        user.BanExpiresAt = request.ExpiresAt;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            "BanUser",
            "User",
            userId,
            new { Reason = request.Reason, ExpiresAt = request.ExpiresAt, AdminId = adminId }
        );

        _logger.LogWarning($"User {userId} banned by admin {adminId}: {request.Reason}");

        return true;
    }

    public async Task<bool> UnbanUser(Guid userId, Guid adminId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null || !user.IsBanned)
        {
            return false;
        }

        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            "UnbanUser",
            "User",
            userId,
            new { AdminId = adminId }
        );

        _logger.LogInformation($"User {userId} unbanned by admin {adminId}");

        return true;
    }

    public async Task<bool> UpdateUserRole(Guid userId, string role, Guid adminId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return false;
        }

        var oldRole = user.Role;
        user.Role = role;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            "UpdateUserRole",
            "User",
            userId,
            new { OldRole = oldRole, NewRole = role, AdminId = adminId }
        );

        _logger.LogInformation($"User {userId} role updated from {oldRole} to {role} by admin {adminId}");

        return true;
    }

    public async Task<ResourceListResult> GetAllResources(ResourceManagementQuery query)
    {
        var queryable = _context.Resources
            .Include(r => r.Author)
            .ThenInclude(u => u.Profile)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(r => r.Status == query.Status.Value);
        }

        if (query.AuthorId.HasValue)
        {
            queryable = queryable.Where(r => r.AuthorId == query.AuthorId.Value);
        }

        if (query.StartDate.HasValue)
        {
            queryable = queryable.Where(r => r.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            queryable = queryable.Where(r => r.CreatedAt <= query.EndDate.Value);
        }

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new ResourceListResult
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<bool> SetResourceTopped(Guid resourceId, bool isTopped, Guid adminId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);

        if (resource == null)
        {
            return false;
        }

        resource.IsTopped = isTopped;
        resource.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            isTopped ? "SetResourceTopped" : "RemoveResourceTopped",
            "Resource",
            resourceId,
            new { AdminId = adminId }
        );

        _logger.LogInformation($"Resource {resourceId} topped status set to {isTopped} by admin {adminId}");

        return true;
    }

    public async Task<bool> SetResourceRecommended(Guid resourceId, bool isRecommended, Guid adminId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);

        if (resource == null)
        {
            return false;
        }

        resource.IsRecommended = isRecommended;
        resource.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            isRecommended ? "SetResourceRecommended" : "RemoveResourceRecommended",
            "Resource",
            resourceId,
            new { AdminId = adminId }
        );

        _logger.LogInformation($"Resource {resourceId} recommended status set to {isRecommended} by admin {adminId}");

        return true;
    }

    public async Task<bool> UnlistResource(Guid resourceId, string reason, Guid adminId)
    {
        var resource = await _context.Resources.FindAsync(resourceId);

        if (resource == null)
        {
            return false;
        }

        resource.Status = ResourceStatus.Removed;
        resource.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            "UnlistResource",
            "Resource",
            resourceId,
            new { Reason = reason, AdminId = adminId }
        );

        _logger.LogWarning($"Resource {resourceId} unlisted by admin {adminId}: {reason}");

        return true;
    }

    public async Task<AdminDashboardStats> GetDashboardStats()
    {
        var stats = new AdminDashboardStats
        {
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsersToday = await _context.Users
                .Where(u => u.LastLoginAt >= DateTime.UtcNow.Date)
                .CountAsync(),
            TotalResources = await _context.Resources.CountAsync(),
            PendingReviews = await _context.Resources
                .Where(r => r.Status == ResourceStatus.Pending)
                .CountAsync(),
            ApprovedResources = await _context.Resources
                .Where(r => r.Status == ResourceStatus.Approved)
                .CountAsync(),
            TotalDownloads = await _context.Resources
                .SumAsync(r => r.DownloadCount),
            PendingReports = await _context.Reports
                .Where(r => r.Status == ReportStatus.Pending)
                .CountAsync(),
            TotalViolations = await _context.Violations.CountAsync(),
            RecentViolations = await _context.Violations
                .Where(v => v.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync()
        };

        return stats;
    }
}
