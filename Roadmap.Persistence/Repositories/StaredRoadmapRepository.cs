using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class StaredRoadmapRepository : GenericRepository<StaredRoadmap>, IStaredRoadmapRepository
{
    private readonly ApplicationDbContext _context;

    public StaredRoadmapRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Domain.Entities.Roadmap>> GetStaredRoadmaps(Guid userId)
    {
        return await _context.StaredRoadmaps
            .Where(x => x.UserId == userId)
            .Include(x => x.Roadmap)
            .Select(x=>x.Roadmap)
            .ToListAsync();
    }
}