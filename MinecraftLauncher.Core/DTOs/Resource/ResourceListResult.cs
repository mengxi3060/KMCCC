namespace MinecraftLauncher.Core.DTOs.Resource;

public class ResourceListResult
{
    public List<ResourceListItem> Resources { get; set; }
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
