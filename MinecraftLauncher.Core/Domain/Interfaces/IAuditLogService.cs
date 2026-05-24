using MinecraftLauncher.Core.Domain.Entities;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string targetType, Guid? targetId, object details, Guid? userId = null, string ipAddress = null, string userAgent = null);
    Task<IEnumerable<AuditLog>> GetLogs(AuditLogQuery query);
    Task<Dictionary<string, int>> GetActionStats(DateTime from, DateTime to);
}

public class AuditLogQuery
{
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
