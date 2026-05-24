using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Entities;

namespace MinecraftLauncher.Infrastructure.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            
            try
            {
                logger.LogInformation("开始数据库迁移...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("数据库初始化完成");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "数据库初始化失败");
                throw;
            }
        }
        
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            
            try
            {
                if (await context.Users.AnyAsync())
                {
                    logger.LogInformation("数据库已有数据,跳过数据种子");
                    return;
                }
                
                logger.LogInformation("开始种子数据...");
                
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                var adminProfile = new UserProfile
                {
                    UserId = adminUser.Id,
                    DisplayName = "Administrator",
                    Bio = "系统管理员"
                };
                
                var modUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "moderator",
                    Email = "mod@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mod@123"),
                    Role = "Moderator",
                    CreatedAt = DateTime.UtcNow
                };
                
                var modProfile = new UserProfile
                {
                    UserId = modUser.Id,
                    DisplayName = "Moderator",
                    Bio = "社区版主"
                };
                
                var testUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "user@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };
                
                var testProfile = new UserProfile
                {
                    UserId = testUser.Id,
                    DisplayName = "Test User",
                    Bio = "测试用户"
                };
                
                await context.Users.AddRangeAsync(adminUser, modUser, testUser);
                await context.UserProfiles.AddRangeAsync(adminProfile, modProfile, testProfile);
                
                await context.SaveChangesAsync();
                
                logger.LogInformation("种子数据创建完成");
                logger.LogInformation("管理员账号: admin@example.com / Admin@123");
                logger.LogInformation("版主账号: mod@example.com / Mod@123");
                logger.LogInformation("测试账号: user@example.com / User@123");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "种子数据创建失败");
                throw;
            }
        }
    }
}
