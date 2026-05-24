using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Common;
using MinecraftLauncher.Core.DTOs.Report;
using System.Security.Claims;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable>>>> GetNotifications(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotifications(userId, pageIndex, pageSize);
        return Ok(ApiResponse<IEnumerable>.Ok(notifications));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCount(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPost("{notificationId}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(Guid notificationId)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAsRead(notificationId, userId);
        return Ok(ApiResponse.Ok("通知已标记为已读"));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsRead(userId);
        return Ok(ApiResponse.Ok("所有通知已标记为已读"));
    }
}
