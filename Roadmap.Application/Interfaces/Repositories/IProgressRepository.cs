using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IProgressRepository : IGenericRepository<Progress>
{
    Task<bool> CheckIfExists(Guid userId, Guid roadmapId);
    Task<Progress> GetByUserAndRoadmap(Guid userId, Guid roadmapId);
}