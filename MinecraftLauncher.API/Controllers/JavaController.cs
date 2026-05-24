using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Services;

namespace MinecraftLauncher.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JavaController : ControllerBase
    {
        private readonly IJavaService _javaService;
        private readonly ILogger<JavaController> _logger;

        public JavaController(IJavaService javaService, ILogger<JavaController> logger)
        {
            _javaService = javaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetInstalledJava()
        {
            try
            {
                var javaList = await _javaService.GetInstalledJava();
                return Ok(javaList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取已安装 Java 列表失败");
                return StatusCode(500, new { message = "获取已安装 Java 列表失败" });
            }
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultJava()
        {
            try
            {
                var java = await _javaService.GetDefaultJava();
                return Ok(java);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取默认 Java 失败");
                return StatusCode(500, new { message = "获取默认 Java 失败" });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateJavaPath([FromBody] ValidateJavaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.JavaPath))
                {
                    return BadRequest(new { message = "Java 路径不能为空" });
                }

                var isValid = await _javaService.ValidateJavaPath(request.JavaPath);
                return Ok(new { 
                    isValid,
                    message = isValid ? "Java 路径有效" : "Java 路径无效"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证 Java 路径失败");
                return StatusCode(500, new { message = "验证 Java 路径失败" });
            }
        }
    }

    public class ValidateJavaRequest
    {
        public string JavaPath { get; set; } = string.Empty;
    }
}
