using System.Linq.Expressions;

namespace Identity.Domain.Interfaces.IRepositories;

public interface IGenericRepository<TEntity, TId>
{
    #region Query Operations
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(TId id, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes);
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes);
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdNoTrackingAsync(TId id, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity?> GetByIdNoTrackingAsync(TId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IReadOnlyList<TResult>> ListAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector);
    Task<IReadOnlyList<TEntity>> ListAllNoTrackingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllWithPaginationAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    #endregion

    #region Command Operations
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate Operations
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    #endregion
}
