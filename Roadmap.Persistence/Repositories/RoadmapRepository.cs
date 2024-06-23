using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Domain.Enums;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class RoadmapRepository : GenericRepository<Domain.Entities.Roadmap>, IRoadmapRepository
{
    private readonly ApplicationDbContext _context;

    public RoadmapRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public new async Task<Domain.Entities.Roadmap> GetById(Guid id)
    {
        return (await _context.Roadmaps
            .Include(x => x.PrivateAccesses)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id)!);
    }


    public async Task<List<Domain.Entities.Roadmap>> GetPublishedRoadmaps(string? name)
    {
        var roadmapsQuery = _context.Roadmaps
            .Include(x => x.User)
            .Where(x => x.Status == Status.Public);

        if (!string.IsNullOrEmpty(name))
        {
            roadmapsQuery = roadmapsQuery.Where(x => x.Name.Contains(name));
        }

        return await roadmapsQuery.ToListAsync();
    }
    public void RemovePrivateAccess(IEnumerable<PrivateAccess>? privateAccesses)
    {
        if (privateAccesses != null)
        {
            _context.PrivateAccesses.RemoveRange(privateAccesses);

        }
    }

    public async Task<List<Domain.Entities.Roadmap>> GetUsersRoadmaps(Guid userId)
    {
        return await _context.Roadmaps
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<List<Domain.Entities.Roadmap>> GetRoadmapsByIds(List<Guid> roadmapIds)
    {
        return await _context.Roadmaps
            .Include(x=>x.User)
            .Where(r => roadmapIds.Contains(r.Id))
            .ToListAsync();
    }
}