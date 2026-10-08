using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Common;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class GenericRepository<T>(RandaSportsDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    protected readonly RandaSportsDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public IQueryable<T> GetAll() => DbSet.AsNoTracking();

    public IQueryable<T> Where(Expression<Func<T, bool>> predicate) => DbSet.AsNoTracking().Where(predicate);

    public ValueTask<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbSet.FindAsync([id], cancellationToken);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(predicate, cancellationToken);

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => DbSet.CountAsync(predicate, cancellationToken);

    public async ValueTask AddAsync(T entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => DbSet.Update(entity);

    public void Delete(T entity) => DbSet.Remove(entity);
}
