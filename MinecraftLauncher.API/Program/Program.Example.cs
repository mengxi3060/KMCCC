// Program.cs 示例 - 展示如何配置 Minecraft Launcher 服务
// 注意: 这是参考文件，根据实际需求修改

using Microsoft.EntityFrameworkCore;
using MinecraftLauncher.Core.Services;
using MinecraftLauncher.Core.Services.Launch;
using MinecraftLauncher.Infrastructure.Data;
using MinecraftLauncher.Infrastructure.Services;
using MinecraftLauncher.Infrastructure.Services.Launch;

// 创建一个 WebApplicationBuilder
var builder = WebApplication.CreateBuilder(args);

// 配置 Minecraft 路径
var gameRootPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    ".minecraft"
);

var javaPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
    "Java",
    "jdk-17",
    "bin",
    "java.exe"
);

// 使用 SQLite 作为数据库
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MinecraftLauncher",
    "launcher.db"
);

// 确保目录存在
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// 添加 Minecraft Launcher 服务
builder.Services.AddMinecraftLauncherServicesWithSqlite(
    dbPath,
    gameRootPath,
    javaPath
);

// 添加控制器
builder.Services.AddControllers();

// 添加 Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Minecraft Launcher API",
        Version = "v1",
        Description = "Minecraft 第三方启动器 API"
    });
});

// 添加 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DatabaseInitializer.InitializeAsync(services);
        await DatabaseInitializer.SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "数据库初始化失败");
    }
}

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// 配置监听地址
app.Urls.Add("http://0.0.0.0:5000");

app.Run();
