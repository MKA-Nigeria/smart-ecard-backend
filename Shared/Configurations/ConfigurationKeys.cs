using System.Collections.ObjectModel;

namespace Shared.Configurations;
public static class ConfigurationKeys
{
    public const string AppDomain = nameof(AppDomain);
    public const string ExternalLoginUrl = nameof(ExternalLoginUrl);
    public const string ExternalEntityUrl = nameof(ExternalEntityUrl);
    public const string ExternalEntityData = nameof(ExternalEntityData);
    public const string ExternalEntityAdditionalData = nameof(ExternalEntityAdditionalData);
    public const string ExternalLoginData = nameof(ExternalLoginData);
    public const string CardData = nameof(CardData);

    public static IReadOnlyList<string> DefaultConfigurationKeys { get; } = new ReadOnlyCollection<string>(
    [
        AppDomain,
        ExternalLoginUrl,
        ExternalEntityUrl,
        ExternalEntityData,
        ExternalEntityAdditionalData,
        CardData,
        ExternalLoginData
    ]);

    public static bool IsDefault(string key) => DefaultConfigurationKeys.Any(r => r == key);
}