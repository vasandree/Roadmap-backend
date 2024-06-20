using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IRoadmapRepository : IGenericRepository<Domain.Entities.Roadmap>
{
    Task<List<Domain.Entities.Roadmap>> GetPublishedRoadmaps(string? name);
    void RemovePrivateAccess(IEnumerable<PrivateAccess?> privateAccesses);
    Task<List<Domain.Entities.Roadmap>> GetUsersRoadmaps(Guid userId);
}