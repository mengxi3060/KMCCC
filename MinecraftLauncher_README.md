# Minecraft 第三方启动器社区版

## 🎮 项目概述

这是一个功能完整的 Minecraft 第三方启动器,基于 KMCCC 启动核心,新增资源分享社区模块,支持玩家上传、分享游戏资源(模组、整合包、光影包、材质包),并提供完善的审核机制和管理后台。

## 📦 技术栈

- **后端框架**: ASP.NET Core 8.0
- **启动核心**: KMCCC (KMCCC.Shared/KMCCC.Pro)
- **数据库**: SQLite (开发环境) / PostgreSQL (生产环境)
- **ORM**: Entity Framework Core 8.0
- **身份认证**: JWT Token + BCrypt
- **API 文档**: Swagger/OpenAPI

## 🏗️ 项目结构

```
MinecraftLauncher/
├── MinecraftLauncher.Core/           # 核心业务层
│   ├── Domain/
│   │   ├── Entities/               # 实体类
│   │   └── Interfaces/             # 服务接口
│   ├── DTOs/                       # 数据传输对象
│   │   ├── Auth/
│   │   ├── Resource/
│   │   ├── Upload/
│   │   ├── Download/
│   │   ├── Review/
│   │   ├── Comment/
│   │   ├── Report/
│   │   ├── Admin/
│   │   └── Common/
│   └── Enums/                      # 枚举类型
│
├── MinecraftLauncher.Infrastructure/ # 基础设施层
│   ├── Data/                       # 数据库上下文
│   ├── Services/                   # 服务实现
│   │   ├── Launch/                # 启动服务
│   │   ├── AuthService.cs
│   │   ├── VersionService.cs
│   │   ├── JavaService.cs
│   │   ├── ResourceService.cs
│   │   ├── ResourceUploadService.cs
│   │   ├── DownloadService.cs
│   │   ├── ReviewService.cs
│   │   ├── CommentService.cs
│   │   ├── ReportService.cs
│   │   ├── NotificationService.cs
│   │   ├── ViolationService.cs
│   │   ├── AdminService.cs
│   │   └── AuditLogService.cs
│   └── ServiceCollectionExtensions.cs
│
└── MinecraftLauncher.API/          # Web API 层
    ├── Controllers/                # API 控制器
    │   ├── AuthController.cs
    │   ├── LaunchController.cs
    │   ├── VersionController.cs
    │   ├── JavaController.cs
    │   ├── ResourceController.cs
    │   ├── ReviewController.cs
    │   ├── CommentController.cs
    │   ├── ReportController.cs
    │   ├── AdminController.cs
    │   └── NotificationController.cs
    ├── Middleware/                 # 中间件
    ├── Program.cs                 # 应用入口
    └── appsettings.json
```

## 🚀 快速开始

### 环境要求

- .NET 8.0 SDK
- Node.js 18+ (前端开发)
- SQLite (开发) / PostgreSQL 14+ (生产)

### 构建项目

```bash
# 1. 克隆项目
git clone <repository-url>
cd MinecraftLauncher

# 2. 还原依赖
dotnet restore MinecraftLauncher.sln

# 3. 构建项目
dotnet build MinecraftLauncher.sln

# 4. 运行应用
cd MinecraftLauncher.API
dotnet run
```

### 访问应用

- API 地址: http://localhost:5000
- Swagger 文档: http://localhost:5000/swagger
- 健康检查: http://localhost:5000/api/health

## 📚 默认测试账号

| 角色 | 邮箱 | 密码 | 说明 |
|------|------|------|------|
| 管理员 | admin@example.com | Admin@123 | 完全权限 |
| 版主 | mod@example.com | Mod@123 | 审核权限 |
| 用户 | user@example.com | User@123 | 基本权限 |

## 🔐 API 接口概览

### 认证接口

```
POST   /api/auth/register        # 用户注册
POST   /api/auth/login           # 用户登录
POST   /api/auth/logout          # 用户登出
GET    /api/auth/me              # 获取当前用户信息
POST   /api/auth/refresh         # 刷新Token
```

### 启动器核心接口

```
GET    /api/launcher/versions         # 获取已安装版本列表
GET    /api/launcher/versions/{id}    # 获取指定版本信息
POST   /api/launcher/launch           # 启动游戏
GET    /api/launcher/java             # 获取Java环境列表
POST   /api/launcher/scan             # 扫描游戏目录
```

### 资源管理接口

