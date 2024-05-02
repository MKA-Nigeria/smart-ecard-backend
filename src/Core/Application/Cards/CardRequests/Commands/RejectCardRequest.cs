using Application.Common.Exceptions;
using Domain.Enums;

namespace Application.Cards.CardRequests.Commands;
public class RejectCardRequest : IRequest<DefaultIdType>
{
    public DefaultIdType CardRequestId { get; set; } = default!;
    public string Reason { get; set; } = default!;
}

public class RejectCardRequestValidator : CustomValidator<RejectCardRequest>
{
    public RejectCardRequestValidator(IReadRepository<CardRequest> repository)
    {
        RuleFor(x => x.CardRequestId).NotEmpty().MustAsync(async (cardRequestId, _) => repository.FirstOrDefaultAsync(x => x.Id == cardRequestId, _) is not null).WithMessage("Invalid card request Id");

        RuleFor(x => x.Reason).NotEmpty().MaximumLength(25);
    }

}

public class RejectCardRequestHandler(IRepository<CardRequest> repository) : IRequestHandler<RejectCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    public async Task<DefaultIdType> Handle(RejectCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequest = await _repository.FirstOrDefaultAsync(x => x.Id == request.CardRequestId, cancellationToken);

        _ = cardRequest ?? throw new NotFoundException($"Card request with Id {request.CardRequestId} not found");
        cardRequest.Reject(request.Reason);

        await _repository.UpdateAsync(cardRequest);
        await _repository.SaveChangesAsync();
        return cardRequest.Id;
    }
}