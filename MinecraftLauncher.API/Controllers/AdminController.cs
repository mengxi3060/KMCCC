using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Admin;
using MinecraftLauncher.Core.DTOs.Common;
using MinecraftLauncher.Core.DTOs.Resource;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAuditLogService _auditLogService;

    public AdminController(
        IAdminService adminService,
        IAuditLogService auditLogService)
    {
        _adminService = adminService;
        _auditLogService = auditLogService;
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<ApiResponse<AdminDashboardStats>>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStats();
        return Ok(ApiResponse<AdminDashboardStats>.Ok(stats));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<UserListResult>>> GetUsers(
        [FromQuery] string? keyword,
        [FromQuery] string? role,
        [FromQuery] bool? isBanned,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new UserQuery
        {
            Keyword = keyword,
            Role = role,
            IsBanned = isBanned,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _adminService.GetUsers(query);
        return Ok(ApiResponse<UserListResult>.Ok(result));
    }

    [HttpPost("users/{userId}/ban")]
    public async Task<ActionResult<ApiResponse>> BanUser(
        Guid userId,
        [FromBody] BanRequest request)
    {
        var adminId = GetCurrentUserId();
        var success = await _adminService.BanUser(userId, request, adminId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("用户不存在"));
        }

        return Ok(ApiResponse.Ok("用户已封禁"));
    }

    [HttpPost("users/{userId}/unban")]
    public async Task<ActionResult<ApiResponse>> UnbanUser(Guid userId)
    {
        var adminId = GetCurrentUserId();
        var success = await _adminService.UnbanUser(userId, adminId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("用户不存在或未被封禁"));
        }

        return Ok(ApiResponse.Ok("用户已解封"));
    }

    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult<ApiResponse>> UpdateUserRole(
        Guid userId,
        [FromBody] UpdateRoleRequest request)
    {
        var adminId = GetCurrentUserId();
        var success = await _adminService.UpdateUserRole(userId, request.Role, adminId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("用户不存在"));
        }

        return Ok(ApiResponse.Ok("用户角色已更新"));
    }

    [HttpGet("resources")]
    public async Task<ActionResult<ApiResponse<ResourceListResult>>> GetAllResources(
        [FromQuery] int? status,
        [FromQuery] Guid? authorId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ResourceManagementQuery
        {
            Status = status.HasValue ? (Core.Domain.Enums.ResourceStatus)status.Value : null,
            AuthorId = authorId,
            StartDate = startDate,
            EndDate = endDate,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _adminService.GetAllResources(query);
        return Ok(ApiResponse<ResourceListResult>.Ok(result));
    }

    [HttpPut("resources/{resourceId}/top")]
    public async Task<ActionResult<ApiResponse>> SetResourceTopped(
        Guid resourceId,
        [FromQuery] bool isTopped)
    {
        var adminId = GetCurrentUserId();
        var success = await _adminService.SetResourceTopped(resourceId, isTopped, adminId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("资源不存在"));
        }

        return Ok(ApiResponse.Ok(isTopped ? "资源已置顶" : "资源已取消置顶"));
    }

    [HttpPut("resources/{resourceId}/recommend")]
    public async Task<ActionResult<ApiResponse>> SetResourceRecommended(
        Guid resourceId,
        [FromQuery] bool isRecommended)
    {
        var adminId = GetCurrentUserId();
        var success = await _adminService.SetResourceRecommended(resourceId, isRecommended, adminId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("资源不存在"));
        }

        return Ok(ApiResponse.Ok(isRecommended ? "资源已推荐" : "资源已取消推荐"));
    }

    [HttpDelete("resources/{resourceId}")]
    public async Task<ActionResult<ApiResponse>> UnlistResource(
        Guid resourceId,
        [FromQuery] string reason)
    {
        var adminId = GetCurrentUserId();
        var success = await _adminService.UnlistResource(resourceId, reason, adminId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("资源不存在"));
        }

        return Ok(ApiResponse.Ok("资源已下架"));
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<IEnumerable>>> GetAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? targetType,
        [FromQuery] Guid? targetId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new AuditLogQuery
        {
            UserId = userId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            StartDate = startDate,
            EndDate = endDate,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var logs = await _auditLogService.GetLogs(query);
        return Ok(ApiResponse<IEnumerable>.Ok(logs));
    }

    [HttpGet("audit-logs/stats")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetActionStats(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var stats = await _auditLogService.GetActionStats(from, to);
        return Ok(ApiResponse<Dictionary<string, int>>.Ok(stats));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
