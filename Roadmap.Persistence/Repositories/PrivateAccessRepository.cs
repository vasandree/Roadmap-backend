using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class PrivateAccessRepository : GenericRepository<PrivateAccess>, IPrivateAccessRepository
{
    private readonly ApplicationDbContext _context;

    public PrivateAccessRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> CheckIfUserHasAccess(Guid roadmapId, Guid userId)
    {
        return await _context.PrivateAccesses.AnyAsync(x => x.UserId == userId && x.RoadmapId == roadmapId);
    }

    public async Task<PrivateAccess> GetByUserAndRoadmap(Guid userId, Guid roadmapId)
    {
        return (await _context.PrivateAccesses.FirstOrDefaultAsync(x => x.RoadmapId == roadmapId && x.UserId == userId))
            !;
    }

    public async Task<List<Domain.Entities.Roadmap>> GetPrivateRoadmaps(Guid userId)
    {
        return await _context.PrivateAccesses
            .Include(x => x.Roadmap)
            .ThenInclude(x=>x.User)
            .Include(x => x.Roadmap)
            .Where(x => x.UserId == userId)
            .Select(x=>x.Roadmap)
            .ToListAsync();
        
    }

    public async Task<IReadOnlyList<User>> GetUsers(Guid roadmapId, string? name)
    {
        var users = await _context.PrivateAccesses
            .Include(x => x.User)
            .Where(x => x.RoadmapId == roadmapId)
            .Select(x => x.User)
            .ToListAsync();

        if (!string.IsNullOrEmpty(name))
        {
            users = users.Where(x => x.Username.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return users.AsReadOnly();
    }
}