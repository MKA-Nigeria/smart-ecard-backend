using Domain.Common.Contracts;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Cards;
public class CardRequest : AuditableEntity, IAggregateRoot
{
    // Unique identification number of card request owner
    public string ExternalId { get; private set; } = default!;
    public DateTime RequestDate { get; private set; }
    public CardRequestStatus Status { get; private set; }
    public string? ReasonForRejection { get; private set; }
    public CardRequestData CardData { get; private set; }
    // other custom data
    public IDictionary<string, object> CustomData { get; private set; }

    // Biometric data could be a complex type if more detail is needed
    public BiometricData? Biometrics { get; private set; }

    private CardRequest() { }

    public CardRequest(Guid requestId, string externalId, BiometricData? biometrics, CardRequestData cardRequestData, IDictionary<string, object> customData)
    {
        Id = requestId;
        ExternalId = externalId;
        RequestDate = DateTime.UtcNow;
        Status = CardRequestStatus.Pending;
        Biometrics = biometrics;
        CardData = cardRequestData;
        CustomData = customData;
    }

    public void Approve()
    {
        if (Status != CardRequestStatus.Pending)
            throw new CardRequestException("Request is not in a pending state.");

        Status = CardRequestStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != CardRequestStatus.Pending)
            throw new CardRequestException("Request is not in a pending state.");

        Status = CardRequestStatus.Rejected;
        ReasonForRejection = reason;
    }

    public void Cancel()
    {
        if (Status != CardRequestStatus.Pending)
            throw new CardRequestException("Request is not in a pending state.");

        Status = CardRequestStatus.Cancelled;
    }
}
