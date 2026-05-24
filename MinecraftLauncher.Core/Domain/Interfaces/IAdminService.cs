using MinecraftLauncher.Core.DTOs.Admin;
using MinecraftLauncher.Core.DTOs.Auth;
using MinecraftLauncher.Core.DTOs.Resource;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IAdminService
{
    Task<UserListResult> GetUsers(UserQuery query);
    Task<bool> BanUser(Guid userId, BanRequest request, Guid adminId);
    Task<bool> UnbanUser(Guid userId, Guid adminId);
    Task<bool> UpdateUserRole(Guid userId, string role, Guid adminId);
    Task<ResourceListResult> GetAllResources(ResourceManagementQuery query);
    Task<bool> SetResourceTopped(Guid resourceId, bool isTopped, Guid adminId);
    Task<bool> SetResourceRecommended(Guid resourceId, bool isRecommended, Guid adminId);
    Task<bool> UnlistResource(Guid resourceId, string reason, Guid adminId);
    Task<AdminDashboardStats> GetDashboardStats();
}
