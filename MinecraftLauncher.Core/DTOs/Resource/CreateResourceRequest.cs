using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class CreateResourceRequest
{
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Screenshots { get; set; } = new();
    public string Copyright { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; }
    public List<CompatibilityInfo> Compatibilities { get; set; } = new();
}
