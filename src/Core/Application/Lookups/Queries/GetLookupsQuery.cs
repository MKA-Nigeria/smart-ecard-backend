using Application.Common.Models;
using Application.Lookups.Dtos;
using Domain.Entities;
using Mapster;

namespace Application.Lookups.Queries;
public class GetLookupsQuery : PaginationFilter, IRequest<PaginationResponse<LookupDto>>;

public class GetLookupsQueryHandler(IRepository<Lookup> lookupRepo) : IRequestHandler<GetLookupsQuery, PaginationResponse<LookupDto>>
{
    private readonly IRepository<Lookup> _lookupRepo = lookupRepo;
    public async Task<PaginationResponse<LookupDto>> Handle(GetLookupsQuery request, CancellationToken cancellationToken)
    {
        var lookups = await _lookupRepo.ListAsync(cancellationToken);
        var lookupResult = lookups.Adapt<List<LookupDto>>();

        return new PaginationResponse<LookupDto>(lookupResult, lookupResult.Count, request.PageNumber, request.PageSize);
    }
}

