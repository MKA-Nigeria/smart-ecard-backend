using Application.Common.Exceptions;
using Domain.Enums;

namespace Application.Cards.CardRequests.Commands;
public class CancelCardRequest : IRequest<DefaultIdType>
{
    public DefaultIdType CardRequestId { get; set; } = default!;
}

public class CancelCardRequestValidator : CustomValidator<CancelCardRequest>
{
    public CancelCardRequestValidator(IReadRepository<CardRequest> repository)
    {
        RuleFor(x => x.CardRequestId).NotEmpty().MustAsync(async (cardRequestId, _) => repository.FirstOrDefaultAsync(x => x.Id == cardRequestId, _) is not null).WithMessage("Invalid card request Id");
    }

}

public class CancelCardRequestHandler(IRepository<CardRequest> repository) : IRequestHandler<CancelCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    public async Task<DefaultIdType> Handle(CancelCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequest = await _repository.FirstOrDefaultAsync(x => x.Id == request.CardRequestId, cancellationToken);

        _ = cardRequest ?? throw new NotFoundException($"Card request with Id {request.CardRequestId} not found");
        cardRequest.Cancel();

        await _repository.UpdateAsync(cardRequest);
        await _repository.SaveChangesAsync();
        return cardRequest.Id;
    }
}