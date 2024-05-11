using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Lookups.Validators;
public class ToggleDefaultStatusCommand : IRequest<DefaultIdType>
{
    public DefaultIdType LookupId { get; set; } = default!;
    public bool IsDefault { get; set; }
}

public class ToggleDefaultStatusCommandHandler(IRepository<Lookup> repository) : IRequestHandler<ToggleDefaultStatusCommand, DefaultIdType>
{
    private readonly IRepository<Lookup> _repository = repository;

    public async Task<DefaultIdType> Handle(ToggleDefaultStatusCommand request, CancellationToken cancellationToken)
    {
        var lookup = await _repository.FirstOrDefaultAsync(x => x.Id == request.LookupId, cancellationToken);
        _ = lookup ?? throw new NotFoundException($"lookup with Id {request.LookupId} not found");

        lookup.ToggleDefaultStatus(request.IsDefault);

        await _repository.UpdateAsync(lookup, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return lookup.Id;
    }
}