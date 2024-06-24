using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class ProgressRepository : GenericRepository<Progress>, IProgressRepository
{
    private readonly ApplicationDbContext _context;

    public ProgressRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> CheckIfExists(Guid userId, Guid roadmapId)
    {
        return await _context.Progresses.AnyAsync(x => x.UserId == userId && x.RoadmapId == roadmapId);
    }

    public async Task<Progress> GetByUserAndRoadmap(Guid userId, Guid roadmapId)
    {
        return await _context.Progresses.FirstOrDefaultAsync(x => x.UserId == userId && x.RoadmapId == roadmapId)!;
    }
}