using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Entities
{
    public class ResourceCompatibility
    {
        public Guid Id { get; set; }
        public Guid ResourceId { get; set; }
        public string GameVersion { get; set; } = string.Empty;
        public LoaderType? LoaderType { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Resource? Resource { get; set; }
    }
}
