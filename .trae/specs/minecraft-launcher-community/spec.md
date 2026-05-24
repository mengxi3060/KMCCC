# Minecraft 第三方启动器社区版规格文档

## 1. 项目概述

### 1.1 项目背景与目标

本项目旨在开发一款功能完整的 Minecraft 第三方启动器,在现有 KMCCC 启动核心基础上,新增资源分享社区模块,搭建内置资源生态平台。系统支持玩家上传、分享游戏资源(模组、整合包、光影包、材质包),并提供完善的审核机制和管理后台,打造安全、可信的资源分享环境。

### 1.2 技术栈

- **后端框架**: ASP.NET Core 8.0
- **启动核心**: KMCCC (KMCCC.Shared/KMCCC.Pro)
- **数据库**: SQLite (开发环境) / PostgreSQL (生产环境)
- **ORM**: Entity Framework Core 8.0
- **前端**: React 18 + TypeScript
- **文件存储**: 本地文件系统 + 对象存储(可选)
- **身份认证**: JWT Token + Cookie
- **实时通信**: SignalR (WebSocket)

## 2. 核心功能模块

### 2.1 启动器核心模块

#### 2.1.1 游戏版本管理

**功能描述**: 管理本地已安装的游戏版本,支持版本扫描、版本信息展示。

**接口规范**:

```csharp
public interface IVersionService
{
    Task<IEnumerable<GameVersion>> GetInstalledVersions();
    Task<GameVersion> GetVersionById(string versionId);
    Task<bool> ScanVersions(string gameRootPath);
}
```

**数据模型**:

```csharp
public class GameVersion
{
    public string Id { get; set; }           // 版本ID (如 "1.20.1")
    public string Name { get; set; }          // 显示名称
    public string GameRootPath { get; set; }  // 游戏根目录
    public DateTime InstallDate { get; set; } // 安装日期
    public long Size { get; set; }            // 版本大小(字节)
    public bool IsValid { get; set; }         // 是否有效
}
```

#### 2.1.2 游戏启动管理

**功能描述**: 封装 KMCCC 的启动功能,支持离线登录和正版登录。

**接口规范**:

```csharp
public interface ILaunchService
{
    Task<LaunchResult> LaunchGame(LaunchOptions options);
    Task<LaunchResult> LaunchWithOfflineAuth(string versionId, string playerName);
    Task<LaunchResult> LaunchWithYggdrasilAuth(string versionId, string email, string password);
}
```

**启动选项模型**:

```csharp
public class LaunchOptions
{
    public string VersionId { get; set; }           // 游戏版本ID
    public IAuthenticator Authenticator { get; set; } // 认证器
    public int MaxMemory { get; set; }              // 最大内存(MB)
    public int MinMemory { get; set; }              // 最小内存(MB)
    public ServerInfo Server { get; set; }          // 服务器信息(可选)
    public WindowSize Size { get; set; }           // 窗口大小(可选)
}
```

#### 2.1.3 Java 环境管理

**功能描述**: 自动检测和管理系统 Java 环境。

**接口规范**:

```csharp
public interface IJavaService
{
    Task<IEnumerable<JavaInfo>> GetInstalledJava();
    Task<JavaInfo> GetDefaultJava();
    Task<bool> ValidateJavaPath(string javaPath);
}
```

### 2.2 用户认证与权限模块

#### 2.2.1 用户注册与登录

**功能描述**: 支持用户注册、登录、登出功能,区分游客和注册用户。

**接口规范**:

```csharp
public interface IAuthService
{
    Task<AuthResult> Register(RegisterRequest request);
    Task<AuthResult> Login(LoginRequest request);
    Task Logout();
    Task<UserInfo> GetCurrentUser();
    Task<bool> ValidateToken(string token);
}
```

**数据模型**:

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }        // User, Moderator, Admin
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public int ViolationCount { get; set; } // 违规次数
    public bool IsBanned { get; set; }
}

