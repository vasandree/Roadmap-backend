namespace Roadmap.Application.Interfaces.Repositories;

public interface IRoadmapRepository : IGenericRepository<Domain.Entities.Roadmap>
{
    Task<List<Domain.Entities.Roadmap>> GetPublishedRoadmaps(string name);
}