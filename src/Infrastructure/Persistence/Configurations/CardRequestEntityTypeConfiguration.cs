using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System.Collections.Generic;

public class CardRequestConfiguration : IEntityTypeConfiguration<CardRequest>
{
    public void Configure(EntityTypeBuilder<CardRequest> builder)
    {
        // Configuring CardData as a JSON column
        builder.Property(c => c.CardData)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<CardRequestData>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                new ValueComparer<CardRequestData>(
                    (c1, c2) => JsonConvert.SerializeObject(c1) == JsonConvert.SerializeObject(c2),
                    c => c == null ? 0 : JsonConvert.SerializeObject(c).GetHashCode(),
                    c => JsonConvert.DeserializeObject<CardRequestData>(JsonConvert.SerializeObject(c))
                ))
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // Configuring Biometrics as a JSON column
        builder.Property(c => c.Biometrics)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<BiometricData>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                new ValueComparer<BiometricData>(
                    (b1, b2) => JsonConvert.SerializeObject(b1) == JsonConvert.SerializeObject(b2),
                    b => b == null ? 0 : JsonConvert.SerializeObject(b).GetHashCode(),
                    b => JsonConvert.DeserializeObject<BiometricData>(JsonConvert.SerializeObject(b))
                ))
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // Configuration for CustomData
        builder.Property(e => e.CustomData)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IDictionary<string, object>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))
            .HasColumnType("nvarchar(max)");

        // Configure other properties
        builder.Property(c => c.ExternalId).IsRequired();
        builder.Property(c => c.RequestDate);
        builder.Property(c => c.Status).HasConversion<int>(); // Store status as integer if it's an enum
        builder.Property(c => c.ReasonForRejection);
    }
}