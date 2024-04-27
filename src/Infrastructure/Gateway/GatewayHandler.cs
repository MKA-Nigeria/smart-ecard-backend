using Application.Gateway;
using DocumentFormat.OpenXml.Spreadsheet;
using Infrastructure.Gateway.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Dynamic;
using Application.Identity.Tokens;

namespace Infrastructure.Gateway;
public class GatewayHandler : IGatewayHandler
{
    private readonly HttpClient _client;
    private readonly string _baseApiPath;
    private readonly IConfigurationSection _config;

    public GatewayHandler(IConfiguration configuration)
    {
        _client = new HttpClient();
        _config = configuration.GetSection("Api");
        //_baseApiPath = _config.GetSection("Url").Value;
        _baseApiPath = "https://tajneedapi.ahmadiyyanigeria.net/";
    }

    public async Task<dynamic> ExternalLoginAsync(TokenRequest request)
    {
        var url = $"{_baseApiPath}{"token"}";
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage();
        requestMessage.RequestUri = new Uri(url);
        requestMessage.Method = HttpMethod.Post;
        requestMessage.Headers.Add("ApiKey", _config.GetSection("ApiKey").Value);
        requestMessage.Content = content;

        var jsonResponse = await _client.SendAsync(requestMessage);

        if (!jsonResponse.IsSuccessStatusCode)
        {
            return null;
        }

        return await jsonResponse.Content.ReadAsStringAsync();
    }

    public async Task<dynamic> GetEntityAsync(string entityId)
    {
        var url = $"{_baseApiPath}users/{entityId}";
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get
        };
        request.Headers.Add("ApiKey", _config.GetSection("ApiKey").Value);
        string jsonResponse = await _client.GetStringAsync(url);
        dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = { new ExpandoObjectConverter() }
        });
        return data;
    }
}
