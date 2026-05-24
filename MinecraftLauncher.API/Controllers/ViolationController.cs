using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Common;
using System.Security.Claims;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ViolationController : ControllerBase
{
    private readonly IViolationService _violationService;

    public ViolationController(IViolationService violationService)
    {
        _violationService = violationService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpPost("record")]
    public async Task<ActionResult<ApiResponse>> RecordViolation(
        [FromQuery] Guid userId,
        [FromQuery] Core.Domain.Enums.ViolationType type,
        [FromQuery] Core.Domain.Enums.ViolationSeverity severity,
        [FromBody] Dictionary<string, string> request)
    {
        try
        {
            var handledBy = GetCurrentUserId();
            var description = request.GetValueOrDefault("description", string.Empty);
            var resourceIdStr = request.GetValueOrDefault("resourceId");
            Guid? resourceId = string.IsNullOrEmpty(resourceIdStr) ? null : Guid.Parse(resourceIdStr);

            var violation = await _violationService.RecordViolation(
                userId, type, description, severity, resourceId, handledBy);

            return Ok(ApiResponse<object>.Ok(new { ViolationId = violation.Id }, "违规已记录"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable>>> GetUserViolations(Guid userId)
    {
        var violations = await _violationService.GetUserViolations(userId);
        return Ok(ApiResponse<IEnumerable>.Ok(violations));
    }

    [HttpPost("user/{userId}/lift-restriction")]
    public async Task<ActionResult<ApiResponse>> LiftUploadRestriction(Guid userId)
    {
        var success = await _violationService.LiftUploadRestriction(userId);

        if (!success)
        {
            return BadRequest(ApiResponse.Fail("无法解除限制"));
        }

        return Ok(ApiResponse.Ok("限制已解除"));
    }
}
