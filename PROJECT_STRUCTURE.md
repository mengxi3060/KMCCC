# Minecraft Launcher 项目结构总结

## 📁 项目目录结构

```
/workspace/
├── MinecraftLauncher.sln                    # 主解决方案文件
├── MinecraftLauncher_README.md              # 项目说明文档
│
├── MinecraftLauncher.Core/                  # 核心业务逻辑层
│   ├── MinecraftLauncher.Core.csproj       # Core 项目文件
│   ├── Entities/                            # 实体类定义
│   │   ├── User.cs                         # 用户实体
│   │   ├── UserProfile.cs                  # 用户资料实体
│   │   ├── Resource.cs                     # 资源实体
│   │   ├── ResourceCompatibility.cs        # 资源兼容性实体
│   │   ├── Comment.cs                      # 评论实体
│   │   ├── ReviewRecord.cs                 # 审核记录实体
│   │   ├── Report.cs                       # 举报实体
│   │   ├── Violation.cs                    # 违规记录实体
│   │   ├── AuditLog.cs                     # 审计日志实体
│   │   └── Notification.cs                 # 通知实体
│   ├── Models/                              # 数据模型
│   │   ├── LaunchModels.cs                 # 启动相关模型
│   │   └── Version.cs                      # 版本模型
│   └── Services/                            # 服务接口
│       ├── ILaunchService.cs               # 启动服务接口
│       ├── IVersionService.cs               # 版本服务接口
│       └── IJavaService.cs                  # Java服务接口
│
├── MinecraftLauncher.Infrastructure/        # 基础设施层
│   ├── MinecraftLauncher.Infrastructure.csproj
│   ├── Data/                                # 数据访问层
│   │   ├── AppDbContext.cs                 # EF Core 数据库上下文
│   │   └── DatabaseInitializer.cs          # 数据库初始化和种子数据
│   ├── Services/                            # 服务实现
│   │   ├── Launch/                         # 启动相关服务
│   │   │   └── KMCCCLaunchService.cs       # KMCCC 启动服务实现
│   │   ├── VersionService.cs                # 版本服务实现
│   │   └── JavaService.cs                   # Java服务实现
│   └── ServiceCollectionExtensions.cs       # 服务注册扩展方法
│
├── MinecraftLauncher.API/                   # Web API 层
│   ├── MinecraftLauncher.API.csproj        # API 项目文件
│   ├── appsettings.json                    # 应用配置文件
│   ├── appsettings.Development.json        # 开发环境配置
│   ├── Properties/
│   │   └── launchSettings.json             # 启动配置
│   └── Controllers/                        # API 控制器
│       ├── LaunchController.cs             # 游戏启动控制器
│       ├── VersionsController.cs           # 版本管理控制器
│       ├── JavaController.cs               # Java管理控制器
│       └── HealthController.cs             # 健康检查控制器
│
├── KMCCC.Basic/                             # KMCCC 基础库
├── KMCCC.Pro/                               # KMCCC 专业版
├── KMCCC.Shared/                            # KMCCC 共享库
└── KMCCC.Simple/                            # KMCCC 简单示例
```

## 🎯 核心功能模块

### 1. KMCCC 启动服务 (KMCCCLaunchService)

**功能特性：**
- ✅ 封装 KMCCC 启动核心库
- ✅ 支持离线游戏启动
- ✅ 支持 Yggdrasil 正版登录
- ✅ 自定义启动选项（内存、窗口大小等）
- ✅ 游戏日志和退出事件处理

**使用示例：**
```csharp
// 注入服务
services.AddScoped<ILaunchService>(provider =>
    new KMCCCLaunchService(gameRootPath, javaPath));

// 离线启动
var result = await launchService.LaunchWithOfflineAuth("1.20.1", "PlayerName");

// 正版启动
var result = await launchService.LaunchWithYggdrasilAuth(
    "1.20.1", 
    "email@example.com", 
    "password"
);
```

### 2. 版本管理服务 (VersionService)

**功能特性：**
- ✅ 自动扫描已安装的游戏版本
- ✅ 获取版本详细信息
- ✅ 版本有效性检查
- ✅ 版本大小统计

**使用示例：**
```csharp
services.AddScoped<IVersionService>(provider =>
    new VersionService(gameRootPath));

// 获取所有已安装版本
var versions = await versionService.GetInstalledVersions();

// 扫描游戏目录
var hasVersions = await versionService.ScanVersions(gameRootPath);
```

### 3. Java 管理服务 (JavaService)

**功能特性：**
- ✅ 自动检测系统已安装的 Java
- ✅ 跨平台支持（Windows/Linux/macOS）
- ✅ Java 路径验证
- ✅ 获取默认 Java 配置

**使用示例：**
```csharp
services.AddSingleton<IJavaService>(provider =>
    new JavaService());

// 获取所有已安装的 Java
var javaList = await javaService.GetInstalledJava();

// 验证 Java 路径
var isValid = await javaService.ValidateJavaPath("/path/to/java");
```

### 4. Entity Framework Core 数据访问

**功能特性：**
- ✅ SQL Server 支持
- ✅ SQLite 支持（开箱即用）
- ✅ 自动数据库迁移
- ✅ 种子数据初始化
- ✅ 完整的关系映射配置

