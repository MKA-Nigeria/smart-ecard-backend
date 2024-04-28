using Application.Common.Exceptions;
using Domain.Enums;

namespace Application.Card.CardRequests.Commands;
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

public class ApproveCardRequestHandler(IRepository<CardRequest> repository, ApproveCardRequestValidator validator) : IRequestHandler<ApproveCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    public async Task<DefaultIdType> Handle(ApproveCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequest = await _repository.FirstOrDefaultAsync(x => x.Id == request.CardRequestId, cancellationToken);
        _ = cardRequest ?? throw new NotFoundException($"Card request with Id {request.CardRequestId} not found");
        cardRequest.Approve();

        await _repository.UpdateAsync(cardRequest, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return cardRequest.Id;
    }
}