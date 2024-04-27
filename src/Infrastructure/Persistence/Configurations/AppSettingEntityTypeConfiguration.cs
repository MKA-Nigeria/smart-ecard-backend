using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
                // Configuration for CustomData
        builder.Property(e => e.Data)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IDictionary<string, object>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))
            .HasColumnType("nvarchar(max)");
    }
}