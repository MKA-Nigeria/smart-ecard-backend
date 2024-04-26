using Shared.Authorization;

namespace System.Security.Claims;
public static class ClaimsPrincipalExtensions
{
    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.Email);

    public static string? GetUserName(this ClaimsPrincipal principal)
        => principal?.FindFirst(ClaimTypes.Name)?.Value;

    public static string? GetUserId(this ClaimsPrincipal principal)
       => principal.FindFirstValue(ClaimTypes.NameIdentifier);

    public static DateTimeOffset GetExpiration(this ClaimsPrincipal principal) =>
        DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(
            principal.FindFirstValue(ClaimConstants.Expiration)));

    private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType) =>
        principal is null
            ? throw new ArgumentNullException(nameof(principal))
            : principal.FindFirst(claimType)?.Value;
}