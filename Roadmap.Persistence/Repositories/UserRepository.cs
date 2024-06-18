using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public new async Task<User> GetById(Guid id)
    {
        return (await _context.Users
            .Include(x=>x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == id)!);
    }


    public async Task<User> GetByEmail(string email)
    {
        return (await _context.Users.FirstOrDefaultAsync(x => x.Email == email)!);
    }

    public async Task<User> GetByUsername(string username)
    {
        return (await _context.Users.FirstOrDefaultAsync(x => x.Username == username)!);
    }

    public async Task<bool> CheckIfEmailExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<bool> CheckIfUsernameExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.Username == username);
    }

    public async Task<IQueryable<User>> GetAsQueryable()
    {
        return  _context.Users.AsQueryable();
    }
}