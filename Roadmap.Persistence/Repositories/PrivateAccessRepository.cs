using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class PrivateAccessRepository : GenericRepository<PrivateAccess>, IPrivateAccessRepository
{
    private readonly ApplicationDbContext _context;
    
    public PrivateAccessRepository( ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> CheckIfUserHasAccess(Guid roadmapId, Guid userId)
    {
        return await _context.PrivateAccesses.AnyAsync(x => x.UserId == userId && x.RoadmapId == roadmapId);
    }

    public async Task<PrivateAccess> GetByUserAndRoadmap(Guid userId, Guid roadmapId)
    {
        return (await _context.PrivateAccesses.FirstOrDefaultAsync(x => x.RoadmapId == roadmapId && x.UserId == userId))!;
    }
}