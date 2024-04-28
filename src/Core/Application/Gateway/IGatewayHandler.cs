using Application.Identity.Tokens;

namespace Application.Gateway;
public interface IGatewayHandler
{
    Task<dynamic> ExternalLoginAsync(TokenRequest request);
    Task<dynamic> GetEntityAsync(string entityId);
    Task<dynamic> GetEntityAsync(string url, string entityId);
}
