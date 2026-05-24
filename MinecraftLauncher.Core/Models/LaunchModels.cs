namespace MinecraftLauncher.Core.Models
{
    public class LaunchOptions
    {
        public string VersionId { get; set; } = string.Empty;
        public object? Authenticator { get; set; }
        public int MaxMemory { get; set; } = 2048;
        public int MinMemory { get; set; } = 512;
        public ServerInfo? Server { get; set; }
        public WindowSize? Size { get; set; }
        public string? GameWindowTitle { get; set; }
        public string? JavaArguments { get; set; }
        public bool EnableIndevCoreguard { get; set; } = false;
        public bool EnableAutoExit { get; set; } = false;
    }

    public class ServerInfo
    {
        public string Address { get; set; } = string.Empty;
        public int Port { get; set; } = 25565;
    }

    public class WindowSize
    {
        public int Width { get; set; } = 854;
        public int Height { get; set; } = 480;
        public bool FullScreen { get; set; } = false;
    }

    public class LaunchResult
    {
        public bool Success { get; set; }
        public LaunchErrorType ErrorType { get; set; }
        public string? ErrorMessage { get; set; }
        public LaunchHandleInfo? Handle { get; set; }
    }

    public class LaunchHandleInfo
    {
        public int ProcessId { get; set; }
        public bool IsRunning { get; set; }
    }

    public enum LaunchErrorType
    {
        None,
        NoJavaFound,
        AuthenticationFailed,
        InvalidVersion,
        InsufficientMemory,
        GameCrashed,
        Unknown
    }

    public class GameVersion
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GameRootPath { get; set; } = string.Empty;
        public DateTime InstallDate { get; set; }
        public long Size { get; set; }
        public bool IsValid { get; set; }
        public string? Type { get; set; }
        public string? JarPath { get; set; }
        public string? JsonPath { get; set; }
    }

    public class JavaInfo
    {
        public string Path { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool Is64Bit { get; set; }
    }
}
