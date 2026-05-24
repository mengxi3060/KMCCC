using MinecraftLauncher.Core.Domain.Entities;

namespace MinecraftLauncher.Core.DTOs.Admin;

public class UserQuery
{
    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public bool? IsBanned { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public class UserListResult
{
    public IEnumerable<User> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public class BanRequest
{
    public string Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateRoleRequest
{
    public string Role { get; set; }
}

public class AdminDashboardStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsersToday { get; set; }
    public int TotalResources { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedResources { get; set; }
    public int TotalDownloads { get; set; }
    public int PendingReports { get; set; }
    public int TotalViolations { get; set; }
    public int RecentViolations { get; set; }
}

public class ResourceManagementItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string AuthorName { get; set; }
    public Guid AuthorId { get; set; }
    public string Status { get; set; }
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }
    public bool IsTopped { get; set; }
    public bool IsRecommended { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ResourceManagementResult
{
    public IEnumerable<ResourceManagementItem> Resources { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
