using System.Linq.Expressions;
using Identity.Domain.Interfaces.IRepositories;
using Identity.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastrcture.Repositories;

public class GenericRepository<TEntity, TId>(DbContext context) : IGenericRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : notnull

{
    private readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    #region Query Operations
    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id!], cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id));
    }

    #region (with AsNoTracking)
    public async Task<TEntity?> GetByIdNoTrackingAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<TEntity?> GetByIdNoTrackingAsync(TId id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet
            .AsNoTracking()
            .AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id));
    }
    #endregion

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = include(query);
        }

        return (await query.FirstOrDefaultAsync(predicate, cancellationToken))!;
    }

    public async Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate)
                           .Select(selector)
                           .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAllNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector)
    {
        return await _dbSet.Where(predicate)
                         .Select(selector)
                         .ToListAsync();
    }

    public async Task<IReadOnlyList<TEntity>> GetAllWithPaginationAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        int maxPageSize = 10;
        pageSize = pageSize > maxPageSize ? maxPageSize : pageSize;
        var skip = (pageNumber - 1) * pageSize;

        return await _dbSet
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
            query = include(query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.Where(predicate);
        return await selector(query).ToListAsync(cancellationToken);
    }
    #endregion

    #region Command Operations
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public Task SoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(entity, cancellationToken);
    }
    #endregion

    #region Aggregate Operations
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }
    #endregion
}
