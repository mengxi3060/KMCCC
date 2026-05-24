using MinecraftLauncher.Core.DTOs.Resource;
using MinecraftLauncher.Core.Domain.Entities;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IResourceService
{
    Task<ResourceListResult> GetResources(ResourceBrowseQuery query);
    Task<ResourceDetail> GetResourceDetail(Guid resourceId);
    Task<Resource> CreateResource(CreateResourceRequest request, Guid authorId);
    Task<Resource> UpdateResource(Guid resourceId, UpdateResourceRequest request, Guid userId);
    Task<bool> DeleteResource(Guid resourceId, Guid userId);
    Task<IEnumerable<Resource>> GetMyResources(Guid userId);
}
