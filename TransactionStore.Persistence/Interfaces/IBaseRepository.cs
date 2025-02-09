using Dapper;
using System.Linq.Expressions;

namespace TransactionStore.Persistence.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task TransactionalAddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateAsync(TEntity entity);
    Task RemoveAsync(TEntity entity);
    Task<T> ExecuteStoredProcedureWithOutputAsync<T>(string storedProcedure, DynamicParameters parameters);
}