public class UserProfile
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; }  // 游戏内显示名
    public string Avatar { get; set; }        // 头像URL
    public string Bio { get; set; }          // 个人简介
    public int UploadCount { get; set; }     // 上传资源数
    public int DownloadCount { get; set; }  // 下载资源数
}
```

#### 2.2.2 权限控制

**功能描述**: 基于角色的权限控制,区分普通用户、版主、管理员。

**权限矩阵**:

| 功能 | 游客 | 普通用户 | 版主 | 管理员 |
|------|------|---------|------|--------|
| 浏览资源 | ✓ | ✓ | ✓ | ✓ |
| 下载资源 | ✓ | ✓ | ✓ | ✓ |
| 上传资源 | ✗ | ✓ | ✓ | ✓ |
| 发表评论 | ✗ | ✓ | ✓ | ✓ |
| 举报资源 | ✗ | ✓ | ✓ | ✓ |
| 审核资源 | ✗ | ✗ | ✓ | ✓ |
| 管理资源 | ✗ | ✗ | ✓ | ✓ |
| 用户管理 | ✗ | ✗ | ✗ | ✓ |

### 2.3 资源上传与管理模块

#### 2.3.1 资源类型定义

**功能描述**: 支持四类游戏资源的统一管理。

**资源类型枚举**:

```csharp
public enum ResourceType
{
    Mod,           // 模组
    Modpack,      // 游戏整合包
    Shader,        // 光影包
    TexturePack    // 材质资源包
}
```

**支持的加载器类型**:

```csharp
public enum LoaderType
{
    None,           // 无(原版)
    Forge,          // Forge
    Fabric,         // Fabric
    Quilt,          // Quilt
    OptiFine,       // OptiFine
    LiteLoader      // LiteLoader
}
```

#### 2.3.2 资源上传流程

**功能描述**: 用户上传资源,系统自动校验文件格式和完整性。

**接口规范**:

```csharp
public interface IResourceUploadService
{
    Task<UploadInitResult> InitializeUpload(UploadRequest request);
    Task<UploadProgress> GetUploadProgress(string uploadId);
    Task<UploadCompleteResult> CompleteUpload(string uploadId);
    Task<bool> CancelUpload(string uploadId);
}

public class UploadRequest
{
    public string Name { get; set; }              // 资源名称
    public ResourceType Type { get; set; }       // 资源类型
    public List<string> CompatibleVersions { get; set; } // 适配版本列表
    public List<LoaderType> CompatibleLoaders { get; set; } // 适配加载器
    public string Description { get; set; }     // 简介描述
    public List<string> Tags { get; set; }       // 标签列表
    public List<string> Screenshots { get; set; } // 预览截图URL
    public string Copyright { get; set; }        // 版权信息
}
```

**资源模型**:

```csharp
public class Resource
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public Guid AuthorId { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Screenshots { get; set; }
    public List<Compatibility> Compatibilities { get; set; }
    public string Copyright { get; set; }
    public string FilePath { get; set; }        // 文件存储路径
    public long FileSize { get; set; }           // 文件大小
    public string FileHash { get; set; }         // 文件校验码
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }
    public ResourceStatus Status { get; set; }   // Pending, Approved, Rejected, Frozen
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class Compatibility
{
    public string GameVersion { get; set; }      // 游戏版本
    public LoaderType LoaderType { get; set; }  // 加载器类型
    public bool IsVerified { get; set; }         // 是否官方验证
}

