using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IPrivateAccessRepository : IGenericRepository<PrivateAccess>
{
    Task<bool> CheckIfUserHasAccess(Guid roadmapId, Guid userId);

    Task<PrivateAccess> GetByUserAndRoadmap(Guid userId, Guid roadmapId);

    Task<List<Domain.Entities.Roadmap>> GetPrivateRoadmaps(Guid userId);
}
