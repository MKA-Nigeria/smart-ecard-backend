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
using System.Net;
using System.Text.Json.Nodes;

namespace Application.Card.CardRequests.Queries;
public class GetMemberRequest : IRequest<MemberData>
{
    public string EntityId { get; set; } = default!;
}
public class GetMemberRequestHandler(IGatewayHandler gateway, IRepository<AppConfiguration> configRepo) : IRequestHandler<GetMemberRequest, MemberData>
{

    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    private readonly IGatewayHandler _gateway = gateway;
    public async Task<MemberData> Handle(GetMemberRequest request, CancellationToken cancellationToken)
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

        var memberData = new MemberData
        {
            FirstName = (string)data[userData["FirstName"]],
            LastName = (string)data[userData["LastName"]],
            DateOfBirth = date,
            Gender = gender.ToString(),
            Address = (string)data[userData["Address"]],
            Email = (string)data[userData["Email"]],
            PhoneNumber = (string)data[userData["PhoneNumber"]],
            MiddleName = (string)data[userData["MiddleName"]],
            EntityId = request.EntityId
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
            var loginData = JsonConvert.SerializeObject(urlObject);
            var bbb = JsonConvert.DeserializeObject<Dictionary<string, string>>(loginData);
            var url = bbb[(string)checkKey];
            var additionalData = await _gateway.GetEntityAsync(url, request.EntityId);
            var additinalDataModel = userDataSett[(string)checkKey];

            var model = JsonConvert.SerializeObject(additinalDataModel);

            var addData = JsonConvert.DeserializeObject<Dictionary<string, object>>(model);
            var existingData = new List<string>() { "FirstName", "LastName", "PhoneNumber", "Gender", "Address", "DateOfBirth", "Email" };
            foreach (var item in addData)
            {
                if(!memberData.CustomData.ContainsKey(item.Key) && !existingData.Contains(item.Key))
                {
                    memberData.CustomData.Add(item.Key, (string)additionalData[item.Value]);
                }
            }

        }

        return memberData;
    }
}


