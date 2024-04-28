using Application.Common.Persistence;
using Domain.Common.Contracts;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repository;

public class ApplicationDbRepository<T>(ApplicationDbContext dbContext) : IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    protected readonly ApplicationDbContext _context = dbContext;

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddRangeAsync(entities);
        return entities;
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
         _context.Set<T>().Remove(entity);
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().RemoveRange(entities);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().UpdateRange(entities);
    }

    // Get an entity by its ID
    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    // Get an entity by a specified predicate
    public async Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    // Get a specific projection of an entity by a specified predicate
    public async Task<TResult?> GetByExpressionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(criteria).Select(selector).FirstOrDefaultAsync(cancellationToken);
    }

    // Get the first or default entity matching the predicate
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var x = await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        return x; 
    }

    // Get a specific projection of the first or default entity matching the predicate
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(criteria).Select(selector).FirstOrDefaultAsync(cancellationToken);
    }

    // Get the single or default entity matching the predicate
    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    // Get a specific projection of the single or default entity matching the predicate
    public async Task<TResult?> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(criteria).Select(selector).SingleOrDefaultAsync(cancellationToken);
    }

    // List all entities
    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }

    // List all entities matching the predicate
    public async Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
    }

    // List all entities matching the predicate with a specific projection
    public async Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(criteria).Select(selector).ToListAsync(cancellationToken);
    }

    // Count all entities matching the predicate
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().CountAsync(predicate, cancellationToken);
    }

    // Count all entities
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().CountAsync(cancellationToken);
    }

    // Check if any entity matches the predicate
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().AnyAsync(predicate, cancellationToken);
    }

    // Check if any entity exists
    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().AnyAsync(cancellationToken);
    }
}