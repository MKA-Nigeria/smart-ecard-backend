using Domain.Common.Contracts;

namespace Domain.Entities;
public class AppSetting : AuditableEntity, IAggregateRoot
{
    public string SettingType { get; set; }
    public IDictionary<string, object> Data { get; private set; }

    private AppSetting() { }
    public AppSetting(string settingType, IDictionary<string, object> data)
    {
        SettingType = settingType;
        Data = data;
    }

    public AppSetting Update(string? settingType, IDictionary<string, object>? data)
    {
        if (settingType is not null && SettingType?.Equals(settingType) is not true) SettingType = settingType;
        if (data is not null && Data?.Equals(data) is not true) Data = data;
        return this;
    }
}
