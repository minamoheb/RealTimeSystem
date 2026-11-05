using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RealTimeSystem.Application.Common.Interfaces;
using System.Linq.Expressions;

namespace RealTimeSystem.Infrastructure.Persistence.Repository
{
public sealed class EfRepository<TEntity>(DbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _set = dbContext.Set<TEntity>();

    public IQueryable<TEntity> Query(bool asNoTracking = true) =>
        asNoTracking ? _set.AsNoTracking() : _set;

    public ValueTask<TEntity?> FindAsync(object id, CancellationToken cancellationToken = default) =>
        _set.FindAsync([id], cancellationToken);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        _set.AddAsync(entity, cancellationToken).AsTask();

    public void Update(TEntity entity) => _set.Update(entity);

    public void Remove(TEntity entity) => _set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _set.RemoveRange(entities);

    public Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool asNoTracking = true,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, orderBy, include, asNoTracking, ignoreQueryFilters: false);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<TResult>> SelectListAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool asNoTracking = true,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, orderBy, include, asNoTracking, ignoreQueryFilters: false);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return query.Select(selector).ToListAsync(cancellationToken);
    }

    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool asNoTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, orderBy, include, asNoTracking, ignoreQueryFilters);
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool asNoTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, orderBy, include, asNoTracking, ignoreQueryFilters);
        return query.Select(selector).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _set.AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query.CountAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        _set.AnyAsync(predicate, cancellationToken);

    private IQueryable<TEntity> BuildQuery(
        Expression<Func<TEntity, bool>>? predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
        bool asNoTracking,
        bool ignoreQueryFilters)
    {
        IQueryable<TEntity> query = _set;

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query).AsSplitQuery();
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return query;
    }
}
}