using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.DTOs.Resource;

public class UpdateResourceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string? Screenshots { get; set; }
    public string? Copyright { get; set; }
    public List<CompatibilityInfo>? Compatibilities { get; set; }
}
