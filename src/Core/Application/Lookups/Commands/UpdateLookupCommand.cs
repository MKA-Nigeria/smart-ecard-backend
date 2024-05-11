using Application.Common.Exceptions;
using Application.Lookups.Dtos;
using Domain.Entities;

namespace Application.Lookups.Commands;
public class UpdateLookupCommand : IRequest<DefaultIdType>
{
    public DefaultIdType LookupId { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public class UpdateLookupCommandHandler(IRepository<Lookup> repository) : IRequestHandler<UpdateLookupCommand, DefaultIdType>
{
    private readonly IRepository<Lookup> _repository = repository;

    public async Task<DefaultIdType> Handle(UpdateLookupCommand request, CancellationToken cancellationToken)
    {
        var lookup = await _repository.FirstOrDefaultAsync(x => x.Id == request.LookupId, cancellationToken);
        _ = lookup ?? throw new NotFoundException($"lookup with Id {request.LookupId} not found");

        lookup.Update(request.Value);

        await _repository.UpdateAsync(lookup, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return lookup.Id;
    }
}