using Application.Configurations.Commands;
using Application.Configurations.Dto;
using Application.Configurations.Queries;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Host.Controllers.Configurations;
public class AppConfigurationsController : VersionNeutralApiController
{
    [HttpGet]
    //[MustHavePermission(AppAction.Search, Resource.AppConfigurations)]
    [OpenApiOperation("Get all application Configurations settings", "")]
    public Task<List<AppConfigurationDto>> GetAppConfigurations()
    {
        return Mediator.Send(new GetAppConfigurationsQuery());
    }

    [HttpPut("update")]
    [OpenApiOperation("Update the provided application Configurations settings", "")]
    public Task<Guid> UpdateAppConfigurations(AppConfigurationDto request)
    {
        return Mediator.Send(new UpdateAppConfigurationsCommand(request));
    }

    [HttpGet("{key}")]
    //[MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Get application Configurations settings by key", "")]
    public Task<AppConfigurationDto> GetMemberDataAsync(string key)
    {
        return Mediator.Send(new GetAppConfigurationsByKeyQuery(key));
    }
}
