using Application.Cards.CardRequests.Queries.Dto;
using Mapster;

namespace Application.Cards.CardRequests.Queries;
public class GetCardRequest : IRequest<CardRequestDto>
{
    public DefaultIdType CardRequestId { get; set; } = default!;
}
public class GetCardRequestValidator : CustomValidator<GetCardRequest>
{
    public GetCardRequestValidator(IReadRepository<CardRequest> repository)
    {
        RuleFor(x => x.CardRequestId).NotEmpty().MustAsync(async (cardRequestId, _) => repository.FirstOrDefaultAsync(x => x.Id == cardRequestId, _) is not null).WithMessage("Invalid card request Id");
    }

}
public class GetCardRequestHandler(IRepository<CardRequest> repository) : IRequestHandler<GetCardRequest, CardRequestDto>
{
    public async Task<CardRequestDto> Handle(GetCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequest = await repository.GetByExpressionAsync(x => x.Id == request.CardRequestId, cancellationToken);
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

