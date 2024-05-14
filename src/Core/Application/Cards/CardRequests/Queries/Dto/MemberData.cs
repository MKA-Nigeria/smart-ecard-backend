using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cards.CardRequests.Queries.Dto;
public class MemberData
{
    public string? EntityId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MiddleName { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PictureUrl { get; set; }
    public Dictionary<string?, string?> CustomData { get; set; } = [];

}
