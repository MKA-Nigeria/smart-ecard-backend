using Application.Card.CardRequests.Commands;
using Application.Card.CardRequests.Queries.Dto;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Gateway;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text.Json.Nodes;

namespace Application.Card.CardRequests.Queries;
public class GetMemberRequest : IRequest<CardRequestDto>
{
    public string EntityId { get; set; } = default!;
}
public class GetMemberRequestHandler(IGatewayHandler gateway, IRepository<AppConfiguration> configRepo) : IRequestHandler<GetMemberRequest, CardRequestDto>
{

    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    private readonly IGatewayHandler _gateway = gateway;
    public async Task<CardRequestDto> Handle(GetMemberRequest request, CancellationToken cancellationToken)
    {
        var data = await _gateway.GetEntityAsync(request.EntityId);
        _ = data ?? throw new NotFoundException("Member not found");

        var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == "UserData");

        if (dataModel == null || dataModel.Value == null)
        {
            throw new Exception("User Data not provided");
        }

        // Deserialize the JSON string into a dictionary
        Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataModel.Value);

        var gender = (data[userData["Gender"]] == "M") ? Gender.Male : Gender.Female;
        DateTime date = data[userData["DateOfBirth"]];
        var memberData = new
        {
            FirstName = data[userData["FirstName"]],
            LastName = data[userData["LastName"]],
            DateOfBirth = date,
            Gender = gender,
            Address = data[userData["Address"]],
            Email = data[userData["Email"]],
            PhoneNumber = data[userData["PhoneNumber"]]
        };

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
            var urlObject = userDataSett["AdditionalUrl"];
            var urlObject1 = userDataSett["AdditionalUrl"] as JObject;
            var urlObject2 = userDataSett["AdditionalUrl"] as JsonObject;
            var urls = userDataSett["AdditionalUrl"] as ArrayList;
            //var url = urls.ToArray().FirstOrDefault(x => x[checkKey] );
            var additionalData = await _gateway.GetEntityAsync(request.EntityId);

        }
        return null;
    }
}


