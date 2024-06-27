using System.Linq.Expressions;
using Roadmap.Domain.Entities;

namespace Roadmap.Application.Interfaces.Repositories;

public interface IGenericRepository<T>  where T : GenericEntity
{
    Task<T> GetById(Guid id);
    Task<bool> CheckIfIdExists(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> Find(Expression<Func<T, bool>> expression);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);

    Task CreateAsync(T entity);
}
