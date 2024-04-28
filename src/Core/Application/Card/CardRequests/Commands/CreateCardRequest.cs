using Application.Gateway;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using Newtonsoft.Json;
using System.Collections;

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

public class CreateCardRequestHandler(IRepository<CardRequest> repository, IGatewayHandler gateway, IRepository<AppConfiguration> configRepo) : IRequestHandler<CreateCardRequest, DefaultIdType>
{
    private readonly IRepository<CardRequest> _repository = repository;
    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    private readonly IGatewayHandler _gateway = gateway;
    public async Task<DefaultIdType> Handle(CreateCardRequest request, CancellationToken cancellationToken)
    {
        var data = await _gateway.GetEntityAsync(request.ExternalId);

        var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == "UserData");

        if (dataModel == null || dataModel.Value == null)
        {
            throw new Exception("User Data not provided");
        }

        // Deserialize the JSON string into a dictionary
        Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataModel.Value);

        var gender = (data[userData["Gender"]] == "M") ? Gender.Male : Gender.Female;
        DateTime date = data[userData["DateOfBirth"]];
        var cardRequestData = new CardRequestData
        {
            FirstName = data[userData["FirstName"]],
            LastName = data[userData["LastName"]],
            DateOfBirth = date,
            Gender = gender,
            Address = data[userData["Address"]],
            Email = data[userData["Email"]],
            PhoneNumber = data[userData["PhoneNumber"]]
        };
        /*var cardRequestData = new CardRequestData(data[userData["FirstName"]], data[userData["LastName"]], date, data[userData["Address"]], data[userData["Email"]], data[userData["PhoneNumber"]], "ambaaq.png", gender, null, null);
*/
        var appDomain = await _configRepo.FirstOrDefaultAsync(x => x.Key == "AppDomain");
        var userDataValue = await _configRepo.FirstOrDefaultAsync(x => x.Key == "UseSubDomain");
        if (userDataValue == null)
        {
            throw new Exception("App Domain User not set");
        }

        var userDataSett = JsonConvert.DeserializeObject<Dictionary<string, object>>(userDataValue.Value);
        if ((bool)userDataSett["CheckAdditionalData"])
        {
            var checkKey = data[(string)userDataSett["CheckDataKey"]];
            var urls = userDataSett["AdditionalUrl"] as ArrayList;
           // var url = urls.ToArray().FirstOrDefault(x => x[checkKey] );
            var additionalData = await _gateway.GetEntityAsync(request.ExternalId);

        }

        userData.Add("Genotype", "Gen");

        var cardRequest = new CardRequest(request.ExternalId, request.Biometrics.Adapt<BiometricData>(), cardRequestData, userData);

        await _repository.AddAsync(cardRequest, cancellationToken);
        await _repository.SaveChangesAsync();
        return cardRequest.Id;
    }
}