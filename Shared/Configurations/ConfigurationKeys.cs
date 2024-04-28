using System.Collections.ObjectModel;

namespace Shared.Configurations;
public static class ConfigurationKeys
{
    public const string AppDomain = nameof(AppDomain);
    public const string ExternalLoginUrl = nameof(ExternalLoginUrl);
    public const string ExternalEntityUrl = nameof(ExternalEntityUrl);

    public static IReadOnlyList<string> DefaultConfigurationKeys { get; } = new ReadOnlyCollection<string>(new[]
    {
        AppDomain,
        ExternalLoginUrl,
        ExternalEntityUrl
    });

    public static bool IsDefault(string key) => DefaultConfigurationKeys.Any(r => r == key);
}