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

    public async Task<dynamic> ExternalLoginAsync(string username, string password)
    {
        var request = new {UserName = username, Password = password};
        var url = $"{_baseApiPath}{"token"}";
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage();
        requestMessage.RequestUri = new Uri(url);
        requestMessage.Method = HttpMethod.Post;
        requestMessage.Headers.Add("ApiKey", _config.GetSection("ApiKey").Value);
        requestMessage.Content = content;

        var response = await _client.SendAsync(requestMessage);
        if (response.StatusCode.Equals(HttpStatusCode.OK))
        {
            return await response.ReadContentAs<dynamic>();
        }
        else if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
        {
            return await response.ReadContentAs<dynamic>();
        }

        throw new Exception(response.StatusCode.ToString());
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

        //var response = await _client.SendAsync(request);
        //if (response.StatusCode.Equals(HttpStatusCode.OK))
        //{
        //    return await response.ReadContentAs<dynamic>();
        //}
        //else if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        //{
        //    return await response.ReadContentAs<dynamic>();
        //}
        //throw new Exception(response.StatusCode.ToString());
    }
}
