using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IStaredRoadmapRepository : IGenericRepository<StaredRoadmap>
{
    Task<List<Domain.Entities.Roadmap>> GetStaredRoadmaps(Guid userId);
}