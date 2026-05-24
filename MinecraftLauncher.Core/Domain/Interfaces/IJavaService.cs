using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IJavaService
{
    Task<IEnumerable<JavaInfo>> GetInstalledJava();
    Task<JavaInfo> GetDefaultJava();
    Task<bool> ValidateJavaPath(string javaPath);
}
