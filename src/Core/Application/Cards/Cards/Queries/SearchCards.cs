using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.FileStorage;
using Application.Common.Models;
using Domain.Enums;
using Mapster;

namespace Application.Cards.Cards.Queries;

public class SearchCardsRequest : PaginationFilter, IRequest<PaginationResponse<CardDto>>
{
    public PrintStatus? PrintStatus { get; set; }
    public CardStatus? CardStatus { get; set; }
    public bool? IsCollected { get; set; }
}

public class SearchCardstHandler(IRepository<Card> repository, IRepository<CardRequest> cardRequestRepository, IFileStorageService fileStorageService) : IRequestHandler<SearchCardsRequest, PaginationResponse<CardDto>>
{
    public async Task<PaginationResponse<CardDto>> Handle(SearchCardsRequest request, CancellationToken cancellationToken)
    {
        var activeCards = await repository.ListAsync(cancellationToken);
        activeCards = string.IsNullOrEmpty(request.Keyword) ? activeCards : [.. activeCards.SearchByKeyword(request.Keyword)];
        // Apply filters
        if (request.IsCollected.HasValue)
        {
            activeCards = activeCards.Where(card => card.IsCollected == request.IsCollected.Value).ToList();
        }

        if (request.PrintStatus.HasValue)
        {
            activeCards = activeCards.Where(card => card.PrintStatus == request.PrintStatus.Value).ToList();
        }

        if (request.CardStatus.HasValue)
        {
            activeCards = activeCards.Where(card => card.Status == request.CardStatus.Value).ToList();
        }

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