using System.Linq.Expressions;

namespace ServerApp.Contracts.Repositories;
public interface IRepositoryBase<T, TContext>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> FindAsync(int id);
    Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? includeProperties = null);

    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);

    Task<bool> Exists(int id);
    
    Task SaveAsync();
}