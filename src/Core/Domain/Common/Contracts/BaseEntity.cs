using Domain.Common.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common.Contracts;
public abstract class BaseEntity : BaseEntity<DefaultIdType>
{
    protected BaseEntity() => Id = Guid.NewGuid();
}

public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; protected set; } = default!;

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new();
}