using MinecraftLauncher.Core.DTOs.Auth;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResult> Register(RegisterRequest request);
    Task<AuthResult> Login(LoginRequest request);
    Task Logout();
    Task<UserInfo> GetCurrentUser();
    Task<bool> ValidateToken(string token);
}
