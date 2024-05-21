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
    public  Card Card { get; private set; }
    // other custom data
    public virtual IDictionary<string, string> CustomData { get; private set; }

    // Biometric data could be a complex type if more detail is needed
    public BiometricData? Biometrics { get; private set; }

    private CardRequest() { }

    public CardRequest(string externalId, BiometricData? biometrics, CardRequestData cardRequestData, IDictionary<string, string> customData, CardRequestStatus status = CardRequestStatus.Pending)
    {
        ExternalId = externalId;
        RequestDate = DateTime.UtcNow;
        Status = status;
        Biometrics = biometrics;
        CardData = cardRequestData;
        CustomData = customData;
    }

    public void Approve(Guid userId)
    {
        if (Status != CardRequestStatus.Pending)
            throw new CardRequestException("Request is not in a pending state.");

        Status = CardRequestStatus.Approved;
        LastModifiedBy = userId;
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
