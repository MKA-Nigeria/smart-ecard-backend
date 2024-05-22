using Application.Cards.CardRequests.Queries.Dto;
using Application.Cards.Cards.Dto;
using Application.Common.Dtos;
using Application.Common.Interfaces;
using Application.Common.Persistence;
using Application.Gateway;
using Domain.Cards;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using Newtonsoft.Json;
using Shared.Configurations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Cards.Cards.Queries;

public class MigrateCardsRequest : IRequest<string>
{

}

public class MigrateCardRequestHandler(IGatewayHandler _gateway, IRepository<Card> _cardRepository, IRepository<CardRequest> _cardRequestRepository, IRepository<AppConfiguration> _configRepo, ICurrentUser _currentUser) : IRequestHandler<MigrateCardsRequest, string>
{

    public async Task<string> Handle(MigrateCardsRequest request, CancellationToken cancellationToken)
    {
        var data = await _gateway.GetCardRecordsAsync();
        if (data == null)
        {
            return "No cards record";
        }

        var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.ExternalCardRecordData);

        if (dataModel == null || dataModel.Value == null)
        {
            return "External Card Record Data Model not Provided";
        }

        // Deserialize the JSON string into a dictionary
        Dictionary<string, object> cardRecords = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataModel.Value);

        foreach (var item in data)
        {
            var gender = (item[cardRecords["Gender"]] == "M") ? Gender.Male : Gender.Female;
            DateTime? date = item[cardRecords["DateOfBirth"]];

            var memberData = new MemberData
            {
                FirstName = (string)item[cardRecords["FirstName"]],
                LastName = (string)item[cardRecords["LastName"]],
                DateOfBirth = date,
                Gender = gender.ToString(),
                Address = (string)item[cardRecords["Address"]],
                Email = (string)item[cardRecords["Email"]],
                PhoneNumber = (string)item[cardRecords["PhoneNumber"]],
                MiddleName = (string)item[cardRecords["MiddleName"]],
                EntityId = (string)item[cardRecords["ExternalId"]],
                PhotoUrl = (string)item[cardRecords["Photo"]]
            };

            var existingData = new List<string>() { "FirstName", "LastName", "PhoneNumber", "Gender", "Address", "DateOfBirth", "Email", "ExternalId", "Photo" };

            foreach (var rec in cardRecords)
            {
                if (!memberData.CustomData.ContainsKey(rec.Key) && !existingData.Contains(rec.Key))
                {
                    memberData.CustomData.Add(rec.Key, (string)item[rec.Value.ToString()]);
                }
            }

            var cardRequestData = memberData.Adapt<CardRequestData>();

            var cardRequest = new CardRequest(memberData.EntityId, null, cardRequestData, memberData.CustomData, CardRequestStatus.Approved);

            await _cardRequestRepository.AddAsync(cardRequest, cancellationToken);

            var userId = _currentUser.GetUserId();
            bool isPrinted = item[cardRecords["IsPrinted"]] != null && (bool)item[cardRecords["IsPrinted"]];
            bool isCollected = item[cardRecords["IsCollected"]] != null && (bool)item[cardRecords["IsCollected"]];
            await CreateCard(cardRequest, userId, isPrinted, isCollected);
        }
        await _cardRequestRepository.SaveChangesAsync(cancellationToken);
        return "Card Succesfully migrated";
    }

    public async Task CreateCard(CardRequest cardRequest, Guid createdBy, bool isPrinted, bool isCollected)
    {
        var dataModel = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.CardData);

        if (dataModel == null || dataModel.Value == null)
        {
            // TODO add logger
            throw new Exception($"No configuration data found for key {ConfigurationKeys.CardData}");
        }

        // Deserialize the JSON string into a dictionary
        Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataModel.Value);
        string orgName = (string)userData["Organisation"];
        string lengthAsString = userData["Length"].ToString();
        int length = int.Parse(lengthAsString);
        string uniqueKey = Guid.NewGuid().ToString().Substring(0, length);
        string cardNumber = $"{orgName}{cardRequest.ExternalId}{uniqueKey}";
        var card = new Card(cardNumber, cardRequest.Id, cardRequest, createdBy);
        if (isCollected)
        {
            card.Collect(true);
        }

        if (isPrinted)
        {
            card.ChangePrintStatus(PrintStatus.Printed);
        }

        await _cardRepository.AddAsync(card);
    }
}