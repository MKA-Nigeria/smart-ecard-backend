using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.FileStorage;
using Application.Common.Models;
using Domain.Cards;
using Domain.Enums;
using Mapster;

namespace Application.Cards.Cards.Queries;

public class SearchCardsRequest : PaginationFilter, IRequest<PaginationResponse<CardDto>>
{
    public string? Printed { get; set; }
    public string? Collected { get; set; }
    public string? Active { get; set; }
}

public class SearchCardstHandler(IRepository<Card> repository, IRepository<CardRequest> cardRequestRepository, IFileStorageService fileStorageService) : IRequestHandler<SearchCardsRequest, PaginationResponse<CardDto>>
{
    public async Task<PaginationResponse<CardDto>> Handle(SearchCardsRequest request, CancellationToken cancellationToken)
    {
        var activeCards = await repository.ListAsync(cancellationToken);
        activeCards = string.IsNullOrEmpty(request.Keyword) ? activeCards : [.. activeCards.SearchByKeyword(request.Keyword)];
        activeCards = string.IsNullOrEmpty(request.Printed) ? activeCards : [.. activeCards.SearchByKeyword(request.Printed)];
        activeCards = string.IsNullOrEmpty(request.Collected) ? activeCards : [.. activeCards.SearchByKeyword(request.Collected)];
        activeCards = string.IsNullOrEmpty(request.Active) ? activeCards : [.. activeCards.SearchByKeyword(request.Active)];

        List<CardDto> cardsDto = [];
        foreach (var item in activeCards)
        {
            var cardRequest = await cardRequestRepository.FirstOrDefaultAsync(x => x.Id == item.CardRequestId, cancellationToken);
            //string imageData = await fileStorageService.GetImageDataAsync(cardRequest.CardData.PhotoUrl);
            var cardDto = new CardDto
            {
                //Id = item.Id,
                ApprovedDate = item.CreatedOn,
                CardNumber = item.CardNumber,
                ExternalId = cardRequest.ExternalId,
                FullName = $"{cardRequest.CardData.FirstName} {cardRequest.CardData.LastName}",
                IsCollected = item.IsCollected,
                PrintStatus = item.PrintStatus,
                DateCollected = item.DateCollected,
                RequestDate = cardRequest.CreatedOn,
                CardStatus = item.Status,
                ApprovedBy = "Admin",
                MemberData = cardRequest.CardData.Adapt<MemberData>(),
            };
            cardDto.MemberData.CustomData = cardRequest.CustomData.ToDictionary();
            cardDto.MemberData.EntityId = cardRequest.ExternalId;
            cardDto.MemberData.PhotoUrl = null;
            cardsDto.Add(cardDto);
        }

        var cardPaginationResponse = new PaginationResponse<CardDto>(cardsDto, cardsDto.Count, request.PageNumber, request.PageSize);

        return cardPaginationResponse;
    }
}