public enum ResourceStatus
{
    Pending,    // 待审核
    Approved,   // 已通过
    Rejected,   // 已驳回
    Frozen,     // 已冻结
    Removed     // 已删除
}
```

#### 2.3.3 文件校验规则

**功能描述**: 系统自动校验上传文件,拦截异常文件。

**校验规则**:

| 资源类型 | 允许格式 | 最大大小 | 校验项 |
|---------|---------|---------|--------|
| 模组 | .jar, .zip | 100MB | 文件完整性、JAR签名 |
| 整合包 | .zip | 10GB | 文件结构完整性 |
| 光影包 | .zip, .jar | 50MB | 文件完整性 |
| 材质包 | .zip, .png | 200MB | 文件完整性 |

**校验流程**:

1. **文件格式检查**: 验证文件扩展名和 MIME 类型
2. **文件大小检查**: 验证文件大小是否超过限制
3. **完整性校验**: 计算文件哈希值,验证文件是否损坏
4. **内容扫描**: 检查文件名和内容是否包含恶意代码特征
5. **结构验证**: 对压缩包进行结构完整性检查

### 2.4 管理员审核模块

#### 2.4.1 审核工作台

**功能描述**: 管理员后台主页,展示待审核资源列表。

**接口规范**:

```csharp
public interface IReviewService
{
    Task<ReviewQueueResult> GetReviewQueue(ReviewQuery query);
    Task<ReviewDetail> GetReviewDetail(Guid resourceId);
    Task<ReviewActionResult> ApproveResource(Guid resourceId, ReviewComment comment);
    Task<ReviewActionResult> RejectResource(Guid resourceId, RejectReason reason);
    Task<ReviewActionResult> FreezeResource(Guid resourceId, FreezeReason reason);
    Task<IEnumerable<ReviewLog>> GetReviewHistory(Guid resourceId);
}
```

**审核查询模型**:

```csharp
public class ReviewQuery
{
    public ReviewStatus? Status { get; set; }      // 审核状态筛选
    public ResourceType? Type { get; set; }       // 资源类型筛选
    public DateTime? DateFrom { get; set; }       // 日期范围
    public DateTime? DateTo { get; set; }
    public string Keyword { get; set; }           // 关键词搜索
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public enum ReviewStatus
{
    Pending,      // 待审核
    InReview,     // 审核中
    Completed     // 已完成
}
```

#### 2.4.2 多维度审核

**审核维度**:

1. **合规性审核**
   - 检查资源名称、描述是否包含违规内容
   - 检查版权声明是否完整准确
   - 检查是否存在违法侵权内容

2. **版本兼容性审核**
   - 验证适配版本是否真实存在
   - 检查版本号格式是否正确
   - 确认加载器类型是否匹配

3. **文件安全性审核**
   - 病毒扫描和恶意代码检测
   - 文件完整性校验
   - 可疑内容分析

4. **内容完整性审核**
   - 检查资源描述是否详尽
   - 验证截图是否清晰可辨
   - 确认标签是否准确

**审核结果模型**:

```csharp
public class ReviewResult
{
    public Guid ResourceId { get; set; }
    public ReviewAction Action { get; set; }    // Approve, Reject, Freeze
    public ReviewComment Comment { get; set; }
    public Guid ReviewerId { get; set; }
    public DateTime ReviewedAt { get; set; }
    public Dictionary<string, bool> CheckResults { get; set; } // 各维度审核结果
}

public class ReviewComment
{
    public string Message { get; set; }          // 审核意见
    public List<string> ViolationTypes { get; set; } // 违规类型列表
}

public class RejectReason
{
    public string Reason { get; set; }           // 驳回原因
    public List<string> Details { get; set; }   // 详细说明
}

public class FreezeReason
{
    public string Reason { get; set; }           // 冻结原因
    public bool IsPermanent { get; set; }        // 是否永久冻结
    public DateTime? ExpiresAt { get; set; }     // 解冻时间(可选)
}
```

#### 2.4.3 资源管理操作

**功能描述**: 对已上架资源进行管理操作。

**管理操作**:

```csharp
public interface IResourceManagementService
{
    Task<bool> SetTopped(Guid resourceId, bool isTopped);
    Task<bool> SetRecommended(Guid resourceId, bool isRecommended);
    Task<bool> UnlistResource(Guid resourceId);  // 下架资源
    Task<bool> DeleteResource(Guid resourceId);  // 删除资源
    Task<bool> RestoreResource(Guid resourceId); // 恢复资源
}
```

### 2.5 资源展示与下载模块

#### 2.5.1 资源浏览

**功能描述**: 资源市场主页,支持分类筛选和搜索。

**接口规范**:

```csharp
public interface IResourceBrowseService
{
    Task<ResourceListResult> GetResources(ResourceBrowseQuery query);
    Task<ResourceDetail> GetResourceDetail(Guid resourceId);
    Task<IEnumerable<Resource>> GetRecommendedResources(Guid? excludeId);
    Task<IEnumerable<Resource>> GetTopResources(ResourceType? type, int count);
}
```

**浏览查询模型**:

```csharp
public class ResourceBrowseQuery
{
    public ResourceType? Type { get; set; }           // 资源类型
    public string GameVersion { get; set; }           // 游戏版本
    public LoaderType? Loader { get; set; }          // 加载器类型
    public string Keyword { get; set; }               // 搜索关键词
    public SortBy SortBy { get; set; }                // 排序方式
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public enum SortBy
{
    Newest,        // 最新
    Popular,       // 最热
    Downloads,     // 下载量
    Rating         // 评分
}
```

#### 2.5.2 资源详情页

**功能描述**: 展示资源的详细信息、作者信息、统计数据。

**详情页模型**:

```csharp
public class ResourceDetail : Resource
{
    public User Author { get; set; }               // 作者信息
    public List<Comment> Comments { get; set; }    // 评论列表
    public List<Review> Reviews { get; set; }       // 评价列表
    public Statistics Statistics { get; set; }      // 统计数据
    public List<Resource> SimilarResources { get; set; } // 相关资源
}
```

#### 2.5.3 资源下载

**功能描述**: 一键安装功能,自动归类文件至对应目录。

**接口规范**:

```csharp
public interface IDownloadService
{
    Task<DownloadInfo> GetDownloadInfo(Guid resourceId);
    Task<DownloadResult> DownloadResource(Guid resourceId, string targetPath);
    Task<InstallResult> InstallResource(Guid resourceId, string gameRootPath);
    Task<DownloadProgress> GetDownloadProgress(string downloadId);
}
```

**安装结果模型**:

```csharp
public class InstallResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<InstalledFile> InstalledFiles { get; set; }
}

public class InstalledFile
{
    public string SourcePath { get; set; }   // 源文件路径
    public string TargetPath { get; set; }  // 目标路径
    public InstallAction Action { get; set; } // Copy, Merge, Replace
}
```

### 2.6 评论与互动模块

#### 2.6.1 评论系统

**功能描述**: 用户可对资源发表评论和回复。

**接口规范**:

```csharp
public interface ICommentService
{
    Task<IEnumerable<Comment>> GetComments(Guid resourceId, int page, int pageSize);
    Task<Comment> AddComment(AddCommentRequest request);
    Task<bool> DeleteComment(Guid commentId);
    Task<bool> ReportComment(Guid commentId, ReportReason reason);
}
```

**评论模型**:

```csharp
public class Comment
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid? ParentId { get; set; }      // 回复目标(顶级评论为null)
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsEdited { get; set; }
}
```

### 2.7 违规管控模块

#### 2.7.1 用户违规管理

**功能描述**: 记录和管理用户违规行为,限制违规用户权限。

**违规模型**:

```csharp
public class Violation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ViolationType Type { get; set; }  // 违规类型
    public Guid? ResourceId { get; set; }     // 关联资源
    public string Description { get; set; }   // 违规描述
    public Guid HandledBy { get; set; }       // 处理人
    public DateTime CreatedAt { get; set; }
    public ViolationSeverity Severity { get; set; } // 严重程度
}

