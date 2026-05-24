using MinecraftLauncher.Core.DTOs.Comment;
using MinecraftLauncher.Core.Domain.Entities;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetComments(Guid resourceId, int page, int pageSize);
    Task<Comment> AddComment(Guid resourceId, Guid userId, AddCommentRequest request);
    Task<bool> DeleteComment(Guid commentId, Guid userId, bool isAdmin = false);
    Task<bool> LikeComment(Guid commentId, Guid userId);
    Task<bool> ReportComment(Guid commentId, Guid reporterId, ReportReason reason);
}
