using Application.Common.Models;
using Application.Lookups.Commands;
using Application.Lookups.Dtos;
using Application.Lookups.Queries;
using Application.Lookups.Validators;
using Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Shared.Authorization;

namespace Host.Controllers.Lookups;
public class LookupsController : VersionNeutralApiController
{
    [HttpPost]
    //[MustHavePermission(AppAction.Create, Resource.Lookups)]
    [OpenApiOperation("Create a new new lookup with unique Id", "")]
    public Task<Guid> CreateAsync(CreateLookupCommand request)
    {
        return Mediator.Send(request);
    }

    [HttpGet]
    //[MustHavePermission(AppAction.View, Resource.Lookups)]
    [OpenApiOperation("Get all application lookups", "")]
    public Task<PaginationResponse<LookupDto>> GetAppConfigurations()
    {
        return Mediator.Send(new GetLookupsQuery());
    }

    [HttpPut("update")]
    //[MustHavePermission(AppAction.Update, Resource.Lookups)]
    [OpenApiOperation("Update the the reqest specified lookup value", "")]
    public Task<DefaultIdType> UpdateAppConfigurations(UpdateLookupCommand request)
    {
        return Mediator.Send(request);
    }

    [HttpPut("toggle-default-status")]
    // [MustHavePermission(AppAction.Update, Resource.Lookups)]
    [OpenApiOperation("Toggle lookup default status", "")]
    public Task<DefaultIdType> UpdateAppConfigurations(ToggleDefaultStatusCommand request)
    {
        return Mediator.Send(request);
    }

    [HttpGet("{type}")]
    //[MustHavePermission(AppAction.Search, Resource.Lookups)]
    [OpenApiOperation("Get application Configurations settings by key", "")]
    public Task<List<LookupDto>> GetByKey(string type)
    {
        return Mediator.Send(new GetLookupsByTypeQuery(type));
    }
}
