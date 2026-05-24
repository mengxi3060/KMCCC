using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Common;
using MinecraftLauncher.Core.DTOs.Report;
using System.Security.Claims;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> SubmitReport([FromBody] ReportRequest request)
    {
        try
        {
            var reporterId = GetCurrentUserId();
            var reportId = await _reportService.SubmitReport(request, reporterId);
            return Ok(ApiResponse<object>.Ok(new { ReportId = reportId }, "举报已提交"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReportListItem>>>> GetPendingReports(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20)
    {
        var reports = await _reportService.GetPendingReports(pageIndex, pageSize);
        return Ok(ApiResponse<IEnumerable<ReportListItem>>.Ok(reports));
    }

    [HttpGet("{reportId}")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<ActionResult<ApiResponse<ReportDetail>>> GetReportDetail(Guid reportId)
    {
        var report = await _reportService.GetReportDetail(reportId);
        if (report == null)
        {
            return NotFound(ApiResponse<ReportDetail>.Fail("举报不存在"));
        }
        return Ok(ApiResponse<ReportDetail>.Ok(report));
    }

    [HttpPost("{reportId}/resolve")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<ActionResult<ApiResponse>> ResolveReport(
        Guid reportId,
        [FromBody] ReportResolution resolution)
    {
        var resolvedBy = GetCurrentUserId();
        var success = await _reportService.ResolveReport(reportId, resolution, resolvedBy);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("举报不存在或已被处理"));
        }

        return Ok(ApiResponse.Ok("举报已处理"));
    }
}
