using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Entities;

public class ResourceCompatibility
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public string GameVersion { get; set; }
    public LoaderType LoaderType { get; set; }
    public bool IsVerified { get; set; }

    public Resource Resource { get; set; }
}