public enum ViolationType
{
    CopyrightInfringement,   // 版权侵权
    MaliciousCode,           // 恶意代码
    InappropriateContent,    // 不当内容
    Spam,                    // 垃圾信息
    FakeResource             // 虚假资源
}

public enum ViolationSeverity
{
    Warning,     // 警告
    Minor,       // 轻微
    Moderate,    // 中等
    Severe,      // 严重
    Critical     // 严重违规
}
```

**违规处罚规则**:

| 违规次数 | 处罚措施 | 权限限制 |
|---------|---------|---------|
| 1次 | 警告 + 资源下架 | 无 |
| 2次 | 限制上传7天 | 禁止上传 |
| 3次 | 限制上传30天 | 禁止上传 |
| 4次+ | 永久封禁 | 全部限制 |

#### 2.7.2 举报系统

**功能描述**: 用户可举报违规资源,管理员快速处置。

**接口规范**:

```csharp
public interface IReportService
{
    Task<Guid> SubmitReport(ReportRequest request);
    Task<IEnumerable<Report>> GetPendingReports();
    Task<bool> ResolveReport(Guid reportId, ReportResolution resolution);
}

public class ReportRequest
{
    public Guid ResourceId { get; set; }
    public ReportType Type { get; set; }
    public string Description { get; set; }
    public List<string> EvidenceUrls { get; set; }
}

public enum ReportType
{
    CopyrightInfringement,
    MaliciousContent,
    InappropriateContent,
    OutdatedResource,
    FakeResource,
    Other
}
```

### 2.8 日志与审计模块

#### 2.8.1 操作日志

**功能描述**: 记录所有关键操作,支持溯源查询。

**日志模型**:

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; }          // 操作类型
    public string TargetType { get; set; }       // 目标类型
    public Guid? TargetId { get; set; }          // 目标ID
    public string Details { get; set; }          // 详细信息(JSON)
    public string IpAddress { get; set; }         // IP地址
    public string UserAgent { get; set; }         // 用户代理
    public DateTime CreatedAt { get; set; }
}

public class ReviewAuditLog : AuditLog
{
    public Guid ResourceId { get; set; }
    public ReviewAction Action { get; set; }
    public string Reason { get; set; }
    public List<string> CheckResults { get; set; }
}
```

## 3. 数据库设计

