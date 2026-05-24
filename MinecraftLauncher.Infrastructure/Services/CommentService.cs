using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Comment;
using MinecraftLauncher.Infrastructure.Data;

namespace MinecraftLauncher.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CommentService> _logger;
    private readonly INotificationService _notificationService;

    public CommentService(
        AppDbContext context,
        ILogger<CommentService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Comment>> GetComments(Guid resourceId, int page, int pageSize)
    {
        return await _context.Comments
            .Include(c => c.User)
            .ThenInclude(u => u.Profile)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .ThenInclude(u => u.Profile)
            .Where(c => c.ResourceId == resourceId && c.ParentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Comment> AddComment(Guid resourceId, Guid userId, AddCommentRequest request)
    {
        var resource = await _context.Resources.FindAsync(resourceId);

        if (resource == null)
        {
            throw new InvalidOperationException("Resource not found");
        }

        if (resource.Status != ResourceStatus.Approved)
        {
            throw new InvalidOperationException("Cannot comment on non-approved resource");
        }

        if (request.ParentId.HasValue)
        {
            var parentComment = await _context.Comments.FindAsync(request.ParentId.Value);
            if (parentComment == null || parentComment.ResourceId != resourceId)
            {
                throw new InvalidOperationException("Parent comment not found");
            }
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            UserId = userId,
            ParentId = request.ParentId,
            Content = request.Content,
            LikeCount = 0,
            IsEdited = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        if (request.ParentId.HasValue)
        {
            var parentComment = await _context.Comments
                .Include(c => c.User)
                .FirstAsync(c => c.Id == request.ParentId);

            if (parentComment.UserId != userId)
            {
                await _notificationService.SendNotificationAsync(
                    parentComment.UserId,
                    "有人回复了您的评论",
                    $"用户回复了您的评论",
                    NotificationType.NewComment,
                    resourceId.ToString()
                );
            }
        }
        else if (resource.AuthorId != userId)
        {
            await _notificationService.SendNotificationAsync(
                resource.AuthorId,
                "有人评论了您的资源",
                $"用户评论了您的资源「{resource.Name}」",
                NotificationType.NewComment,
                resourceId.ToString()
            );
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Comment added: {comment.Id} by user {userId}");

        return comment;
    }

    public async Task<bool> DeleteComment(Guid commentId, Guid userId, bool isAdmin = false)
    {
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
        {
            return false;
        }

        if (!isAdmin && comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this comment");
        }

        await DeleteCommentRecursive(commentId);

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Comment deleted: {commentId} by user {userId}");

        return true;
    }

    private async Task DeleteCommentRecursive(Guid commentId)
    {
        var replies = await _context.Comments
            .Where(c => c.ParentId == commentId)
            .ToListAsync();

        foreach (var reply in replies)
        {
            await DeleteCommentRecursive(reply.Id);
        }

        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
        }
    }

    public async Task<bool> LikeComment(Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
        {
            return false;
        }

        comment.LikeCount++;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReportComment(Guid commentId, Guid reporterId, ReportReason reason)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return false;
        }

        var report = new Report
        {
            Id = Guid.NewGuid(),
            ResourceId = comment.ResourceId,
            ReporterId = reporterId,
            Type = ReportType.InappropriateContent,
            Description = $"评论举报: {reason.Reason}\n\n被举报评论:\n{comment.Content}",
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Comment reported: {commentId} by user {reporterId}");

        return true;
    }
}
