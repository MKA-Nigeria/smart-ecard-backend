using Application.Card.CardRequests.Queries.Dto;
using Application.Common.Models;
using Mapster;

namespace Application.Card.CardRequests.Queries;
public class SearchCardRequest : PaginationFilter, IRequest<PaginationResponse<CardRequestDto>>
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}

public class SearchCardRequestHandler(IRepository<CardRequest> repository) : IRequestHandler<SearchCardRequest, PaginationResponse<CardRequestDto>>
{
    public async Task<PaginationResponse<CardRequestDto>> Handle(SearchCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequests = await repository.ListAsync(cancellationToken);
        
        cardRequests = (request.Keyword == null) ? cardRequests : [.. cardRequests.SearchByKeyword(request.Keyword)];

        if(request.Key is not null && request.Value is not null)
        {
            cardRequests = cardRequests.Where(cardRequest => cardRequest.CustomData.Any(kv => kv.Key.Contains(request.Key, StringComparison.CurrentCultureIgnoreCase) && kv.Value.Contains(request.Value, StringComparison.CurrentCultureIgnoreCase))).ToList();
        }
        var cardRequestsDto = cardRequests.ConvertAll(request => new CardRequestDto
        {
            MemberData = request.CardData.Adapt<MemberData>(),
            ExternalId = request.ExternalId,
            Id = request.Id,
            Status = request.Status
        });

        cardRequestsDto.ForEach(dto => dto.MemberData.CustomData = dto.MemberData.CustomData.ToDictionary());

        return new PaginationResponse<CardRequestDto>(cardRequestsDto, cardRequestsDto.Count, request.PageNumber, request.PageSize);
    }
}