### 3.1 核心表结构

```sql
-- 用户表
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'User',
    ViolationCount INT DEFAULT 0,
    IsBanned BOOLEAN DEFAULT FALSE,
    BanReason TEXT,
    BanExpiresAt TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt TIMESTAMP
);

-- 用户资料表
CREATE TABLE UserProfiles (
    UserId UUID PRIMARY KEY REFERENCES Users(Id),
    DisplayName VARCHAR(50),
    Avatar VARCHAR(500),
    Bio TEXT,
    UploadCount INT DEFAULT 0,
    DownloadCount INT DEFAULT 0
);

-- 资源表
CREATE TABLE Resources (
    Id UUID PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Type INT NOT NULL,  -- 0: Mod, 1: Modpack, 2: Shader, 3: TexturePack
    AuthorId UUID REFERENCES Users(Id),
    Description TEXT,
    Tags JSON,
    Screenshots JSON,
    Copyright TEXT,
    FilePath VARCHAR(500),
    FileSize BIGINT,
    FileHash VARCHAR(64),
    DownloadCount INT DEFAULT 0,
    LikeCount INT DEFAULT 0,
    Status INT DEFAULT 0,  -- 0: Pending, 1: Approved, 2: Rejected, 3: Frozen
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP,
    ApprovedAt TIMESTAMP,
    IsTopped BOOLEAN DEFAULT FALSE,
    IsRecommended BOOLEAN DEFAULT FALSE
);

-- 资源兼容性表
CREATE TABLE ResourceCompatibilities (
    Id UUID PRIMARY KEY,
    ResourceId UUID REFERENCES Resources(Id),
    GameVersion VARCHAR(20),
    LoaderType INT,
    IsVerified BOOLEAN DEFAULT FALSE
);

-- 审核记录表
CREATE TABLE ReviewRecords (
    Id UUID PRIMARY KEY,
    ResourceId UUID REFERENCES Resources(Id),
    ReviewerId UUID REFERENCES Users(Id),
    Action INT NOT NULL,  -- 0: Approve, 1: Reject, 2: Freeze
    Comment TEXT,
    CheckResults JSON,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 举报表
CREATE TABLE Reports (
    Id UUID PRIMARY KEY,
    ResourceId UUID REFERENCES Resources(Id),
    ReporterId UUID REFERENCES Users(Id),
    Type INT NOT NULL,
    Description TEXT,
    EvidenceUrls JSON,
    Status INT DEFAULT 0,  -- 0: Pending, 1: Resolved, 2: Dismissed
    ResolvedBy UUID REFERENCES Users(Id),
    Resolution TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ResolvedAt TIMESTAMP
);

-- 违规记录表
CREATE TABLE Violations (
    Id UUID PRIMARY KEY,
    UserId UUID REFERENCES Users(Id),
    Type INT NOT NULL,
    ResourceId UUID REFERENCES Resources(Id),
    Description TEXT,
    Severity INT NOT NULL,
    HandledBy UUID REFERENCES Users(Id),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 评论表
CREATE TABLE Comments (
    Id UUID PRIMARY KEY,
    ResourceId UUID REFERENCES Resources(Id),
    ParentId UUID REFERENCES Comments(Id),
    UserId UUID REFERENCES Users(Id),
    Content TEXT NOT NULL,
    LikeCount INT DEFAULT 0,
    IsEdited BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 审计日志表
CREATE TABLE AuditLogs (
    Id UUID PRIMARY KEY,
    UserId UUID REFERENCES Users(Id),
    Action VARCHAR(50) NOT NULL,
    TargetType VARCHAR(50),
    TargetId UUID,
    Details JSON,
    IpAddress VARCHAR(45),
    UserAgent VARCHAR(500),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## 4. API 接口规范

### 4.1 认证相关接口

```
POST   /api/auth/register        - 用户注册
POST   /api/auth/login           - 用户登录
POST   /api/auth/logout          - 用户登出
GET    /api/auth/me              - 获取当前用户信息
POST   /api/auth/refresh         - 刷新Token
```

### 4.2 启动器核心接口

```
GET    /api/launcher/versions         - 获取已安装版本列表
GET    /api/launcher/versions/{id}   - 获取指定版本信息
POST   /api/launcher/launch           - 启动游戏
GET    /api/launcher/java             - 获取Java环境列表
POST   /api/launcher/scan             - 扫描游戏目录
```

### 4.3 资源管理接口

```
GET    /api/resources                  - 浏览资源列表
GET    /api/resources/{id}             - 获取资源详情
POST   /api/resources                 - 上传资源(需登录)
PUT    /api/resources/{id}            - 更新资源(需登录,仅作者)
DELETE /api/resources/{id}             - 删除资源(需登录,仅作者)
POST   /api/resources/{id}/download     - 下载资源
POST   /api/resources/{id}/install    - 一键安装
GET    /api/resources/my              - 获取我的上传列表(需登录)
```

### 4.4 审核管理接口(需管理员权限)

```
GET    /api/admin/review/queue         - 获取审核队列
GET    /api/admin/review/{resourceId} - 获取审核详情
POST   /api/admin/review/{resourceId}/approve   - 通过审核
POST   /api/admin/review/{resourceId}/reject    - 驳回审核
POST   /api/admin/review/{resourceId}/freeze    - 冻结资源
GET    /api/admin/review/history       - 审核历史记录
```

### 4.5 管理后台接口(需管理员权限)

```
GET    /api/admin/resources            - 资源管理列表
PUT    /api/admin/resources/{id}/top   - 设置置顶
PUT    /api/admin/resources/{id}/recommend - 设置推荐
DELETE /api/admin/resources/{id}       - 下架资源
GET    /api/admin/users                - 用户管理列表
PUT    /api/admin/users/{id}/ban       - 封禁用户
PUT    /api/admin/users/{id}/unban     - 解封用户
```

### 4.6 举报与违规接口

```
POST   /api/reports                    - 提交举报(需登录)
GET    /api/admin/reports              - 举报列表(需管理员)
PUT    /api/admin/reports/{id}/resolve - 处理举报
GET    /api/admin/violations           - 违规记录列表
POST   /api/admin/violations           - 记录违规
```

### 4.7 评论接口

```
GET    /api/resources/{id}/comments   - 获取评论列表
POST   /api/resources/{id}/comments    - 添加评论(需登录)
DELETE /api/comments/{id}              - 删除评论(需登录,仅作者)
POST   /api/comments/{id}/report       - 举报评论(需登录)
```

## 5. 安全要求

### 5.1 认证安全

- 密码使用 BCrypt 加密,盐值长度≥16
- JWT Token 有效期: Access Token 1小时, Refresh Token 7天
- 敏感操作需二次验证
- 登录失败锁定: 5次失败后锁定15分钟

### 5.2 文件安全

- 文件上传大小限制: 单文件100MB,总上传量10GB
- 文件类型白名单校验
- 文件名 sanitize 处理,防止路径遍历
- 文件存储隔离,禁止执行权限
- 定期病毒扫描

### 5.3 权限控制

- 基于角色和资源的所有权双重验证
- 管理员操作需记录审计日志
- 敏感接口限流: 100次/分钟

### 5.4 数据安全

- 敏感数据传输使用 HTTPS
- 数据库连接加密
- 定期备份机制
- GDPR 合规: 用户数据删除权

## 6. 性能要求

- API 响应时间: P95 < 500ms
- 资源列表查询: P95 < 1s
- 文件上传: 支持断点续传
- 并发用户: 支持1000+同时在线
- 数据库查询: 避免N+1问题

## 7. 部署架构

### 7.1 开发环境

- 单机部署
- SQLite 数据库
- 本地文件存储
- HTTP 开发服务器

### 7.2 生产环境(推荐)

- 负载均衡 + 多实例部署
- PostgreSQL 数据库集群
- 对象存储服务(MinIO/OSS)
- CDN 加速静态资源
- Redis 缓存层
- Docker/Kubernetes 容器化

## 8. 术语表

| 术语 | 定义 |
|-----|------|
| KMCCC | Minecraft Launcher Core for C#, 开源启动器核心库 |
| 模组(Mod) | 游戏中添加新功能的第三方扩展 |
| 整合包(Modpack) | 预装多个模组的游戏包 |
| 光影包(Shader) | 图形渲染增强包 |
| 材质包(TexturePack) | 游戏中贴图纹理替换包 |
| Forge | Minecraft 模组加载器 |
| Fabric | 轻量级模组加载器 |
| Yggdrasil | Mojang 正版验证API |

## 9. 参考文档

- [KMCCC GitHub](https://github.com/DeathlyBark08/KMCCC)
- [Minecraft Forge Documentation](https://docs.minecraftforge.net/)
- [Fabric Wiki](https://fabricmc.net/wiki/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
