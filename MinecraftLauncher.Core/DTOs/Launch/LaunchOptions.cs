using MinecraftLauncher.Core.DTOs.Auth;

namespace MinecraftLauncher.Core.DTOs.Launch;

public class LaunchOptions
{
    public string VersionId { get; set; }
    public IAuthenticator Authenticator { get; set; }
    public int MaxMemory { get; set; }
    public int MinMemory { get; set; }
    public ServerInfo? Server { get; set; }
    public WindowSize? Size { get; set; }
}

public interface IAuthenticator
{
    string Type { get; }
    Task<AuthResult> Authenticate();
}

public class ServerInfo
{
    public string Address { get; set; }
    public int Port { get; set; }
}

public class WindowSize
{
    public int Width { get; set; }
    public int Height { get; set; }
}
