using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.Models;
using Domain.Cards;
using Domain.Enums;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cards.Cards.Queries;

public class SearchCardsRequest : PaginationFilter, IRequest<PaginationResponse<CardDto>>
{

}

public class SearchCardsRequestHandler(IRepository<Card> repository, IRepository<CardRequest> cardRequestRepository) : IRequestHandler<SearchCardsRequest, PaginationResponse<CardDto>>
{
    public async Task<PaginationResponse<CardDto>> Handle(SearchCardsRequest request, CancellationToken cancellationToken)
    {
        var activeCards = await repository.ListAsync(x => x.Status == CardStatus.Active, cancellationToken);

        activeCards = string.IsNullOrEmpty(request.Keyword) ? activeCards : [.. activeCards.SearchByKeyword(request.Keyword)];

        List<CardDto> cardsDto = new();
        foreach (var item in activeCards)
        {
            var cardRequest = await cardRequestRepository.FirstOrDefaultAsync(x => x.Id == item.CardRequestId);
            var cardDto = new CardDto
            {
                Id = item.Id,
                ApprovedDate = item.CreatedOn,
                CardNumber = item.CardNumber,
                Name = $"{cardRequest.CardData.FirstName} {cardRequest.CardData.LastName}",
                IsCollected = item.IsCollected,
                PrintStatus = item.PrintStatus
            };
            cardsDto.Add(cardDto);
        }

        return new PaginationResponse<CardDto>(cardsDto, cardsDto.Count, request.PageNumber, request.PageSize);
    }
}