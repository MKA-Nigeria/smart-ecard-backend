using Application.Card.CardRequests.Queries.Dto;
using Application.Common.Exceptions;
using Application.Gateway;
using Domain.Entities;
using Domain.Enums;
using Mapster;
namespace Application.Card.CardRequests.Commands;

public class BiometricDataRequest
{
    public byte[] Fingerprint { get; set; }
    public byte[] RetinalScan { get; set; }
}
public class CreateCardRequest : IRequest<DefaultIdType>
{
    public string ExternalId { get; set; } = default!;
    public MemberData MemberData { get; set; }
    public BiometricDataRequest? Biometrics { get; set; }

}
public class CreateCardRequestValidator : CustomValidator<CreateCardRequest>
{
    public CreateCardRequestValidator(IReadRepository<CardRequest> repository) => RuleFor(p => p.ExternalId)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (externalId, _) => await repository.FirstOrDefaultAsync(c => c.ExternalId == externalId, _) is null)
                .WithMessage((_, externalId) => $"Card Request aleady exist for {externalId}");

}

public class CreateCardRequestHandler(IRepository<CardRequest> repository, IGatewayHandler gateway, IRepository<AppConfiguration> configRepo) : IRequestHandler<CreateCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    private readonly IGatewayHandler _gateway = gateway;
    public async Task<DefaultIdType> Handle(CreateCardRequest request, CancellationToken cancellationToken)
    {
        var existingCardRequest = await _repository.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && (x.Status != CardRequestStatus.Rejected || x.Status != CardRequestStatus.Cancelled), cancellationToken);

        if(existingCardRequest != null)
        {
            throw new ForbiddenException("The member has an existing card");
        }

        var cardRequestData = request.MemberData.Adapt<CardRequestData>();

        var cardRequest = new CardRequest(request.ExternalId, request.Biometrics.Adapt<BiometricData>(), cardRequestData, request.MemberData.CustomData);

        await _repository.AddAsync(cardRequest, cancellationToken);
        await _repository.SaveChangesAsync();
        return cardRequest.Id;
    }
}