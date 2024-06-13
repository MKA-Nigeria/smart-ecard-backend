using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Gateway;
using Domain.Entities;
using Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        try
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
            if (data == null)
            {
                return new BaseResponse<MemberData>
                {
                    Message = "Member with the identity number not found",
                    Status = false
                };
            }

            var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.ExternalEntityData);
            var appDomain = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.AppDomain);

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

            var jsonData = data.ToString();
            var jsonObject = JObject.Parse(jsonData);

            // Flatten the JSON object
            var flattenedDict = FlattenJObject(jsonObject);
            Gender? gender = null;
            if (appDomain.Value != "MKAN")
            {
                gender = (flattenedDict[userData["Gender"].ToString()] == "F") ? Gender.Female : Gender.Male;
            }
            else
            {
                gender = Gender.Male;
            }

            DateTime? date = flattenedDict[userData["DateOfBirth"].ToString()];

            var memberData = new MemberData
            {
                FirstName = (string)flattenedDict[userData["FirstName"].ToString()],
                LastName = (string)flattenedDict[userData["LastName"].ToString()],
                DateOfBirth = date,
                Gender = gender.ToString(),
                Address = (string)flattenedDict[userData["Address"].ToString()],
                Email = (string)flattenedDict[userData["Email"].ToString()],
                PhoneNumber = (string)flattenedDict[userData["PhoneNumber"].ToString()],
                MiddleName = (string)flattenedDict[userData["MiddleName"].ToString()],
                EntityId = request.EntityId
            };
            var coreData = new List<string>() { "FirstName", "LastName", "PhoneNumber", "Gender", "Address", "DateOfBirth", "Email", "MiddleName" };
            foreach (var item in userData)
            {
                var key = item.Key;

                if (!memberData.CustomData.ContainsKey(key) && !coreData.Contains(key))
                {
                    // Retrieve the value from flattenedDict using the key from userData
                    if (flattenedDict.TryGetValue(userData[key].ToString(), out object objectValue))
                    {
                        // Convert objectValue to string if possible
                        string stringValue = objectValue?.ToString();
                        if (stringValue != null)
                        {
                            memberData.CustomData.Add(key, stringValue);
                        }
                    }
                }
            }

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
        catch (Exception ex)
        {
            return new BaseResponse<MemberData>
            {
                Message = """An error occurred! Please try again or contact the admin""",
                Status = true
            };
        }
    }

    /*private string GetMappedValue(Dictionary<string, object> configData, Dictionary<string, string> nestedAttributes, Dictionary<string, object> data, string key)
    {
        if (nestedAttributes != null && nestedAttributes.ContainsKey(key) && data.ContainsKey(nestedAttributes[key]))
        {
            return data[nestedAttributes[key]].ToString();
        }
        else if (configData.ContainsKey(key) && data.ContainsKey(configData[key].ToString()))
        {
            return data[configData[key].ToString()].ToString();
        }
        return string.Empty;
    }*/

    private Dictionary<string, object> FlattenJObject(JObject jsonObject, string prefix = "")
    {
        var dict = new Dictionary<string, object>();

        foreach (var property in jsonObject.Properties())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (property.Value.Type == JTokenType.Object)
            {
                var nestedDict = FlattenJObject((JObject)property.Value, key);
                foreach (var nestedEntry in nestedDict)
                {
                    dict.Add(nestedEntry.Key, nestedEntry.Value);
                }
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                var array = (JArray)property.Value;
                var arrayValues = string.Join(", ", array.Select(item => item.ToString()));
                dict.Add(key, arrayValues);
            }
            else
            {
                dict.Add(key, ((JValue)property.Value).Value);
            }
        }

        return dict;
    }
}


