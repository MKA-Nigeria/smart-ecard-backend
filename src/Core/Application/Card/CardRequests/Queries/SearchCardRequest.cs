﻿using Application.Card.CardRequests.Queries.Dto;
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
        
        cardRequests = string.IsNullOrEmpty(request.Keyword) ? cardRequests : [.. cardRequests.SearchByKeyword(request.Keyword)];

        if(request.Key is not null && request.Value is not null)
        {
            cardRequests = cardRequests.Where(cardRequest => cardRequest.CustomData.Any(kv => kv.Key.Contains(request.Key, comparisonType: StringComparison.CurrentCultureIgnoreCase) && kv.Value.Contains(request.Value, StringComparison.CurrentCultureIgnoreCase))).ToList();
        }

        List<CardRequestDto> cardRequestsDto = cardRequests.ConvertAll(cardRequest =>
        {
            var cardRequestDto = new CardRequestDto
            {
                MemberData = cardRequest.CardData.Adapt<MemberData>(),
                ExternalId = cardRequest.ExternalId,
                Id = cardRequest.Id,
                Status = cardRequest.Status
            };

            cardRequestDto.MemberData.CustomData = cardRequest.CustomData.ToDictionary();
            cardRequestDto.MemberData.EntityId = cardRequest.ExternalId;

            return cardRequestDto;
        });
        return new PaginationResponse<CardRequestDto>(cardRequestsDto, cardRequestsDto.Count, request.PageNumber, request.PageSize);
    }
}