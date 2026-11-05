using Microsoft.EntityFrameworkCore;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Infrastructure.Persistence.DBContext;
using RealTimeSystem.Infrastructure.Persistence.Repository;

namespace RealTimeSystem.Infrastructure.Persistence.UnitOfWork
{
public sealed class EfUnitOfWork(ApplicationDbContext applicationDbContext) : IUnitOfWork
{
    private readonly DbContext _dbContext = applicationDbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (_repositories.TryGetValue(type, out var repository))
        {
            return (IRepository<TEntity>)repository;
        }

        var created = new EfRepository<TEntity>(_dbContext);
        _repositories[type] = created;
        return created;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
}
