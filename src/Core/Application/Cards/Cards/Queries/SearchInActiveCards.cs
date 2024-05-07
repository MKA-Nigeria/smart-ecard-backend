using Application.Cards.Cards.Dto;
using Application.Common.Models;
using Domain.Enums;

namespace Application.Cards.Cards.Queries;

public class SearchInActiveCardsRequest : PaginationFilter, IRequest<PaginationResponse<CardDto>>
{

}

public class SearchInActiveCardsRequestHandler(IRepository<Card> repository, IRepository<CardRequest> cardRequestRepository) : IRequestHandler<SearchInActiveCardsRequest, PaginationResponse<CardDto>>
{
    public async Task<PaginationResponse<CardDto>> Handle(SearchInActiveCardsRequest request, CancellationToken cancellationToken)
    {
        var activeCards = await repository.ListAsync(x => x.Status == CardStatus.InActive, cancellationToken);
        List<CardDto> cardsDto = [];
        foreach (var item in activeCards)
        {
            var cardRequest = await cardRequestRepository.FirstOrDefaultAsync(x => x.Id == item.CardRequestId, cancellationToken);
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

        cardsDto = string.IsNullOrEmpty(request.Keyword) ? cardsDto : [.. cardsDto.SearchByKeyword(request.Keyword)];
        return new PaginationResponse<CardDto>(cardsDto, cardsDto.Count, request.PageNumber, request.PageSize);
    }
}