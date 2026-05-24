using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.Services;
using MinecraftLauncher.Core.Services.Launch;
using MinecraftLauncher.Infrastructure.Data;
using MinecraftLauncher.Infrastructure.Services;
using MinecraftLauncher.Infrastructure.Services.Launch;

namespace MinecraftLauncher.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMinecraftLauncherServices(
            this IServiceCollection services,
            string connectionString,
            string gameRootPath,
            string javaPath,
            string uploadDirectory = "uploads")
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<ILaunchService>(provider =>
                new KMCCCLaunchService(gameRootPath, javaPath));

            services.AddScoped<IVersionService>(provider =>
                new VersionService(gameRootPath));

            services.AddSingleton<IJavaService>(provider =>
                new JavaService());

            services.AddScoped<IResourceUploadService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ResourceUploadService>>();
                return new ResourceUploadService(context, logger, uploadDirectory);
            });

            services.AddScoped<IDownloadService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<DownloadService>>();
                return new DownloadService(context, logger, gameRootPath);
            });

            services.AddScoped<IResourceService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ResourceService>>();
                var uploadService = provider.GetRequiredService<IResourceUploadService>();
                return new ResourceService(context, logger, uploadService);
            });

            services = AddReviewAndReportServices(services);

            return services;
        }

        public static IServiceCollection AddMinecraftLauncherServicesWithSqlite(
            this IServiceCollection services,
            string dbPath,
            string gameRootPath,
            string javaPath,
            string uploadDirectory = "uploads")
        {
            var connectionString = $"Data Source={dbPath}";
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddScoped<ILaunchService>(provider =>
                new KMCCCLaunchService(gameRootPath, javaPath));

            services.AddScoped<IVersionService>(provider =>
                new VersionService(gameRootPath));

            services.AddSingleton<IJavaService>(provider =>
                new JavaService());

            services.AddScoped<IResourceUploadService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ResourceUploadService>>();
                return new ResourceUploadService(context, logger, uploadDirectory);
            });

            services.AddScoped<IDownloadService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<DownloadService>>();
                return new DownloadService(context, logger, gameRootPath);
            });

            services.AddScoped<IResourceService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ResourceService>>();
                var uploadService = provider.GetRequiredService<IResourceUploadService>();
                return new ResourceService(context, logger, uploadService);
            });

            services = AddReviewAndReportServices(services);

            return services;
        }

        private static IServiceCollection AddReviewAndReportServices(IServiceCollection services)
        {
            services.AddScoped<INotificationService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<NotificationService>>();
                return new NotificationService(context, logger);
            });

            services.AddScoped<IReviewService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ReviewService>>();
                var notificationService = provider.GetRequiredService<INotificationService>();
                return new ReviewService(context, logger, notificationService);
            });

            services.AddScoped<IReportService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ReportService>>();
                var notificationService = provider.GetRequiredService<INotificationService>();
                return new ReportService(context, logger, notificationService);
            });

            services.AddScoped<IViolationService>(provider =>
            {
                var context = provider.GetRequiredService<AppDbContext>();
                var logger = provider.GetRequiredService<ILogger<ViolationService>>();
                var notificationService = provider.GetRequiredService<INotificationService>();
                return new ViolationService(context, logger, notificationService);
            });

            return services;
        }
    }
}
