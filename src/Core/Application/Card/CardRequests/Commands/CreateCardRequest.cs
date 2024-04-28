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

public class CreateCardRequestHandler(IRepository<CardRequest> repository) : IRequestHandler<CreateCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    public async Task<DefaultIdType> Handle(CreateCardRequest request, CancellationToken cancellationToken)
    {
        var cardRequestData = new CardRequestData("Ajibike", "Ambaaq", DateTime.Now, "Monatan", "ambaaq@gmail.com", "08144875105", "ambaaq.png", Domain.Enums.Gender.Male, "", "");

        var customData = new Dictionary<string, object>();

        var cardRequest = new CardRequest(request.ExternalId, request.Biometrics.Adapt<BiometricData>(), cardRequestData, customData);

        await _repository.AddAsync(cardRequest, cancellationToken);
        await _repository.SaveChangesAsync();
        return cardRequest.Id;
    }
}