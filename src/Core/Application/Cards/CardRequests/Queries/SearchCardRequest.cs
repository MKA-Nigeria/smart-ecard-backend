using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.FileStorage;
using Application.Common.Models;
using Mapster;

namespace Application.Cards.CardRequests.Queries;
public class SearchCardRequest : PaginationFilter, IRequest<PaginationResponse<CardRequestDto>>
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}

public class SearchCardRequestHandler(IRepository<CardRequest> repository, IFileStorageService fileStorageService) : IRequestHandler<SearchCardRequest, PaginationResponse<CardRequestDto>>
{
    public async Task<PaginationResponse<CardRequestDto>> Handle(SearchCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequests = await repository.ListAsync(cancellationToken);
        
        cardRequests = string.IsNullOrEmpty(request.Keyword) ? cardRequests : [.. cardRequests.SearchByKeyword(request.Keyword)];

        if(request.Key is not null && request.Value is not null)
        {
            cardRequests = cardRequests.Where(cardRequest => cardRequest.CustomData.Any(kv => kv.Key.Contains(request.Key, comparisonType: StringComparison.CurrentCultureIgnoreCase) && kv.Value.Contains(request.Value, StringComparison.CurrentCultureIgnoreCase))).ToList();
        }

        List<CardRequestDto> cardRequestsDto = [];
        foreach (var cardRequest in cardRequests)
        {
            //string imageData = await fileStorageService.GetImageDataAsync(cardRequest.CardData.PhotoUrl);

            var cardRequestDto = new CardRequestDto
            {
                MemberData = cardRequest.CardData.Adapt<MemberData>(),
                ExternalId = cardRequest.ExternalId,
                Id = cardRequest.Id,
                Status = cardRequest.Status
            };

            cardRequestDto.MemberData.CustomData = cardRequest.CustomData.ToDictionary();
            cardRequestDto.MemberData.EntityId = cardRequest.ExternalId;
            cardRequestDto.MemberData.PhotoUrl = null;
            cardRequestsDto.Add(cardRequestDto);
        }

      /*  List<CardRequestDto> cardRequestsDto = cardRequests.ConvertAll(cardRequest =>
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
        });*/
        return new PaginationResponse<CardRequestDto>(cardRequestsDto, cardRequestsDto.Count, request.PageNumber, request.PageSize);
    }
}