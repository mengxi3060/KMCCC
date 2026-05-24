using MinecraftLauncher.Core.DTOs.Review;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IReviewService
{
    Task<ReviewQueueResult> GetReviewQueue(ReviewQuery query);
    Task<ReviewDetail> GetReviewDetail(Guid resourceId);
    Task<ReviewActionResult> ApproveResource(Guid resourceId, ReviewComment comment, Guid reviewerId);
    Task<ReviewActionResult> RejectResource(Guid resourceId, RejectReason reason, Guid reviewerId);
    Task<ReviewActionResult> FreezeResource(Guid resourceId, FreezeReason reason, Guid reviewerId);
    Task<IEnumerable<ReviewHistoryItem>> GetReviewHistory(Guid resourceId);
}
