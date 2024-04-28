using Application.Card.CardRequests.Commands;
using Application.Card.CardRequests.Queries.Dto;
using Application.Common.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Card.CardRequests.Queries;
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
        var cardRequestDto = cardRequest.Adapt<CardRequestDto>();

        return cardRequestDto;
    }
}

