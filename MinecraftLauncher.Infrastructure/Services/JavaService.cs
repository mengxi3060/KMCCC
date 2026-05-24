using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Launch;

namespace MinecraftLauncher.Infrastructure.Services;

public class JavaService : IJavaService
{
    public Task<IEnumerable<JavaInfo>> GetInstalledJava()
    {
        return Task.FromResult<IEnumerable<JavaInfo>>(new List<JavaInfo>());
    }

    public Task<JavaInfo> GetDefaultJava()
    {
        return Task.FromResult(new JavaInfo
        {
            Path = string.Empty,
            Version = "Unknown",
            Is64Bit = true
        });
    }

    public Task<bool> ValidateJavaPath(string javaPath)
    {
        return Task.FromResult(false);
    }
}
