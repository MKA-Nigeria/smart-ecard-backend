using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Dtos;
using Domain.Enums;
using Mapster;

namespace Application.Cards.CardRequests.Queries;
public class GetCardRequestByExternalId : IRequest<BaseResponse<CardRequestDto>>
{
    public string EntityId { get; set; } = default!;
}
public class GetCardRequestByExternalIdHandler(IRepository<CardRequest> repository) : IRequestHandler<GetCardRequestByExternalId, BaseResponse<CardRequestDto>>
{
    public async Task<BaseResponse<CardRequestDto>> Handle(GetCardRequestByExternalId request, CancellationToken cancellationToken)
    {
        var cardRequest = await repository.GetByExpressionAsync(x => x.ExternalId == request.EntityId && x.Status == CardRequestStatus.Pending, cancellationToken);
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
        return new BaseResponse<CardRequestDto>
        {
            Data = cardRequestDto,
            Message = "Success",
            Status = true
        };
    }
}

