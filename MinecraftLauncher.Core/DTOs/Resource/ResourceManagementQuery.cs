using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class ResourceManagementQuery
{
    public ResourceStatus? Status { get; set; }
    public Guid? AuthorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
