using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string content, NotificationType type, string relatedId = null);
    Task<IEnumerable<Notification>> GetUserNotifications(Guid userId, int pageIndex, int pageSize);
    Task<int> GetUnreadCount(Guid userId);
    Task MarkAsRead(Guid notificationId, Guid userId);
    Task MarkAllAsRead(Guid userId);
}
