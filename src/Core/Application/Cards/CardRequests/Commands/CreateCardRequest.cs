using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Exceptions;
using Application.Common.FileStorage;
using Application.Gateway;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using MediatR;
namespace Application.Cards.CardRequests.Commands;

public class BiometricDataRequest
{
    public byte[] Fingerprint { get; set; }
    public byte[] RetinalScan { get; set; }
}
public class CreateCardRequest : IRequest<DefaultIdType>
{
    public string ExternalId { get; set; } = default!;
    public MemberData MemberData { get; set; }
    public FileUploadRequest? ImageRequest { get; set; }
    public BiometricDataRequest? Biometrics { get; set; }

}

public class CreateCardRequestValidator : CustomValidator<CreateCardRequest>
{
    public CreateCardRequestValidator(IReadRepository<CardRequest> repository) => RuleFor(p => p.ExternalId)
            .NotEmpty()
            .MaximumLength(20)
                .WithMessage((_, externalId) => $"Card Request aleady exist for {externalId}");

}

public class CreateCardRequestHandler(IRepository<CardRequest> repository, IRepository<Card> cardRepository, IGatewayHandler gateway, IRepository<AppConfiguration> configRepo, IFileStorageService fileStorage) : IRequestHandler<CreateCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    private readonly IRepository<Card> _cardRepository = cardRepository;
    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    private readonly IGatewayHandler _gateway = gateway;
    private readonly IFileStorageService _fileStorage = fileStorage;
    public async Task<DefaultIdType> Handle(CreateCardRequest request, CancellationToken cancellationToken)
    {
        var existingCard = await _cardRepository.FirstOrDefaultAsync(x => x.CardRequest.ExternalId == request.ExternalId);
        if (existingCard != null && existingCard.Status != CardStatus.InActive)
        {
            throw new ForbiddenException("The member has an active card");
        }

        var existingCardRequest = await _repository.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId, cancellationToken);
        if (existingCardRequest != null && (existingCardRequest.Status != CardRequestStatus.Rejected && existingCardRequest.Status != CardRequestStatus.Cancelled))
        {
            throw new ForbiddenException("The member has a pending card request");
        }

        if (request.ImageRequest is null)
        {
            throw new ForbiddenException("Image is required");
        }

        string imageName = await _fileStorage.UploadAsync<string>(request.ImageRequest, FileType.Image);
        var cardRequestData = request.MemberData.Adapt<CardRequestData>();
        cardRequestData.PhotoUrl = imageName;

        var cardRequest = new CardRequest(request.ExternalId, request.Biometrics.Adapt<BiometricData>(), cardRequestData, request.MemberData.CustomData);

        await _repository.AddAsync(cardRequest, cancellationToken);
        await _repository.SaveChangesAsync();
        return cardRequest.Id;
    }
}