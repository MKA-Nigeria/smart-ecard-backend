using Application.Common.Exceptions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cards.Cards.Commands;
public class DeactivateCardRequest : IRequest<DefaultIdType>
{
    public string CardNumber { get; set; } = default!;
}

public class DeactivateCardValidator : CustomValidator<ActivateCardRequest>
{
    public DeactivateCardValidator(IRepository<Card> repository)
    {

        RuleFor(x => x.CardNumber).NotEmpty().MustAsync(async (cardNumber, _) =>
        await repository.FirstOrDefaultAsync(x => x.CardNumber == cardNumber, _) is Card card).WithMessage("Invalid card Id");
    }

}

public class DeactivateCardHandler(DeactivateCardValidator validator, IRepository<Card> _repository) : IRequestHandler<DeactivateCardRequest, DefaultIdType>
{

    public async Task<DefaultIdType> Handle(DeactivateCardRequest request, CancellationToken cancellationToken)
    {
        var card = await _repository.FirstOrDefaultAsync(x => x.CardNumber == request.CardNumber, cancellationToken);
        _ = card ?? throw new NotFoundException($"Card request with Id {request.CardNumber} not found");
        card.ChangeCardStatus(CardStatus.InActive);

        await _repository.UpdateAsync(card, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return card.Id;
    }

}