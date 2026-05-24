namespace MinecraftLauncher.Core.Models
{
    public class Version
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string GameRootPath { get; set; } = string.Empty;
        public string? JarPath { get; set; }
        public string? JsonPath { get; set; }
        public int? Size { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsVirtual { get; set; } = false;
        public bool IsIntegrityCheckResult { get; set; } = false;
        public List<string>? Libraries { get; set; }
    }
}
