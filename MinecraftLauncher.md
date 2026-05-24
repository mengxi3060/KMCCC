# Minecraft Launcher Community Edition - ASP.NET Core 8.0 Web API

Minecraft 第三方启动器社区版 ASP.NET Core 8.0 Web API 项目。

## 项目结构

```
/workspace/MinecraftLauncher/
├── MinecraftLauncher.Core/           # 核心类库
│   ├── Domain/
│   │   ├── Entities/                 # 实体类
│   │   ├── Enums/                    # 枚举类型
│   │   └── Interfaces/               # 服务接口
│   └── DTOs/                         # 数据传输对象
│       ├── Auth/
│       ├── Resource/
│       ├── Launch/
│       ├── Review/
│       ├── Comment/
│       ├── Report/
│       ├── Admin/
│       └── Common/
│
├── MinecraftLauncher.Infrastructure/  # 基础设施类库
│   ├── Data/                         # DbContext 和数据访问
│   ├── Repositories/                 # 仓储实现
│   └── Services/                    # 基础设施服务实现
│
└── MinecraftLauncher.API/             # Web API 项目
    ├── Controllers/                  # API 控制器
    ├── Middleware/                   # 中间件
    ├── Configuration/                # 配置
    └── Program.cs                    # 应用入口
```

## 项目功能

### 1. MinecraftLauncher.Core - 核心类库

包含所有领域模型、枚举、接口和 DTOs。

#### 实体类 (Domain/Entities/)
- **User** - 用户实体
- **UserProfile** - 用户资料
- **Resource** - 资源实体 (模组/整合包/光影/材质)
- **ResourceCompatibility** - 资源兼容性
- **Comment** - 评论
- **ReviewRecord** - 审核记录
- **Report** - 举报
- **Violation** - 违规记录
- **AuditLog** - 审计日志
- **Notification** - 通知

#### 枚举类 (Domain/Enums/)
- **ResourceType** - 资源类型 (Mod, Modpack, Shader, TexturePack)
- **LoaderType** - 加载器类型 (Forge, Fabric, Quilt, OptiFine等)
- **ResourceStatus** - 资源状态 (Pending, Approved, Rejected, Frozen, Removed)
- **ReviewAction** - 审核动作 (Approve, Reject, Freeze)
- **ReportType** - 举报类型
- **ReportStatus** - 举报状态
- **ViolationType** - 违规类型
- **ViolationSeverity** - 违规严重程度
- **NotificationType** - 通知类型

#### 服务接口 (Domain/Interfaces/)
- **IAuthService** - 认证服务
- **IVersionService** - 游戏版本服务
- **ILaunchService** - 游戏启动服务
- **IJavaService** - Java 管理服务
- **IResourceService** - 资源服务
- **IResourceUploadService** - 资源上传服务
- **IReviewService** - 审核服务
- **IDownloadService** - 下载服务
- **ICommentService** - 评论服务
- **IReportService** - 举报服务
- **IAdminService** - 管理员服务
- **IAuditLogService** - 审计日志服务

### 2. MinecraftLauncher.Infrastructure - 基础设施类库

包含数据库访问、文件存储和第三方服务集成。

