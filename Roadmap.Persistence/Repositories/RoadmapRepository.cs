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
            .FirstOrDefaultAsync(x => x.Id == id)!);
    }


    public async Task<List<Domain.Entities.Roadmap>> GetPublishedRoadmaps(string name)
    {
        var roadmaps = _context.Roadmaps.AsQueryable();
        if (!string.IsNullOrEmpty(name))
        {
            roadmaps = roadmaps.Where(x => x.Name.Contains(name));
        }

        return await roadmaps.Where(x => x.Status == Status.Public).ToListAsync();
    }

    public void RemovePrivateAccess(IEnumerable<PrivateAccess?> privateAccesses)
    {
        _context.PrivateAccesses.RemoveRange(privateAccesses);
    }
}