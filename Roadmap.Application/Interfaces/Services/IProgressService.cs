using Roadmap.Domain.Enums;

namespace Roadmap.Application.Interfaces.Services;

public interface IProgressService
{
    Task ChangeProgress(Guid userId, Guid roadmapId, Guid topicId, ProgressStatus progressStatus);
}