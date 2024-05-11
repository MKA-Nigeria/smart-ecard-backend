using Domain.Common.Contracts;

namespace Domain.Entities;
public class Lookup : AuditableEntity, IAggregateRoot
{
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
    public bool IsDefault { get; set; }

    public Lookup(string type, string name, string value)
    {
        Type = type;
        Name = name;
        Value = value;
        IsDefault = false;
    }

    public Lookup Update(string? value)
    {
        if (value is not null && Value?.Equals(value) is not true) Value = value;
        return this;
    }

    public void ToggleDefaultStatus(bool isDefault)
    {
        // prevent error while changing to same status
        if (IsDefault == isDefault) return;

        IsDefault = isDefault;
    }
}
