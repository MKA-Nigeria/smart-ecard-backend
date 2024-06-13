using Application.Gateway;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Text;
using Application.Identity.Tokens;
using Microsoft.Extensions.Logging;
using Application.Common.Persistence;
using Domain.Entities;
using Shared.Configurations;
using System.Net.Http;

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

    public async Task<dynamic> GetCardRecordsAsync()
    {
        var externalCardRecordUrl = await _configRepository.GetByExpressionAsync(x => x.Key == ConfigurationKeys.ExternalCardRecordDataUrl);
        if (externalCardRecordUrl is null || externalCardRecordUrl.Value is null)
        {
            _logger.LogInformation("externalCardRecordUrl not configured");
            throw new InvalidOperationException("externalCardRecordUrl not found");
        }

        var url = $"{externalCardRecordUrl.Value}";
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get
        };
        request.Headers.Add("ApiKey", _config.GetSection("ApiKey").Value);

        try
        {
            var response = await _client.SendAsync(request);
            _logger.LogInformation("Get Entity Data response: {response}", response.Content);
            //response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                // Reading the response as string
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Optionally, check if the jsonResponse is not null or empty
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    _logger.LogWarning("Received empty response ");
                    return null; // or throw an appropriate exception
                }

                // Deserialize the JSON response
                dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = { new ExpandoObjectConverter() }
                });
                return data;
            }
            return null;
        }
        catch (HttpRequestException hre)
        {
            _logger.LogError("HTTP Request failed: {Message}", hre.Message);
            throw new Exception($"HTTP Request failed: {hre.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {Message}", ex.Message);
            throw; // Rethrow the current exception without losing stack trace
        }
    }

    public async Task<dynamic> GetEntityAsync(string entityId)
    {
        var externalEntityUrl = await _configRepository.GetByExpressionAsync(x => x.Key == ConfigurationKeys.ExternalEntityUrl);
        if (externalEntityUrl is null || externalEntityUrl.Value is null)
        {
            _logger.LogInformation("externalEntityUrl not configured");
            throw new InvalidOperationException("externalEntityUrl not found");
        }

        var url = $"{externalEntityUrl.Value}{entityId}";
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get
        };
        request.Headers.Add("ApiKey", _config.GetSection("ApiKey").Value);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiI3OTE3OnRhSm5lZWQiLCJpYXQiOjE3MTgyNTg2OTMsImV4cCI6MTcxODI4MjY5M30.d5gmAdeZyzJaFnZM-H7xfslQFbcQbbzFzp34QAAd-7Z007uFQSIkig0HOV1H2G5Qjg-GlYoXRhY0Jq_S5hFlMg");
        try
        {
            var response = await _client.SendAsync(request);
            _logger.LogInformation("Get Entity Data response: {response}", response.Content);
            //response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                // Reading the response as string
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Optionally, check if the jsonResponse is not null or empty
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    _logger.LogWarning("Received empty response for entityId: {entityId}", entityId);
                    return null; // or throw an appropriate exception
                }

                // Deserialize the JSON response
                dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = { new ExpandoObjectConverter() }
                });
                return data;
            }
            return null;
        }
        catch (HttpRequestException hre)
        {
            _logger.LogError("HTTP Request failed: {Message}", hre.Message);
            throw new Exception($"HTTP Request failed: {hre.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {Message}", ex.Message);
            throw; // Rethrow the current exception without losing stack trace
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
        try
        {
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Reading the response as string
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Optionally, check if the jsonResponse is not null or empty
            if (string.IsNullOrEmpty(jsonResponse))
            {
                _logger.LogWarning("Received empty response for entityId: {entityId}", entityId);
                return null; // or throw an appropriate exception
            }

            // Deserialize the JSON response
            dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new ExpandoObjectConverter() }
            });
            return data;
        }
        catch (HttpRequestException hre)
        {
            _logger.LogError("HTTP Request failed: {Message}", hre.Message);
            throw new Exception($"HTTP Request failed: {hre.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred: {Message}", ex.Message);
            throw; // Rethrow the current exception without losing stack trace
        }
    }
}
