using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Exceptions;
using Application.Identity.Tokens;
using Infrastructure.Auth;
using Infrastructure.Auth.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Application.Gateway;
using System.Text.Json;
using Newtonsoft.Json;
using Shared.Authorization;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Infrastructure.Identity;
internal class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SecuritySettings _securitySettings;
    private readonly JwtSettings _jwtSettings;
    private readonly IGatewayHandler _gatewayHandler;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        IGatewayHandler gatewayHandler)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _gatewayHandler = gatewayHandler;
    }

    public async Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName.Trim().Normalize());
        if (user is null)
        {
            var json = await _gatewayHandler.GetEntityAsync(request.UserName.Trim().Normalize());
            string jsonString = JsonConvert.SerializeObject(json);
            UserData userData;
            try
            {
                userData = JsonConvert.DeserializeObject<UserData>(jsonString);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                // Handle or rethrow the exception as appropriate
                throw;
            }

            if (userData is not null)
            {
                user.Email = userData.Email;
                user.FirstName = userData.FirstName;
                user.LastName = userData.LastName;
                user.UserName = userData.Username;
                user.PhoneNumber = userData.PhoneNumber;
                user.IsActive = true;
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    throw new InternalServerException("Validation Error Occured", errors: result.Errors.Select(x => x.Description).ToList());
                }

                await _userManager.AddToRoleAsync(user, Roles.Basic);

            }
            else {
                throw new UnauthorizedException("Authentication Failed.");
            }
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User Not Active. Please contact the administrator.");
        }

        if (_securitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
        {
            throw new UnauthorizedException("E-Mail not confirmed.");
        }
        return await GenerateTokensAndUpdateUser(user);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
        string? userEmail = userPrincipal.GetEmail();
        var user = await _userManager.FindByEmailAsync(userEmail!);
        if (user is null)
        {
            throw new UnauthorizedException("Authentication Failed.");
        }

        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid Refresh Token.");
        }

        return await GenerateTokensAndUpdateUser(user);
    }

    private async Task<TokenResponse> GenerateTokensAndUpdateUser(ApplicationUser user)
    {
        string token = GenerateJwt(user);

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        await _userManager.UpdateAsync(user);

        return new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
    }

    private string GenerateJwt(ApplicationUser user) =>
        GenerateEncryptedToken(GetSigningCredentials(), GetClaims(user));

    private IEnumerable<Claim> GetClaims(ApplicationUser user) =>
        new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName)
        };

    private static string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
           signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = false
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UnauthorizedException("Invalid Token.");
        }

        return principal;
    }

    private SigningCredentials GetSigningCredentials()
    {
        byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }
}

public class UserData
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string Fullname { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}