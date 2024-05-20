using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Gateway;
using Domain.Entities;
using Domain.Enums;
using Newtonsoft.Json;
using Shared.Configurations;
using System;

namespace Application.Cards.CardRequests.Queries;
public class GetMemberRequest : IRequest<BaseResponse<MemberData>>
{
    public string EntityId { get; set; } = default!;
}
public class GetMemberRequestHandler(IGatewayHandler gateway, IRepository<AppConfiguration> configRepo, IRepository<CardRequest> cardRequestRepo) : IRequestHandler<GetMemberRequest, BaseResponse<MemberData>>
{

    private readonly IRepository<AppConfiguration> _configRepo = configRepo;
    private readonly IRepository<CardRequest> _cardRequestRepo = cardRequestRepo;
    private readonly IGatewayHandler _gateway = gateway;
    public async Task<BaseResponse<MemberData>> Handle(GetMemberRequest request, CancellationToken cancellationToken)
    {
        var cardRequest = await _cardRequestRepo.FirstOrDefaultAsync(x => x.ExternalId.Equals(request.EntityId) && !(x.Status != CardRequestStatus.Rejected || x.Status != CardRequestStatus.Cancelled));
        if (cardRequest is not null)
        {
            return new BaseResponse<MemberData>
            {
                Message = "Card request already exist for this member",
                Status = false
            };
        }

        var data = await _gateway.GetEntityAsync(request.EntityId);
        if(data == null)
        {
            return new BaseResponse<MemberData>
            {
                Message = "Member with the identity number not found",
                Status = false
            };
        }

        var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.ExternalEntityData);

        if (dataModel == null || dataModel.Value == null)
        {
            return new BaseResponse<MemberData>
            {
                Message = "User Data not provided",
                Status = false
            };
        }

        // Deserialize the JSON string into a dictionary
        Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataModel.Value);

        var gender = (data[userData["Gender"]] == "M") ? Gender.Male : Gender.Female;
        DateTime? date = data[userData["DateOfBirth"]];

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

        var userDataValue = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.ExternalEntityAdditionalData);
        if (userDataValue == null)
        {
            return new BaseResponse<MemberData>
            {
                Message = "Additional data information not configured",
                Status = false
            };
        }

        var userDataSett = JsonConvert.DeserializeObject<Dictionary<string, object>>(userDataValue.Value);
        if ((bool)userDataSett["CheckAdditionalData"])
        {
            var checkKey = data[(string)userDataSett["CheckDataKey"]];
            var urlObject = userDataSett["AdditionalUrl"];
            var objectData = JsonConvert.SerializeObject(urlObject);
            var dataResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(objectData);
            //var url = dataResponse[(string)checkKey];
            if (!dataResponse.TryGetValue((string)checkKey, out string urlObj) || urlObj == null)
            {
                return new BaseResponse<MemberData>
                {
                    Message = $"URL data is missing or null for key {checkKey}",
                    Status = false
                };
            }

            string url = urlObj;

            // Check if the URL is in a valid format
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return new BaseResponse<MemberData>
                {
                    Message = $"The URL retrieved for key {checkKey} is not valid: {url}",
                    Status = false
                };
            }
            var additionalData = await _gateway.GetEntityAsync(url, request.EntityId);
            var additinalDataModel = userDataSett[(string)checkKey];

            var model = JsonConvert.SerializeObject(additinalDataModel);

            var addData = JsonConvert.DeserializeObject<Dictionary<string, object>>(model);
            var existingData = new List<string>() { "FirstName", "LastName", "PhoneNumber", "Gender", "Address", "DateOfBirth", "Email" };
            foreach (var item in addData)
            {
                if (!memberData.CustomData.ContainsKey(item.Key) && !existingData.Contains(item.Key))
                {
                    memberData.CustomData.Add(item.Key, (string)additionalData[item.Value]);
                }
            }

        }

        return new BaseResponse<MemberData>
        {
            Message = "Member data retrieved",
            Status = true,
            Data = memberData
        };
    }
}


