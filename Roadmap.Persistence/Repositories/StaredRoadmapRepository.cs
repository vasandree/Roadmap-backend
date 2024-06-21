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
            .Include(x => x.Roadmap)
            .ThenInclude(x=>x.User)
            .Include(x => x.Roadmap)
            .ThenInclude(x=>x.Stared)
            .Where(x => x.UserId == userId)
            .Select(x=>x.Roadmap)
            .ToListAsync();
    }

    public bool IsStared(Guid userId, Guid roadmapId)
    {
        return _context.StaredRoadmaps.Any(x => x.UserId == userId && x.RoadmapId == roadmapId);
    }

    public Task<StaredRoadmap> GetByUserAndRoadmap(Guid userId, Guid roadmapId)
    {
        return _context.StaredRoadmaps.FirstOrDefaultAsync(x => x.UserId == userId && x.RoadmapId == roadmapId)!;
    }
}