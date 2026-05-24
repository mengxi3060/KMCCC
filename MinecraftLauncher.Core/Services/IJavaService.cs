using MinecraftLauncher.Core.Models;

namespace MinecraftLauncher.Core.Services
{
    public interface IJavaService
    {
        Task<IEnumerable<JavaInfo>> GetInstalledJava();
        Task<JavaInfo> GetDefaultJava();
        Task<bool> ValidateJavaPath(string javaPath);
    }
}