```
GET    /api/resources                  # 浏览资源列表
GET    /api/resources/{id}             # 获取资源详情
POST   /api/resources                 # 上传资源(需登录)
PUT    /api/resources/{id}            # 更新资源(需登录,仅作者)
DELETE /api/resources/{id}            # 删除资源(需登录,仅作者)
POST   /api/resources/{id}/download   # 下载资源
POST   /api/resources/{id}/install    # 一键安装
GET    /api/resources/my              # 获取我的上传列表(需登录)
```

### 审核管理接口

```
GET    /api/admin/review/queue              # 获取审核队列
GET    /api/admin/review/{resourceId}       # 获取审核详情
POST   /api/admin/review/{resourceId}/approve   # 通过审核
POST   /api/admin/review/{resourceId}/reject    # 驳回审核
POST   /api/admin/review/{resourceId}/freeze    # 冻结资源
GET    /api/admin/review/history             # 审核历史记录
```

### 管理员后台接口

```
GET    /api/admin/dashboard/stats            # 仪表盘统计
GET    /api/admin/users                      # 用户管理列表
POST   /api/admin/users/{id}/ban             # 封禁用户
POST   /api/admin/users/{id}/unban           # 解封用户
PUT    /api/admin/users/{id}/role            # 更新用户角色
GET    /api/admin/resources                   # 资源管理列表
PUT    /api/admin/resources/{id}/top         # 设置置顶
PUT    /api/admin/resources/{id}/recommend   # 设置推荐
DELETE /api/admin/resources/{id}             # 下架资源
GET    /api/admin/audit-logs                # 审计日志
```

## ✨ 核心功能

### 1. 游戏启动核心

- ✅ 离线模式启动
- ✅ 正版登录启动 (Yggdrasil)
- ✅ 游戏版本扫描和管理
- ✅ Java 环境自动检测
- ✅ 自定义启动参数

### 2. 资源分享系统

- ✅ 四类资源支持: 模组、整合包、光影包、材质包
- ✅ 大文件分片上传
- ✅ 自动文件校验 (格式、大小、完整性)
- ✅ 恶意代码检测
- ✅ 资源审核流程

### 3. 用户系统

- ✅ 用户注册/登录
- ✅ JWT Token 认证
- ✅ 基于角色的权限控制 (User/Moderator/Admin)
- ✅ 用户资料管理
- ✅ 违规次数追踪

### 4. 审核系统

- ✅ 多维度审核 (合规性、技术性、安全性、完整性)
- ✅ 审核通过/驳回/冻结操作
- ✅ 审核历史记录
- ✅ 自动通知用户

### 5. 管理员后台

- ✅ 用户管理 (封禁/解封/角色调整)
- ✅ 资源管理 (置顶/推荐/下架)
- ✅ 仪表盘统计
- ✅ 审计日志查询

### 6. 安全特性

- ✅ 密码 BCrypt 加密
- ✅ JWT Token 安全机制
- ✅ 文件上传安全校验
- ✅ SQL 注入防护
- ✅ XSS 防护
- ✅ CSRF 防护

## 🔧 配置说明

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Jwt": {
    "SecretKey": "YourSecretKeyHere_MustBeAtLeast32Characters",
    "Issuer": "MinecraftLauncher",
    "Audience": "MinecraftLauncher",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Upload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".zip", ".jar", ".png"],
    "UploadDirectory": "uploads"
  }
}
```

## 📊 数据库表结构

系统包含以下核心表:

- **Users**: 用户表
- **UserProfiles**: 用户资料表
- **Resources**: 资源表
- **ResourceCompatibilities**: 资源兼容性表
- **Comments**: 评论表
- **ReviewRecords**: 审核记录表
- **Reports**: 举报表
- **Violations**: 违规记录表
- **AuditLogs**: 审计日志表
- **Notifications**: 通知表

## 🧪 测试

```bash
# 运行单元测试
dotnet test

# 运行集成测试
dotnet test --filter Category=Integration
```

## 📝 开发指南

### 添加新服务

1. 在 `Core/Domain/Interfaces/` 创建服务接口
2. 在 `Infrastructure/Services/` 实现服务
3. 在 `ServiceCollectionExtensions.cs` 注册服务
4. 在 `Controllers/` 创建 API 控制器

### 数据库迁移

```bash
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

## 🤝 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 🙏 致谢

- [KMCCC](https://github.com/DeathlyBark08/KMCCC) - Minecraft 启动核心库
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/) - Web 框架
- [Entity Framework Core](https://docs.microsoft.com/ef/) - ORM 框架

## 📬 联系方式

- 项目主页: https://github.com/yourusername/MinecraftLauncher
- 问题反馈: https://github.com/yourusername/MinecraftLauncher/issues

---

**版本**: 1.0.0  
**最后更新**: 2026-05-24
