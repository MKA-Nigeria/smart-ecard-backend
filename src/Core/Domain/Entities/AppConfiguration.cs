using Domain.Common.Contracts;

namespace Domain.Entities;
public class AppConfiguration : AuditableEntity, IAggregateRoot
{
    public string Key { get; set; }
    public string Value { get; set; }

    public AppConfiguration(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public AppConfiguration Update(string? key, string? value)
    {
        if (key is not null && Key?.Equals(key) is not true) Key = key;
        if (value is not null && Value?.Equals(value) is not true) Value = value;
        return this;
    }
}
