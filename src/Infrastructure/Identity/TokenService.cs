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
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Charts;
using Shared.Helper;
using Infrastructure.Auditing;
using Application.Common.Persistence;
using Domain.Entities;
using Shared.Configurations;

namespace Infrastructure.Identity;
internal class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<AppConfiguration> _configRepo;
    private readonly SecuritySettings _securitySettings;
    private readonly JwtSettings _jwtSettings;
    private readonly IGatewayHandler _gatewayHandler;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SecuritySettings> securitySettings,
        IGatewayHandler gatewayHandler,
        IRepository<AppConfiguration> configRepo)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _securitySettings = securitySettings.Value;
        _gatewayHandler = gatewayHandler;
        _configRepo = configRepo;
    }

    public async Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName.Trim().Normalize());
        if (user is not null)
        {
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                // check external authentication
                var loginJsonString = await _gatewayHandler.ExternalLoginAsync(request);
                LoginResponse loginData;
                try
                {
                    loginData = JsonConvert.DeserializeObject<dynamic>(loginJsonString);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    throw new InvalidCastException($"JSON parsing error: {ex.Message}");
                }

                if (loginData != null)
                {
                    string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, code!, request.Password!);
                }
                else
                {
                    throw new UnauthorizedException("Authentication Failed.");
                }
            }
        }
        else
        {
            dynamic userData;
            try
            {
                userData = await _gatewayHandler.GetEntityAsync(request.UserName.Trim().Normalize());
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new InvalidCastException($"JSON parsing error: {ex.Message}");
            }

            if (userData is not null)
            {
                // check external authentication
                var loginJsonString = await _gatewayHandler.ExternalLoginAsync(request) ?? throw new UnauthorizedException("Invalid credentials");
                dynamic loginData;
                try
                {
                    loginData = JsonConvert.DeserializeObject<dynamic>(loginJsonString);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    throw new UnauthorizedException("Invalid credentials");
                }

                var userModelData = await _configRepo.FirstOrDefaultAsync(x => x.Key == ConfigurationKeys.ExternalEntityData, cancellationToken);

                if(userModelData == null || userModelData.Value == null)
                {
                    throw new Exception($"Login Model Data not provided");
                }


                // Deserialize the JSON string into a dictionary
                Dictionary<string, string> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(userModelData.Value);

                if (loginData != null)
                {
                    try
                    {
                        var newUser = new ApplicationUser
                        {
                            Email = userData[keyValuePairs["Email"]],
                            FirstName = userData[keyValuePairs["FirstName"]],
                            LastName = userData[keyValuePairs["LastName"]],
                            UserName = request.UserName,
                            PhoneNumber = userData[keyValuePairs["PhoneNumber"]],
                            IsActive = true,
                            EmailConfirmed = true
                        };

                        var result = await _userManager.CreateAsync(newUser, request.Password);
                        if (!result.Succeeded)
                        {
                            throw new InternalServerException("Validation Error Occured", errors: result.Errors.Select(x => x.Description).ToList());
                        }

                        await _userManager.AddToRoleAsync(newUser, Roles.Basic);
                        user = newUser;
                    }
                    catch (KeyNotFoundException ex)
                    {
                        // Handle the exception when a key is missing
                        throw new KeyNotFoundException($"Error creating user: Missing key - {ex.Message}");
                    }
                }
                else
                {
                    throw new UnauthorizedException("Authentication Failed.");
                }
            }
            else
            {
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

public class LoginResponse
{
    public LoginData Data { get; set; }
    public string Token { get; set; }
    public string Expiry { get; set; }
    public string Message { get; set; }
    public bool Status { get; set; }
}

public class LoginData
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
}