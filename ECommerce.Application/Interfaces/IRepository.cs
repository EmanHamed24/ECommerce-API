using System.Linq.Expressions;

namespace ECommerce.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate);

    Task<IEnumerable<T>> GetAllAsync(
    Expression<Func<T, bool>> predicate,
    params Expression<Func<T, object>>[] includes);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);

    Task SaveChangesAsync();
}