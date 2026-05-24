using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Report;
using MinecraftLauncher.Infrastructure.Data;
using System.Text.Json;

namespace MinecraftLauncher.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportService> _logger;
    private readonly INotificationService _notificationService;
    
    public ReportService(
        AppDbContext context,
        ILogger<ReportService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }
    
    public async Task<Guid> SubmitReport(ReportRequest request, Guid reporterId)
    {
        var resource = await _context.Resources.FindAsync(request.ResourceId);
        
        if (resource == null)
        {
            throw new InvalidOperationException("Resource not found");
        }
        
        var existingReport = await _context.Reports
            .FirstOrDefaultAsync(r => 
                r.ResourceId == request.ResourceId && 
                r.ReporterId == reporterId && 
                r.Status == ReportStatus.Pending);
        
        if (existingReport != null)
        {
            throw new InvalidOperationException("You have already reported this resource");
        }
        
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ResourceId = request.ResourceId,
            ReporterId = reporterId,
            Type = request.Type,
            Description = request.Description,
            EvidenceUrls = request.EvidenceUrls != null ? 
                JsonSerializer.Serialize(request.EvidenceUrls) : null,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Report submitted: {ReportId} for resource {ResourceId} by user {ReporterId}", 
            report.Id, request.ResourceId, reporterId);
        
        return report.Id;
    }
    
    public async Task<IEnumerable<ReportListItem>> GetPendingReports(int pageIndex, int pageSize)
    {
        var rawReports = await _context.Reports
            .Include(r => r.Resource)
            .Include(r => r.Reporter)
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return rawReports.Select(r => new ReportListItem
        {
            Id = r.Id,
            ResourceId = r.ResourceId,
            ResourceName = r.Resource.Name,
            ReporterName = r.Reporter.Profile != null ? r.Reporter.Profile.DisplayName : r.Reporter.Username,
            Type = r.Type,
            Description = r.Description,
            EvidenceUrls = r.EvidenceUrls != null ? 
                JsonSerializer.Deserialize<List<string>>(r.EvidenceUrls) : null,
            CreatedAt = r.CreatedAt
        });
    }
    
    public async Task<ReportDetail> GetReportDetail(Guid reportId)
    {
        var report = await _context.Reports
            .Include(r => r.Resource)
            .ThenInclude(res => res.Author)
            .Include(r => r.Reporter)
            .Where(r => r.Id == reportId)
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return null;
        }

        return new ReportDetail
        {
            Id = report.Id,
            ResourceId = report.ResourceId,
            ResourceName = report.Resource.Name,
            ResourceAuthorName = report.Resource.Author.Profile != null ? report.Resource.Author.Profile.DisplayName : report.Resource.Author.Username,
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter.Profile != null ? report.Reporter.Profile.DisplayName : report.Reporter.Username,
            Type = report.Type,
            Description = report.Description,
            EvidenceUrls = report.EvidenceUrls != null ? 
                JsonSerializer.Deserialize<List<string>>(report.EvidenceUrls) : null,
            Status = report.Status,
            Resolution = report.Resolution,
            CreatedAt = report.CreatedAt,
            ResolvedAt = report.ResolvedAt
        };
    }
    
    public async Task<bool> ResolveReport(Guid reportId, ReportResolution resolution, Guid resolvedBy)
    {
        var report = await _context.Reports.FindAsync(reportId);
        
        if (report == null)
        {
            return false;
        }
        
        if (report.Status != ReportStatus.Pending)
        {
            return false;
        }
        
        try
        {
            report.Status = resolution.Status;
            report.Resolution = resolution.Comment;
            report.ResolvedBy = resolvedBy;
            report.ResolvedAt = DateTime.UtcNow;
            
            if (resolution.Status == ReportStatus.Resolved)
            {
                var resource = await _context.Resources.FindAsync(report.ResourceId);
                if (resource != null && resource.Status == ResourceStatus.Approved)
                {
                    resource.Status = ResourceStatus.Frozen;
                    
                    var reviewRecord = new ReviewRecord
                    {
                        Id = Guid.NewGuid(),
                        ResourceId = resource.Id,
                        ReviewerId = resolvedBy,
                        Action = ReviewAction.Freeze,
                        Comment = $"因用户举报被冻结: {resolution.Comment}",
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.ReviewRecords.Add(reviewRecord);
                    
                    await _notificationService.SendNotificationAsync(
                        resource.AuthorId,
                        "资源因举报被冻结",
                        $"您的资源「{resource.Name}」因收到举报已被管理员冻结",
                        NotificationType.ResourceFrozen,
                        resource.Id.ToString()
                    );
                }
            }
            
            await _notificationService.SendNotificationAsync(
                report.ReporterId,
                "举报已处理",
                $"您对资源「{report.Resource?.Name}」的举报已被处理\n处理结果: {(resolution.Status == ReportStatus.Resolved ? "已采纳" : "已驳回")}",
                NotificationType.ReportResolved,
                report.Id.ToString()
            );
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Report resolved: {ReportId} by user {ResolvedBy}", reportId, resolvedBy);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve report: {ReportId}", reportId);
            return false;
        }
    }
}
