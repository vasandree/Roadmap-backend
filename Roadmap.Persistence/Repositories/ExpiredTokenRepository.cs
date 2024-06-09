using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class ExpiredTokenRepository : GenericRepository<ExpiredToken>, IExpiredToken
{
    public ExpiredTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}