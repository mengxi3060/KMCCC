using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Common;
using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LaunchController : ControllerBase
{
    private readonly IVersionService _versionService;
    private readonly ILaunchService _launchService;
    private readonly IJavaService _javaService;

    public LaunchController(
        IVersionService versionService,
        ILaunchService launchService,
        IJavaService javaService)
    {
        _versionService = versionService;
        _launchService = launchService;
        _javaService = javaService;
    }

    [HttpGet("versions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GameVersion>>>> GetInstalledVersions()
    {
        var versions = await _versionService.GetInstalledVersions();
        return Ok(ApiResponse<IEnumerable<GameVersion>>.Ok(versions));
    }

    [HttpGet("versions/{versionId}")]
    public async Task<ActionResult<ApiResponse<GameVersion>>> GetVersionById(string versionId)
    {
        var version = await _versionService.GetVersionById(versionId);
        return Ok(ApiResponse<GameVersion>.Ok(version));
    }

    [HttpPost("versions/scan")]
    public async Task<ActionResult<ApiResponse>> ScanVersions([FromQuery] string gameRootPath)
    {
        var success = await _versionService.ScanVersions(gameRootPath);

        if (!success)
        {
            return BadRequest(ApiResponse.Fail("扫描失败"));
        }

        return Ok(ApiResponse.Ok("扫描完成"));
    }

    [HttpPost("launch")]
    public async Task<ActionResult<ApiResponse<LaunchResult>>> LaunchGame([FromBody] LaunchOptions options)
    {
        var result = await _launchService.LaunchGame(options);
        return Ok(ApiResponse<LaunchResult>.Ok(result));
    }

    [HttpPost("launch/offline")]
    public async Task<ActionResult<ApiResponse<LaunchResult>>> LaunchWithOfflineAuth(
        [FromQuery] string versionId,
        [FromQuery] string playerName)
    {
        var result = await _launchService.LaunchWithOfflineAuth(versionId, playerName);
        return Ok(ApiResponse<LaunchResult>.Ok(result));
    }

    [HttpPost("launch/yggdrasil")]
    public async Task<ActionResult<ApiResponse<LaunchResult>>> LaunchWithYggdrasilAuth(
        [FromQuery] string versionId,
        [FromQuery] string email,
        [FromQuery] string password)
    {
        var result = await _launchService.LaunchWithYggdrasilAuth(versionId, email, password);
        return Ok(ApiResponse<LaunchResult>.Ok(result));
    }

    [HttpGet("java")]
    public async Task<ActionResult<ApiResponse<IEnumerable<JavaInfo>>>> GetInstalledJava()
    {
        var javaList = await _javaService.GetInstalledJava();
        return Ok(ApiResponse<IEnumerable<JavaInfo>>.Ok(javaList));
    }

    [HttpGet("java/default")]
    public async Task<ActionResult<ApiResponse<JavaInfo>>> GetDefaultJava()
    {
        var java = await _javaService.GetDefaultJava();
        return Ok(ApiResponse<JavaInfo>.Ok(java));
    }

    [HttpPost("java/validate")]
    public async Task<ActionResult<ApiResponse>> ValidateJavaPath([FromQuery] string javaPath)
    {
        var valid = await _javaService.ValidateJavaPath(javaPath);

        if (!valid)
        {
            return BadRequest(ApiResponse.Fail("Java 路径无效"));
        }

        return Ok(ApiResponse.Ok("Java 路径有效"));
    }
}
