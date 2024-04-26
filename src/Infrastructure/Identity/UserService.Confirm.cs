using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Application.Common.Exceptions;
using Infrastructure.Common;

namespace Infrastructure.Identity;
internal partial class UserService
{
    private async Task<string> GetEmailVerificationUriAsync(ApplicationUser user, string origin)
    {

        string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        const string route = "api/users/confirm-email/";
        var endpointUri = new Uri(string.Concat($"{origin}/", route));
        string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), QueryStringKeys.UserId, user.Id);
        verificationUri = QueryHelpers.AddQueryString(verificationUri, QueryStringKeys.Code, code);
        return verificationUri;
    }

    public async Task<string> ConfirmEmailAsync(string userId, string code, string tenant, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.EmailConfirmed, cancellationToken);

        _ = user ?? throw new InternalServerException("An error occurred while confirming E-Mail.");

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);

        return result.Succeeded
            ? ($"Account Confirmed for E-Mail {user.Email}")
            : throw new InternalServerException($"An error occurred while confirming {user.Email}");
    }
}
