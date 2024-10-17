using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.Dtos;
using Application.Common.FileStorage;
using Domain.Cards;
using Domain.Enums;
using Mapster;

namespace Application.Cards.CardRequests.Queries;
public class GetCardRequestByExternalId : IRequest<BaseResponse<CardRequestDto>>
{
    public string EntityId { get; set; } = default!;
}
public class GetCardRequestByExternalIdHandler(IRepository<CardRequest> repository, IFileStorageService fileStorageService) : IRequestHandler<GetCardRequestByExternalId, BaseResponse<CardRequestDto>>
{
    public async Task<BaseResponse<CardRequestDto>> Handle(GetCardRequestByExternalId request, CancellationToken cancellationToken)
    {
        var cardRequest = await repository.GetByExpressionAsync(x => x.ExternalId == request.EntityId && x.Status == CardRequestStatus.Pending, cancellationToken);

        string imageData = await fileStorageService.GetImageDataAsync(cardRequest.CardData.PhotoUrl);

        if (cardRequest is null)
        {
            return new BaseResponse<CardRequestDto>
            {
                Message = "No card request exist",
                Status = false,
            };
            //use logging
            //throw new Exception($"No card request for {request.EntityId}");
        }
        var cardRequestDto = new CardRequestDto
        {
            MemberData = cardRequest.CardData.Adapt<MemberData>(),
            ExternalId = cardRequest.ExternalId,
            Id = cardRequest.Id,
            Status = cardRequest.Status
        };
        cardRequestDto.MemberData.CustomData = cardRequest.CustomData.ToDictionary();
        cardRequestDto.MemberData.EntityId = cardRequest.ExternalId;
        cardRequestDto.MemberData.PhotoUrl = imageData ?? null;
        return new BaseResponse<CardRequestDto>
        {
            Data = cardRequestDto,
            Message = "Success",
            Status = true
        };
    }
}

