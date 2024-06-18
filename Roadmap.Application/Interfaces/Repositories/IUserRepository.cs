using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User> GetByEmail(string email);
    Task<User> GetByUsername(string username);
    Task<bool> CheckIfEmailExists(string email);
    Task<bool> CheckIfUsernameExists(string username);

    Task<IQueryable<User>> GetAsQueryable();
}