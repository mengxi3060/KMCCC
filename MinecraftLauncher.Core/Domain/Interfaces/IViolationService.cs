using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;

namespace MinecraftLauncher.Core.Domain.Interfaces;

public interface IViolationService
{
    Task<Violation> RecordViolation(Guid userId, ViolationType type, string description, 
        ViolationSeverity severity, Guid? resourceId, Guid handledBy);
    Task<IEnumerable<Violation>> GetUserViolations(Guid userId);
    Task<bool> LiftUploadRestriction(Guid userId);
}
