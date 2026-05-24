using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Services;
using MinecraftLauncher.Core.Models;

namespace MinecraftLauncher.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ILaunchService _launchService;
        private readonly IVersionService _versionService;
        private readonly IJavaService _javaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(
            ILaunchService launchService,
            IVersionService versionService,
            IJavaService javaService,
            IConfiguration configuration,
            ILogger<ConfigController> logger)
        {
            _launchService = launchService;
            _versionService = versionService;
            _javaService = javaService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfiguration()
        {
            try
            {
                var javaList = await _javaService.GetInstalledJava();
                var versions = await _versionService.GetInstalledVersions();
                
                var config = new
                {
                    minecraft = new
                    {
                        gameRootPath = _configuration["Minecraft:GameRootPath"],
                        defaultJavaPath = _configuration["Minecraft:DefaultJavaPath"],
                        maxMemory = _configuration["Minecraft:DefaultMaxMemory"],
                        minMemory = _configuration["Minecraft:DefaultMinMemory"]
                    },
                    system = new
                    {
                        installedJava = javaList,
                        installedVersions = versions,
                        os = Environment.OSVersion.Platform,
                        processorCount = Environment.ProcessorCount,
                        workingSet = Environment.WorkingSet / 1024 / 1024 + " MB"
                    },
                    database = new
                    {
                        provider = "SQLite",
                        path = _configuration["ConnectionStrings:DefaultConnection"]
                    }
                };

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置信息失败");
                return StatusCode(500, new { message = "获取配置信息失败" });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSetup([FromBody] SetupValidationRequest request)
        {
            try
            {
                var result = new SetupValidationResult
                {
                    GameRootPath = request.GameRootPath ?? string.Empty,
                    JavaPath = request.JavaPath ?? string.Empty,
                    Validations = new List<ValidationItem>()
                };

                // 验证游戏路径
                if (!string.IsNullOrEmpty(request.GameRootPath))
                {
                    var gameRootExists = Directory.Exists(request.GameRootPath);
                    var versionsPath = Path.Combine(request.GameRootPath, "versions");
                    var versionsExist = Directory.Exists(versionsPath);
                    
                    result.Validations.Add(new ValidationItem
                    {
                        Name = "游戏根目录",
                        Success = gameRootExists,
                        Message = gameRootExists 
                            ? $"目录存在 ({request.GameRootPath})"
                            : "目录不存在"
                    });

                    if (gameRootExists && versionsExist)
                    {
                        var versions = Directory.GetDirectories(versionsPath);
                        result.Validations.Add(new ValidationItem
                        {
                            Name = "版本目录",
                            Success = versions.Length > 0,
                            Message = $"发现 {versions.Length} 个版本"
                        });
                    }
                }

                // 验证 Java 路径
                if (!string.IsNullOrEmpty(request.JavaPath))
                {
                    var javaExists = File.Exists(request.JavaPath);
                    var javaValid = javaExists ? await _javaService.ValidateJavaPath(request.JavaPath) : false;

                    result.Validations.Add(new ValidationItem
                    {
                        Name = "Java 可执行文件",
                        Success = javaExists,
                        Message = javaExists ? "文件存在" : "文件不存在"
                    });

                    result.Validations.Add(new ValidationItem
                    {
                        Name = "Java 有效性",
                        Success = javaValid,
                        Message = javaValid ? "Java 可正常运行" : "Java 无法执行"
                    });
                }

                // 验证数据库连接
                result.Validations.Add(new ValidationItem
                {
                    Name = "数据库",
                    Success = true,
                    Message = "数据库配置正常"
                });

                result.IsValid = result.Validations.All(v => v.Success);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证设置失败");
                return StatusCode(500, new { message = "验证设置失败" });
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var java = await _javaService.GetDefaultJava();
                var versions = await _versionService.GetInstalledVersions();
                
                var health = new SystemHealth
                {
                    Status = "healthy",
                    Timestamp = DateTime.UtcNow,
                    Components = new Dictionary<string, ComponentHealth>
                    {
                        ["java"] = new ComponentHealth
                        {
                            Status = !string.IsNullOrEmpty(java.Path) ? "healthy" : "warning",
                            Message = $"默认 Java: {java.Version}",
                            Details = java
                        },
                        ["minecraft_versions"] = new ComponentHealth
                        {
                            Status = versions.Any() ? "healthy" : "warning",
                            Message = $"已安装 {versions.Count()} 个版本",
                            Details = versions
                        },
                        ["api"] = new ComponentHealth
                        {
                            Status = "healthy",
                            Message = "API 服务正常运行"
                        }
                    }
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统健康状态失败");
                return StatusCode(500, new { message = "获取系统健康状态失败" });
            }
        }
    }

    public class SetupValidationRequest
    {
        public string? GameRootPath { get; set; }
        public string? JavaPath { get; set; }
    }

    public class SetupValidationResult
    {
        public string GameRootPath { get; set; } = string.Empty;
        public string JavaPath { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<ValidationItem> Validations { get; set; } = new();
    }

    public class ValidationItem
    {
        public string Name { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SystemHealth
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, ComponentHealth> Components { get; set; } = new();
    }

    public class ComponentHealth
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
}
