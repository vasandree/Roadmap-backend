using Microsoft.EntityFrameworkCore;
using Roadmap.Application.Interfaces.Repositories;
using Roadmap.Domain.Entities;
using Roadmap.Infrastructure;

namespace Roadmap.Persistence.Repositories;

public class UserRepository: GenericRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(DbContext context, ApplicationDbContext context1) : base(context)
    {
        _context = context1;
    }
    
    public async Task<User> GetById(Guid id)
    {
        return (await _context.Users.FirstOrDefaultAsync(x => x.UserId == id)!);
    }
    

    public async Task<User> GetByEmail(string email)
    {
        return (await _context.Users.FirstOrDefaultAsync(x => x.Email == email)!);
    }

    public async Task<User> GetByUsername(string username)
    {
        return (await _context.Users.FirstOrDefaultAsync(x => x.Username == username)!);
    }

    public async Task<bool> CheckIfIdExists(Guid id)
    {
        return await _context.Users.AnyAsync(x => x.UserId == id);
    }

    public async Task<bool> CheckIfEmailExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<bool> CheckIfUsernameExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.Username == username);
    }
}