using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Cards.Cards.Dto;
public class CardDto
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public PrintStatus PrintStatus { get; set; }
    public bool IsCollected { get; set; }
    public string Name { get; set; }
    public DateTime ApprovedDate { get; set; }
}
