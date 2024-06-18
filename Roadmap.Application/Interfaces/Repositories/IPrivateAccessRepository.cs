using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IPrivateAccessRepository : IGenericRepository<PrivateAccess>
{
    Task<bool> CheckIfUserHasAccess(Guid roadmapId, Guid userId);
}
