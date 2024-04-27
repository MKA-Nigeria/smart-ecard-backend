namespace Application.Gateway;
public interface IGatewayHandler
{
    Task<dynamic> ExternalLoginAsync(string username, string password);
    Task<dynamic> GetEntityAsync(string entityId);
}
