using Application.Common.Models;
using Application.Configurations.Commands;
using Application.Configurations.Dto;
using Application.Configurations.Queries;
using Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Shared.Authorization;

namespace Host.Controllers.Configurations;
public class AppConfigurationsController : VersionNeutralApiController
{
    [HttpGet]
    //[MustHavePermission(AppAction.View, Resource.AppConfigurations)]
    [OpenApiOperation("Get all application Configurations settings", "")]
    public Task<PaginationResponse<AppConfigurationDto>> GetAppConfigurations()
    {
        return Mediator.Send(new GetAppConfigurationsQuery());
    }

    [HttpPut("update")]
    //[MustHavePermission(AppAction.Update, Resource.AppConfigurations)]
    [OpenApiOperation("Update the provided application Configurations settings", "")]
    public Task<Guid> UpdateAppConfigurations(AppConfigurationDto request)
    {
        return Mediator.Send(new UpdateAppConfigurationsCommand(request));
    }

    [AllowAnonymous]
    [HttpGet("{key}")]
    //[MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Get application Configurations settings by key", "")]
    public Task<AppConfigurationDto> GetAppConfigurationByKey(string key)
    {
        return Mediator.Send(new GetAppConfigurationsByKeyQuery(key));
    }
}
