using Domain.Common.Contracts;
using System.Linq.Expressions;

namespace Application.Common.Persistence;

/// <summary>
/// The regular read/write repository for an aggregate root.
/// </summary>
public interface IRepository<T>
    where T : class, IAggregateRoot
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull;
    Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> GetByExpressionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// The read-only repository for an aggregate root.
/// </summary>
public interface IReadRepository<T>
    where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull;
    Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> GetByExpressionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
public interface IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull;
    Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> GetByExpressionAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TResult?> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}