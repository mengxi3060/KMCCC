using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Common;
using MinecraftLauncher.Core.DTOs.Review;
using System.Security.Claims;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Moderator,Admin")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet("queue")]
    public async Task<ActionResult<ApiResponse<ReviewQueueResult>>> GetReviewQueue(
        [FromQuery] ReviewStatus? status,
        [FromQuery] ResourceType? type,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ReviewQuery
        {
            Status = status,
            Type = type,
            Keyword = keyword,
            DateFrom = dateFrom,
            DateTo = dateTo,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _reviewService.GetReviewQueue(query);
        return Ok(ApiResponse<ReviewQueueResult>.Ok(result));
    }

    [HttpGet("{resourceId}")]
    public async Task<ActionResult<ApiResponse<ReviewDetail>>> GetReviewDetail(Guid resourceId)
    {
        try
        {
            var result = await _reviewService.GetReviewDetail(resourceId);
            if (result == null)
            {
                return NotFound(ApiResponse<ReviewDetail>.Fail("资源不存在"));
            }
            return Ok(ApiResponse<ReviewDetail>.Ok(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<ReviewDetail>.Fail(ex.Message));
        }
    }

    [HttpPost("{resourceId}/approve")]
    public async Task<ActionResult<ApiResponse<ReviewActionResult>>> ApproveResource(
        Guid resourceId,
        [FromBody] ReviewComment comment)
    {
        var reviewerId = GetCurrentUserId();
        var result = await _reviewService.ApproveResource(resourceId, comment, reviewerId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<ReviewActionResult>.Fail(result.Message!));
        }

        return Ok(ApiResponse<ReviewActionResult>.Ok(result, "资源已通过审核"));
    }

    [HttpPost("{resourceId}/reject")]
    public async Task<ActionResult<ApiResponse<ReviewActionResult>>> RejectResource(
        Guid resourceId,
        [FromBody] RejectReason reason)
    {
        var reviewerId = GetCurrentUserId();
        var result = await _reviewService.RejectResource(resourceId, reason, reviewerId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<ReviewActionResult>.Fail(result.Message!));
        }

        return Ok(ApiResponse<ReviewActionResult>.Ok(result, "资源已驳回"));
    }

    [HttpPost("{resourceId}/freeze")]
    public async Task<ActionResult<ApiResponse<ReviewActionResult>>> FreezeResource(
        Guid resourceId,
        [FromBody] FreezeReason reason)
    {
        var reviewerId = GetCurrentUserId();
        var result = await _reviewService.FreezeResource(resourceId, reason, reviewerId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<ReviewActionResult>.Fail(result.Message!));
        }

        return Ok(ApiResponse<ReviewActionResult>.Ok(result, "资源已冻结"));
    }

    [HttpGet("{resourceId}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewHistoryItem>>>> GetReviewHistory(Guid resourceId)
    {
        var history = await _reviewService.GetReviewHistory(resourceId);
        return Ok(ApiResponse<IEnumerable<ReviewHistoryItem>>.Ok(history));
    }
}
