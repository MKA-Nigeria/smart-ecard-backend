using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Identity.Users;
using Domain.Cards;
using Mapster;

namespace Application.Cards.Cards.Queries;
public class GetCardRequest : IRequest<CardDto>
{
    public string CardNumber { get; set; } = default!;
}
public class GetCardRequestValidator : CustomValidator<GetCardRequest>
{
    public GetCardRequestValidator(IReadRepository<Card> repository)
    {
        RuleFor(x => x.CardNumber).NotEmpty().MustAsync(async (cardNumber, _) => repository.FirstOrDefaultAsync(x => x.CardNumber == cardNumber, _) is not null).WithMessage("Invalid card number");
    }

}
public class GetCardRequestHandler(IRepository<Card> repository, IRepository<CardRequest> cardRequestRepository, IUserService userService) : IRequestHandler<GetCardRequest, CardDto>
{
    public async Task<CardDto> Handle(GetCardRequest request, CancellationToken cancellationToken)
    {
        var card = await repository.FirstOrDefaultAsync(x => x.CardNumber == request.CardNumber || x.CardRequest.ExternalId == request.CardNumber, cancellationToken) ?? throw new NotFoundException("Invalid card number");
        var cardRequest = await cardRequestRepository.FirstOrDefaultAsync(x => x.Id == card.CardRequestId, cancellationToken);
        var user = await userService.GetAsync(card.CreatedBy.ToString(), cancellationToken);
        var cardDto = new CardDto
        {
            //Id = item.Id,
            ApprovedDate = card.CreatedOn,
            CardNumber = card.CardNumber,
            ExternalId = cardRequest.ExternalId,
            FullName = $"{cardRequest.CardData.FirstName} {cardRequest.CardData.LastName}",
            IsCollected = card.IsCollected,
            PrintStatus = card.PrintStatus,
            DateCollected = card.DateCollected,
            RequestDate = cardRequest.CreatedOn,
            CardStatus = card.Status,
            ApprovedBy = $"{user.FirstName} {user.LastName}",
            MemberData = cardRequest.CardData.Adapt<MemberData>(),
        };
        cardDto.MemberData.CustomData = cardRequest.CustomData.ToDictionary();
        cardDto.MemberData.EntityId = cardRequest.ExternalId;
        return cardDto;
    }
}

