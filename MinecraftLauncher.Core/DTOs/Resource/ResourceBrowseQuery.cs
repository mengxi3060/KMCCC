using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class ResourceBrowseQuery
{
    public ResourceType? Type { get; set; }
    public string? GameVersion { get; set; }
    public LoaderType? Loader { get; set; }
    public string? Keyword { get; set; }
    public SortBy SortBy { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}

public enum SortBy
{
    Newest = 0,
    Popular = 1,
    Downloads = 2,
    Rating = 3
}