**配置示例：**
```csharp
// SQLite 配置
builder.Services.AddMinecraftLauncherServicesWithSqlite(
    "launcher.db",
    gameRootPath,
    javaPath
);

// SQL Server 配置
builder.Services.AddMinecraftLauncherServices(
    "Server=.;Database=LauncherDB;Trusted_Connection=True;",
    gameRootPath,
    javaPath
);

// 数据库初始化
await DatabaseInitializer.InitializeAsync(app.Services);
await DatabaseInitializer.SeedDataAsync(app.Services);
```

## 📊 数据库架构

### 核心实体关系

```
User (用户)
├── UserProfile (用户资料) - 1:1
├── Resource (资源) - 1:N
├── Comment (评论) - 1:N
└── Violation (违规记录) - 1:N

Resource (资源)
├── ResourceCompatibility (兼容性) - 1:N
├── Comment (评论) - 1:N
└── ReviewRecord (审核记录) - 1:N

Comment (评论)
└── Comment (回复) - 自引用 1:N

Violation (违规记录)
├── User (被处罚用户) - N:1
└── User (处理人) - N:1

AuditLog (审计日志) - N:1 -> User
Notification (通知) - N:1 -> User
Report (举报) - N:1 -> User, Resource
```

## 🔌 API 端点概览

### LaunchController (`/api/launch`)
- `GET /versions` - 获取所有可用版本
- `GET /versions/{id}` - 获取特定版本
- `POST /offline` - 离线启动游戏
- `POST /yggdrasil` - 正版登录启动
- `POST /custom` - 自定义启动

### VersionsController (`/api/versions`)
- `GET /` - 获取已安装版本
- `GET /{id}` - 获取版本详情
- `POST /scan` - 扫描游戏目录

### JavaController (`/api/java`)
- `GET /` - 获取已安装 Java
- `GET /default` - 获取默认 Java
- `POST /validate` - 验证 Java 路径

### HealthController (`/api/health`)
- `GET /` - 健康检查

## 🛠️ 开发指南

### 1. 环境要求
- .NET 8.0 SDK
- SQL Server 2019+ 或 SQLite
- Minecraft 游戏文件

### 2. 快速开始

```bash
# 克隆项目
cd /workspace

# 还原依赖
dotnet restore MinecraftLauncher.sln

# 构建项目
dotnet build MinecraftLauncher.sln

# 运行 API 项目
cd MinecraftLauncher.API
dotnet run
```

### 3. 配置说明

编辑 `MinecraftLauncher.API/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LauncherDB;..."
  },
  "Minecraft": {
    "GameRootPath": "C:\\Users\\Public\\Minecraft",
    "DefaultJavaPath": "C:\\Program Files\\Java\\jdk-17\\bin\\java.exe",
    "DefaultMaxMemory": 2048,
    "DefaultMinMemory": 512
  }
}
```

### 4. 默认账号

| 角色 | 邮箱 | 密码 |
|------|------|------|
| 管理员 | admin@example.com | Admin@123 |
| 版主 | mod@example.com | Mod@123 |
| 用户 | user@example.com | User@123 |

## 📦 依赖项

### MinecraftLauncher.Core
- System.Runtime.InteropServices

### MinecraftLauncher.Infrastructure
- Microsoft.EntityFrameworkCore (8.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.EntityFrameworkCore.Design (8.0.0)
- Microsoft.Extensions.DependencyInjection (8.0.0)
- Microsoft.Extensions.Logging (8.0.0)
- BCrypt.Net-Next (4.0.3)
- KMCCC.Pro
- KMCCC.Shared

### MinecraftLauncher.API
- Microsoft.AspNetCore.OpenApi (8.0.0)
- Swashbuckle.AspNetCore (6.5.0)

## 🎨 架构优势

1. **分层架构** - 清晰的分层设计，易于维护和扩展
2. **依赖注入** - 完全支持依赖注入，便于测试
3. **接口抽象** - 核心服务通过接口抽象，支持多种实现
4. **EF Core** - 现代化 ORM，支持多种数据库
5. **KMCCC 集成** - 成熟的 Minecraft 启动核心库
6. **RESTful API** - 标准化的 Web API 设计
7. **跨平台** - 支持 Windows、Linux、macOS

## 📝 扩展建议

### 添加新的启动器支持
```csharp
public class CustomLaunchService : ILaunchService
{
    // 实现 ILaunchService 接口
}
```

### 添加新的数据库支持
```csharp
// 在 ServiceCollectionExtensions 中添加
services.AddDbContext<AppDbContext>(options =>
    options.UsePomelo(connectionString)); // MySQL
```

### 添加缓存层
```csharp
services.AddMemoryCache();
services.AddScoped<IVersionService, CachedVersionService>();
```

## 🔒 安全建议

1. **敏感配置** - 使用环境变量或密钥管理服务
2. **密码存储** - 使用 BCrypt 等强哈希算法
3. **JWT 配置** - 生产环境使用强密钥和短过期时间
4. **数据库连接** - 使用加密连接和最小权限原则
5. **API 限流** - 实现请求频率限制防止滥用

## 📞 技术支持

如有问题，请查看：
- [KMCCC GitHub](https://github.com)
- [Entity Framework Core 文档](https://docs.microsoft.com/ef/core)
- [ASP.NET Core 文档](https://docs.microsoft.com/aspnet/core)

## 📄 许可证

MIT License - 详见 LICENSE 文件
