using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Comment;
using MinecraftLauncher.Core.DTOs.Common;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("resource/{resourceId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Comment>>>> GetComments(
        Guid resourceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var comments = await _commentService.GetComments(resourceId, page, pageSize);
        return Ok(ApiResponse<IEnumerable<Comment>>.Ok(comments));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> AddComment(
        [FromBody] AddCommentRequest request,
        [FromQuery] Guid userId,
        [FromQuery] Guid resourceId)
    {
        try
        {
            var comment = await _commentService.AddComment(resourceId, userId, request);
            return Ok(ApiResponse.Ok("评论已添加"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{commentId}")]
    public async Task<ActionResult<ApiResponse>> DeleteComment(
        Guid commentId,
        [FromQuery] Guid userId,
        [FromQuery] bool isAdmin = false)
    {
        try
        {
            var success = await _commentService.DeleteComment(commentId, userId, isAdmin);

            if (!success)
            {
                return NotFound(ApiResponse.Fail("评论不存在"));
            }

            return Ok(ApiResponse.Ok("评论已删除"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpPost("{commentId}/like")]
    public async Task<ActionResult<ApiResponse>> LikeComment(
        Guid commentId,
        [FromQuery] Guid userId)
    {
        var success = await _commentService.LikeComment(commentId, userId);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("评论不存在"));
        }

        return Ok(ApiResponse.Ok("点赞成功"));
    }

    [HttpPost("{commentId}/report")]
    public async Task<ActionResult<ApiResponse>> ReportComment(
        Guid commentId,
        [FromQuery] Guid reporterId,
        [FromBody] ReportReason reason)
    {
        var success = await _commentService.ReportComment(commentId, reporterId, reason);

        if (!success)
        {
            return BadRequest(ApiResponse.Fail("举报失败"));
        }

        return Ok(ApiResponse.Ok("举报已提交"));
    }
}
