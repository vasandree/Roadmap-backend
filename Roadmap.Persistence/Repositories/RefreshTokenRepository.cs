using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class RefreshTokenRepository: GenericRepository<RefreshToken>, IRefreshToken
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }
}