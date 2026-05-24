using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Services;

namespace MinecraftLauncher.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersionsController : ControllerBase
    {
        private readonly IVersionService _versionService;
        private readonly ILogger<VersionsController> _logger;

        public VersionsController(IVersionService versionService, ILogger<VersionsController> logger)
        {
            _versionService = versionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetInstalledVersions()
        {
            try
            {
                var versions = await _versionService.GetInstalledVersions();
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取已安装版本失败");
                return StatusCode(500, new { message = "获取已安装版本失败" });
            }
        }

        [HttpGet("{versionId}")]
        public async Task<IActionResult> GetVersionById(string versionId)
        {
            try
            {
                var version = await _versionService.GetVersionById(versionId);
                if (version == null || !version.IsValid)
                {
                    return NotFound(new { message = $"版本 {versionId} 不存在或无效" });
                }
                return Ok(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取版本详情失败");
                return StatusCode(500, new { message = "获取版本详情失败" });
            }
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanVersions([FromQuery] string gameRootPath)
        {
            try
            {
                if (string.IsNullOrEmpty(gameRootPath))
                {
                    return BadRequest(new { message = "游戏根目录路径不能为空" });
                }

                var hasVersions = await _versionService.ScanVersions(gameRootPath);
                return Ok(new { 
                    hasVersions,
                    message = hasVersions ? "发现已安装的版本" : "未发现已安装的版本"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "扫描版本失败");
                return StatusCode(500, new { message = "扫描版本失败" });
            }
        }
    }
}
