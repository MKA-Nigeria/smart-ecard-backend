using Domain.Common.Contracts;

namespace Domain.Entities;
public class AppSetting(string settingType, string data) : AuditableEntity, IAggregateRoot
{
    public string SettingType { get; set; } = settingType;
    public string Data { get; set; } = data;

    public AppSetting Update(string? settingType, string? data)
    {
        if (settingType is not null && SettingType?.Equals(settingType) is not true) SettingType = settingType;
        if (data is not null && Data?.Equals(data) is not true) Data = data;
        return this;
    }
}
