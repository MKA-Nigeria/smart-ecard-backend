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
using Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;
using Application.Common.Persistence;
using Domain.Entities;

namespace Infrastructure.Gateway;
public class GatewayHandler : IGatewayHandler
{
    private readonly HttpClient _client;
    private readonly string _baseApiPath;
    private readonly IConfigurationSection _config;
    private readonly IRepository<AppConfiguration> _configRepository;
    private readonly IRepository<AppSetting> _settingRepository;
    private readonly ILogger<GatewayHandler> _logger;

    public GatewayHandler(IConfiguration configuration, ILogger<GatewayHandler> logger, IRepository<AppConfiguration> configRepository, IRepository<AppSetting> settingRepository)
    {
        _client = new HttpClient();
        _config = configuration.GetSection("Api");
        //_baseApiPath = _config.GetSection("Url").Value;
        _baseApiPath = "https://tajneedapi.ahmadiyyanigeria.net/";
        _logger = logger;
        _configRepository = configRepository;
        _settingRepository = settingRepository;
    }

    public async Task<dynamic> ExternalLoginAsync(TokenRequest request)
    {
        var externalLoginUrl = await _configRepository.GetByExpressionAsync(x => x.Key == "ExternalLoginUrl");
        if (externalLoginUrl is null || externalLoginUrl.Value is null)
        {
            _logger.LogInformation($"ExternalLoginUrl not found");
            throw new Exception($"ExternalLoginUrl not found");
        }

        var url = externalLoginUrl.Value;
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
        var externalEntityUrl = await _configRepository.GetByExpressionAsync(x => x.Key == "ExternalEntityUrl");
        if (externalEntityUrl is null || externalEntityUrl.Value is null)
        {
            _logger.LogInformation($"externalEntityUrl not found");
            throw new Exception($"externalEntityUrl not found");
        }

        var url = $"{externalEntityUrl.Value}{entityId}";
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get
        };
        request.Headers.Add("ApiKey", _config.GetSection("ApiKey").Value);
        try
        {
            string jsonResponse = await _client.GetStringAsync(url);
            dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new ExpandoObjectConverter() }
            });
            return data;
        }
        catch (Exception ex)
        {

            throw ex;
        }
        
    }

    public async Task<dynamic> GetEntityAsync(string url, string entityId)
    {
        url = $"{url}{entityId}";
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
