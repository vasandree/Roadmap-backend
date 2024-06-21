using Roadmap.Application.Dtos.Responses;

namespace Roadmap.Application.Interfaces.Services;

public interface IRoadmapAccessService
{
    Task PublishRoadmap(Guid userId, Guid roadmapId);
    Task AddPrivateAccess(Guid userId, Guid[] userIds, Guid roadmapId);
    Task RemovePrivateAccess(Guid userId, Guid[] userIds, Guid roadmapId);
    Task<List<UserDto>> GetPrivateUsers(Guid userId, Guid roadmapId, string? name);
}