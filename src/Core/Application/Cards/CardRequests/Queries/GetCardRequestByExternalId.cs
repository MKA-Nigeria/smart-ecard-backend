using Application.Cards.CardRequests.Queries.Dto;
using Domain.Enums;
using Mapster;

namespace Application.Cards.CardRequests.Queries;
public class GetCardRequestByExternalId : IRequest<CardRequestDto>
{
    public string EntityId{ get; set; } = default!;
}
public class GetCardRequestByExternalIdHandler(IRepository<CardRequest> repository) : IRequestHandler<GetCardRequestByExternalId, CardRequestDto>
{
    public async Task<CardRequestDto> Handle(GetCardRequestByExternalId request, CancellationToken cancellationToken)
    {
        var cardRequest = await repository.GetByExpressionAsync(x => x.ExternalId == request.EntityId && x.Status == CardRequestStatus.Pending, cancellationToken) ?? throw new Exception($"No card request for {request.EntityId}");
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
    }
}

