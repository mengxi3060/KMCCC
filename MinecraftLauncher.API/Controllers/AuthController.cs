using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Auth;
using MinecraftLauncher.Core.DTOs.Common;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResult>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.Register(request);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<AuthResult>.Fail(result.Error!));
        }

        return Ok(ApiResponse<AuthResult>.Ok(result, "注册成功"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResult>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.Login(request);

        if (!result.Success)
        {
            return Unauthorized(ApiResponse<AuthResult>.Fail(result.Error!));
        }

        return Ok(ApiResponse<AuthResult>.Ok(result, "登录成功"));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        await _authService.Logout();
        return Ok(ApiResponse.Ok("登出成功"));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUser();
        return Ok(ApiResponse<UserInfo>.Ok(user));
    }
}
