using Domain.Entities;

namespace Application.Lookups.Commands;
public class CreateLookupCommand : IRequest<DefaultIdType>
{
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public class CreateLookupCommandHanler(IRepository<Lookup> repository) : IRequestHandler<CreateLookupCommand, DefaultIdType>
{
    private readonly IRepository<Lookup> _repository = repository;
    public async Task<DefaultIdType> Handle(CreateLookupCommand request, CancellationToken cancellationToken)
    {
        var lookup = new Lookup(request.Type, request.Name, request.Value);

        await _repository.AddAsync(lookup, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return lookup.Id;
    }
}
