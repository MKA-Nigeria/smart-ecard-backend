using Application.Cards.CardRequests.Queries.Dto;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cards.Cards.Dto;
public class CardDto
{
    //public Guid Id { get; set; }
    public string ExternalId { get; set; }
    public string CardNumber { get; set; }
    public PrintStatus PrintStatus { get; set; }
    public CardStatus CardStatus { get; set; }
    public bool IsCollected { get; set; }
    public DateTime DateCollected { get; set; }
    public string FullName { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public MemberData MemberData { get; set; }
}
