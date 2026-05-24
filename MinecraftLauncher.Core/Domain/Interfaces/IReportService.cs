using MinecraftLauncher.Core.DTOs.Report;
using MinecraftLauncher.Core.Domain.Entities;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IReportService
{
    Task<Guid> SubmitReport(ReportRequest request, Guid reporterId);
    Task<IEnumerable<ReportListItem>> GetPendingReports(int pageIndex, int pageSize);
    Task<ReportDetail> GetReportDetail(Guid reportId);
    Task<bool> ResolveReport(Guid reportId, ReportResolution resolution, Guid resolvedBy);
}
