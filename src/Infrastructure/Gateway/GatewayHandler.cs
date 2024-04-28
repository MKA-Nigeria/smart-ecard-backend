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

    public async Task EnsureProviderConfigurationLoaded()
    {
        var app = await _configRepository.GetByExpressionAsync(x => x.Key == "AppId");
        if (app is null)
        {
            _logger.LogInformation($"App configuration data not found");
            throw new InvalidOperationException($"App configuration data not found");
        }

        _logger.LogInformation($"Retrieving provider configuration data for {app.Value}");
        var data = await _settingRepository.GetByExpressionAsync(x => x.SettingType == app.Value);
        if (data is null || data.Data is null)
        {
            _logger.LogInformation($"App Settings data not found");
            throw new InvalidOperationException($"App Settings data not found");
        }
        else
        {
            //ProviderData = JsonConvert.DeserializeObject<IntrabankProviderData>(data.Data)!;
            _logger.LogInformation($"Provider configuration data loaded for {app.Value}");
        }
    }

    public async Task<dynamic> ExternalLoginAsync(TokenRequest request)
    {
        var externalLoginUrl = await _configRepository.GetByExpressionAsync(x => x.Key == "ExternalLoginUrl");
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
        //var url = $"{_baseApiPath}users/{entityId}";
        var url = $"{externalEntityUrl.Value}{entityId}";
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
