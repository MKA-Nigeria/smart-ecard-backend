using Domain.Common.Contracts;

namespace Application.Lookups.Dtos;
public class LookupDto
{
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}
