using Application.Card.CardRequests.Queries.Dto;
using Application.Common.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Card.CardRequests.Queries;
public class SearchCardRequest : PaginationFilter, IRequest<PaginationResponse<CardRequestDto>>
{
}

public class SearchCardRequestHandler(IRepository<CardRequest> repository) : IRequestHandler<SearchCardRequest, PaginationResponse<CardRequestDto>>
{
    public async Task<PaginationResponse<CardRequestDto>> Handle(SearchCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequests = await repository.ListAsync(cancellationToken);
        var cardRequestsDto = cardRequests.Adapt<List<CardRequestDto>>();
        return new PaginationResponse<CardRequestDto>(cardRequestsDto, cardRequestsDto.Count, request.PageNumber, request.PageSize);
    }
}

