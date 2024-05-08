using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Newtonsoft.Json;

namespace Application.Cards.CardRequests.Commands;
public class ApproveCardRequest : IRequest<DefaultIdType>
{
    public DefaultIdType CardRequestId { get; set; } = default!;
}

public class ApproveCardRequestValidator : CustomValidator<ApproveCardRequest>
{
    public ApproveCardRequestValidator(IRepository<CardRequest> repository)
    {

        RuleFor(x => x.CardRequestId).NotEmpty().MustAsync(async (cardRequestId, _) =>
        await repository.FirstOrDefaultAsync(x => x.Id == cardRequestId, _) is CardRequest card).WithMessage("Invalid card request Id");
    }

}

public class ApproveCardRequestHandler(IRepository<CardRequest> _repository, ApproveCardRequestValidator validator, IRepository<AppConfiguration> _configRepo, IRepository<Card> _cardRepository, ICurrentUser _currentUser) : IRequestHandler<ApproveCardRequest, DefaultIdType>
{
    public async Task<DefaultIdType> Handle(ApproveCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequest = await _repository.FirstOrDefaultAsync(x => x.Id == request.CardRequestId, cancellationToken);
        _ = cardRequest ?? throw new NotFoundException($"Card request with Id {request.CardRequestId} not found");
        var userId = _currentUser.GetUserId();
        cardRequest.Approve(userId);

        await _repository.UpdateAsync(cardRequest, cancellationToken);

        await CreateCard(cardRequest, userId);

        await _repository.SaveChangesAsync(cancellationToken);
        return cardRequest.Id;
    }

    public async Task CreateCard(CardRequest cardRequest, Guid createdBy)
    {
        var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == "CardData");

        if (dataModel == null || dataModel.Value == null)
        {
            return;
        }

        // Deserialize the JSON string into a dictionary
        Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataModel.Value);
        string orgName = (string) userData["Organisation"];
        string lengthAsString = userData["Length"].ToString();
        int length = int.Parse(lengthAsString);
        string uniqueKey = Guid.NewGuid().ToString().Substring(0, length);
        string cardNumber = $"{orgName}{cardRequest.ExternalId}{uniqueKey}";
        var card = new Card(cardNumber, cardRequest.Id, cardRequest, createdBy);
        await _cardRepository.AddAsync(card);
    }
}