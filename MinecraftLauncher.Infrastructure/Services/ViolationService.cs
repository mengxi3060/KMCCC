using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Infrastructure.Data;

namespace MinecraftLauncher.Infrastructure.Services;

public class ViolationService : IViolationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ViolationService> _logger;
    private readonly INotificationService _notificationService;
    
    public ViolationService(
        AppDbContext context,
        ILogger<ViolationService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }
    
    public async Task<Violation> RecordViolation(Guid userId, ViolationType type, string description, 
        ViolationSeverity severity, Guid? resourceId, Guid handledBy)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }
        
        var violation = new Violation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Description = description,
            Severity = severity,
            ResourceId = resourceId,
            HandledBy = handledBy,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Violations.Add(violation);
        
        user.ViolationCount++;
        
        await ApplyPunishment(user, user.ViolationCount);
        
        await _context.SaveChangesAsync();
        
        _logger.LogWarning("Violation recorded for user {UserId}: {Type}, severity: {Severity}", 
            userId, type, severity);
        
        return violation;
    }
    
    public async Task<IEnumerable<Violation>> GetUserViolations(Guid userId)
    {
        return await _context.Violations
            .Include(v => v.Handler)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }
    
    private async Task ApplyPunishment(User user, int violationCount)
    {
        string punishmentMessage = "";
        
        switch (violationCount)
        {
            case 1:
                punishmentMessage = "您因违规行为收到警告。如有资源涉嫌违规,管理员已将其下架处理。";
                break;
                
            case 2:
                punishmentMessage = "您因再次违规被限制上传功能7天。请遵守社区规范,避免再次违规。";
                break;
                
            case 3:
                punishmentMessage = "您因多次违规被限制上传功能30天。如再次违规,账号将被永久封禁。";
                break;
                
            case >= 4:
                user.IsBanned = true;
                user.BanReason = "因多次违规,账号已被永久封禁";
                punishmentMessage = "您的账号因多次违规已被永久封禁。如有异议,请联系管理员。";
                break;
        }
        
        await _notificationService.SendNotificationAsync(
            user.Id,
            "违规通知",
            punishmentMessage,
            NotificationType.ViolationWarning
        );
    }
    
    public async Task<bool> LiftUploadRestriction(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null || !user.IsBanned)
        {
            return false;
        }
        
        var recentViolations = await _context.Violations
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(3)
            .ToListAsync();
        
        if (user.BanExpiresAt.HasValue && user.BanExpiresAt > DateTime.UtcNow)
        {
            return false;
        }
        
        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Upload restriction lifted for user {UserId}", userId);
        
        return true;
    }
}