- **AppDbContext** - Entity Framework Core 数据库上下文
- **Services/** - 所有服务实现

### 3. MinecraftLauncher.API - Web API 项目

包含所有 API 控制器、中间件和配置。

#### API 控制器 (Controllers/)
- **AuthController** - 认证接口 (注册、登录、登出)
- **ResourceController** - 资源管理 (浏览、创建、更新、删除、上传、下载)
- **ReviewController** - 审核管理 (审核队列、通过、驳回、冻结)
- **CommentController** - 评论管理 (获取、添加、删除、举报)
- **ReportController** - 举报管理 (提交、处理)
- **AdminController** - 管理员接口 (用户管理、资源管理)
- **LaunchController** - 游戏启动 (版本管理、启动游戏)

#### 中间件 (Middleware/)
- **ExceptionHandlingMiddleware** - 全局异常处理
- **RequestLoggingMiddleware** - 请求日志记录
- **JwtAuthenticationExtensions** - JWT 认证配置

## 技术栈

- **.NET 8.0**
- **ASP.NET Core 8.0 Web API**
- **Entity Framework Core 8.0** + SQLite
- **JWT Bearer Authentication**
- **BCrypt** - 密码哈希
- **Swashbuckle** - Swagger/OpenAPI 文档

## 快速开始

### 1. 创建解决方案

```bash
cd /workspace
dotnet new sln -n MinecraftLauncher
dotnet sln add MinecraftLauncher.Core/MinecraftLauncher.Core.csproj
dotnet sln add MinecraftLauncher.Infrastructure/MinecraftLauncher.Infrastructure.csproj
dotnet sln add MinecraftLauncher.API/MinecraftLauncher.API.csproj
```

### 2. 还原依赖

```bash
dotnet restore
```

### 3. 构建项目

```bash
dotnet build
```

### 4. 运行应用

```bash
cd MinecraftLauncher.API
dotnet run
```

应用将在 `http://localhost:5000` 启动，Swagger UI 可在 `/swagger` 访问。

## API 端点

### 认证接口
- `POST /api/auth/register` - 用户注册
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/logout` - 用户登出
- `GET /api/auth/me` - 获取当前用户信息

### 资源接口
- `GET /api/resource` - 获取资源列表
- `GET /api/resource/{id}` - 获取资源详情
- `POST /api/resource` - 创建资源
- `PUT /api/resource/{id}` - 更新资源
- `DELETE /api/resource/{id}` - 删除资源
- `GET /api/resource/my` - 获取我的资源
- `POST /api/resource/upload/init` - 初始化上传
- `GET /api/resource/upload/{id}/progress` - 获取上传进度
- `POST /api/resource/upload/{id}/complete` - 完成上传
- `POST /api/resource/{id}/download` - 下载资源
- `POST /api/resource/{id}/install` - 安装资源

### 审核接口
- `GET /api/review/queue` - 获取审核队列
- `GET /api/review/{id}` - 获取审核详情
- `POST /api/review/{id}/approve` - 通过审核
- `POST /api/review/{id}/reject` - 驳回审核
- `POST /api/review/{id}/freeze` - 冻结资源

### 评论接口
- `GET /api/comment/resource/{id}` - 获取资源评论
- `POST /api/comment` - 添加评论
- `DELETE /api/comment/{id}` - 删除评论
- `POST /api/comment/{id}/report` - 举报评论

### 举报接口
- `POST /api/report` - 提交举报
- `GET /api/report/pending` - 获取待处理举报
- `POST /api/report/{id}/resolve` - 处理举报

### 管理员接口
- `GET /api/admin/users` - 获取用户列表
- `POST /api/admin/users/{id}/ban` - 封禁用户
- `POST /api/admin/users/{id}/unban` - 解封用户
- `GET /api/admin/resources` - 获取所有资源
- `POST /api/admin/resources/{id}/top` - 设置置顶
- `POST /api/admin/resources/{id}/recommend` - 设置推荐
- `POST /api/admin/resources/{id}/unlist` - 下架资源

### 游戏启动接口
- `GET /api/launch/versions` - 获取已安装版本
- `GET /api/launch/versions/{id}` - 获取版本详情
- `POST /api/launch/versions/scan` - 扫描版本
- `POST /api/launch` - 启动游戏
- `POST /api/launch/offline` - 离线启动
- `POST /api/launch/yggdrasil` - Yggdrasil 启动
- `GET /api/launch/java` - 获取 Java 列表
- `GET /api/launch/java/default` - 获取默认 Java
- `POST /api/launch/java/validate` - 验证 Java 路径

## 数据库

项目使用 SQLite 数据库，数据库文件位于 `minecraft_launcher.db`。

Entity Framework Core 会自动创建数据库表结构。

## 配置

配置文件位于 `MinecraftLauncher.API/appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "YourSuperSecretKeyForJwtTokenGeneration2024!",
    "Issuer": "MinecraftLauncher"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=minecraft_launcher.db"
  }
}
```

## 开发指南

### 添加新服务

1. 在 `MinecraftLauncher.Core/Domain/Interfaces/` 创建接口
2. 在 `MinecraftLauncher.Infrastructure/Services/` 实现服务
3. 在 `MinecraftLauncher.API/Program.cs` 注册服务

### 添加新控制器

1. 在 `MinecraftLauncher.API/Controllers/` 创建控制器
2. 继承 `ControllerBase`
3. 使用 `[ApiController]` 和 `[Route]` 属性

## 许可证

本项目仅供学习和研究使用。
