using Application.Common.Persistence;
using Domain.Common.Contracts;
using Domain.Common.Events;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repository;

/// <summary>
/// The repository that implements IRepositoryWithEvents.
/// Implemented as a decorator. It only augments the Add,
/// Update and Delete calls where it adds the respective
/// EntityCreated, EntityUpdated or EntityDeleted event
/// before delegating to the decorated repository.
/// </summary>
public class EventAddingRepositoryDecorator<T> : IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    private readonly IRepository<T> _decorated;

    public EventAddingRepositoryDecorator(IRepository<T> decorated) => _decorated = decorated;

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.DomainEvents.Add(EntityCreatedEvent.WithEntity(entity));
        return _decorated.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.DomainEvents.Add(EntityUpdatedEvent.WithEntity(entity));
        return _decorated.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.DomainEvents.Add(EntityDeletedEvent.WithEntity(entity));
        return _decorated.DeleteAsync(entity, cancellationToken);
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.DomainEvents.Add(EntityDeletedEvent.WithEntity(entity));
        }

        return _decorated.DeleteRangeAsync(entities, cancellationToken);
    }

    // The rest of the methods are simply forwarded.
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _decorated.SaveChangesAsync(cancellationToken);
    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull =>
        _decorated.GetByIdAsync(id, cancellationToken);
    public Task<TResult?> GetBySpecAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        _decorated.FirstOrDefaultAsync(criteria, selector, cancellationToken);
    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        _decorated.ListAsync(cancellationToken);
    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _decorated.ListAsync(predicate, cancellationToken);
    public Task<List<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        _decorated.ListAsync(criteria, selector, cancellationToken);
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _decorated.AnyAsync(predicate, cancellationToken);
    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _decorated.AnyAsync(cancellationToken);
    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _decorated.CountAsync(predicate, cancellationToken);
    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _decorated.CountAsync(cancellationToken);

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
        _decorated.AddRangeAsync(entities, cancellationToken);

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
        _decorated.UpdateRangeAsync(entities, cancellationToken);

    public Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    => _decorated.FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    => _decorated.FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        _decorated.FirstOrDefaultAsync<TResult>(criteria, selector, cancellationToken);

    public Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _decorated.SingleOrDefaultAsync(predicate, cancellationToken);

    public Task<TResult?> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> criteria,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        _decorated.SingleOrDefaultAsync<TResult>(criteria, selector, cancellationToken);

    public Task<TResult?> GetByExpressionAsync<TResult>(Expression<Func<T, bool>> criteria, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
    {
       return _decorated.FirstOrDefaultAsync(criteria, selector, cancellationToken);
    }
}