using Application.Lookups.Dtos;
using Domain.Entities;
using Mapster;

namespace Application.Lookups.Queries;
public record GetLookupsByTypeQuery(string Type) : IRequest<List<LookupDto>>;

public class GetLookupsByTypeQueryHandler(IRepository<Lookup> repository) : IRequestHandler<GetLookupsByTypeQuery, List<LookupDto>>
{
    private readonly IRepository<Lookup> _repository = repository;
    public async Task<List<LookupDto>> Handle(GetLookupsByTypeQuery request, CancellationToken cancellationToken)
    {
        var lookups = await _repository.ListAsync(l => l.Type == request.Type, cancellationToken);

        return lookups.Adapt<List<LookupDto>>();
    }
}