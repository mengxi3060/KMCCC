using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Infrastructure.Data;

namespace MinecraftLauncher.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AppDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string action, string targetType, Guid? targetId, object details, 
        Guid? userId = null, string ipAddress = null, string userAgent = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogDebug($"Audit log created: {action} on {targetType} {targetId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create audit log: {action}");
        }
    }

    public async Task<IEnumerable<AuditLog>> GetLogs(AuditLogQuery query)
    {
        var queryable = _context.AuditLogs.AsQueryable();

        if (query.UserId.HasValue)
        {
            queryable = queryable.Where(l => l.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            queryable = queryable.Where(l => l.Action.Contains(query.Action));
        }

        if (!string.IsNullOrWhiteSpace(query.TargetType))
        {
            queryable = queryable.Where(l => l.TargetType == query.TargetType);
        }

        if (query.TargetId.HasValue)
        {
            queryable = queryable.Where(l => l.TargetId == query.TargetId.Value);
        }

        if (query.StartDate.HasValue)
        {
            queryable = queryable.Where(l => l.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            queryable = queryable.Where(l => l.CreatedAt <= query.EndDate.Value);
        }

        return await queryable
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetActionStats(DateTime from, DateTime to)
    {
        var stats = await _context.AuditLogs
            .Where(l => l.CreatedAt >= from && l.CreatedAt <= to)
            .GroupBy(l => l.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);

        return stats;
    }
}
