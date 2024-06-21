using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class TopicRepository : GenericRepository<Topic>, ITopicRepository
{
    private readonly ApplicationDbContext _context;

    public TopicRepository( ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    
}