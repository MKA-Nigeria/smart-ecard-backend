using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cards.CardRequests.Queries.Dto;
public class CardRequestDto
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; }
    public CardRequestStatus Status { get; set; }
    public MemberData MemberData { get; set; }
}
