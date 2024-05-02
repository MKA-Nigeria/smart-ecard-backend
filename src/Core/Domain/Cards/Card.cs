using Domain.Common.Contracts;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Cards;
public class Card : AuditableEntity, IAggregateRoot
{
    public string CardNumber { get; private set; }
    public Guid CardRequestId { get; private set; }
    public CardRequest CardRequest { get; private set; }
    public CardStatus Status { get; private set; }
    public PrintStatus PrintStatus { get; private set; }
    public bool IsCollected { get; set; }
    private Card()
    {
        
    }
    public Card(string cardNumber, Guid cardRequestId, CardRequest cardRequest)
    {
        CardNumber = cardNumber;
        CardRequestId = cardRequestId;
        CardRequest = cardRequest;
        Status = CardStatus.Active;
        PrintStatus = PrintStatus.ReadyForPrint;
        IsCollected = false;
    }

    public void ChangeCardStatus(CardStatus status)
    {
        Status = status;
    }

    public void ChangePrintStatus(PrintStatus status)
    {
        PrintStatus = status;
    }

    public void SetISCollected(bool isCollected)
    {
        IsCollected = isCollected;
    }

}
