using Domain.Common.Contracts;

namespace Domain.Entities;
public class AppConfiguration(string key, string value) : AuditableEntity, IAggregateRoot
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;

    public AppConfiguration Update(string? key, string? value)
    {
        if (key is not null && Key?.Equals(key) is not true) Key = key;
        if (value is not null && Value?.Equals(value) is not true) Value = value;
        return this;
    }
}
