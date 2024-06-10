using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Newtonsoft.Json;

namespace Application.Cards.Cards.Commands;
public class SetCardPrintedRequest : IRequest<DefaultIdType>
{
    public string CardNumber { get; set; } = default!;
}

public class SetCardPrintedValidator : CustomValidator<SetCardPrintedRequest>
{
    public SetCardPrintedValidator(IRepository<Card> repository)
    {

        RuleFor(x => x.CardNumber).NotEmpty().MustAsync(async (cardNumber, _) =>
        await repository.FirstOrDefaultAsync(x => x.CardNumber == cardNumber, _) is Card card).WithMessage("Invalid card Id");
    }

}

public class SetCardPrintedHandler(SetCardPrintedValidator validator, IRepository<Card> _repository) : IRequestHandler<SetCardPrintedRequest, DefaultIdType>
{

    public async Task<DefaultIdType> Handle(SetCardPrintedRequest request, CancellationToken cancellationToken)
    {
        var card = await _repository.FirstOrDefaultAsync(x => x.CardNumber == request.CardNumber, cancellationToken);
        _ = card ?? throw new NotFoundException($"Card request with Id {request.CardNumber} not found");
        card.ChangePrintStatus(PrintStatus.Printed);

        await _repository.UpdateAsync(card, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return card.Id;
    }

}