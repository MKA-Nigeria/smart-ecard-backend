using Application.Cards.Dashboard.Queries;
using Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Shared.Authorization;

namespace Host.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DashboardController : VersionNeutralApiController
{
    [HttpGet]
    [MustHavePermission(AppAction.View, Resource.Dashboard)]
    [OpenApiOperation("Dashboard", "")]
    public Task<DashboardData> GetAsync()
    {
        return Mediator.Send(new GetDashboardData());
    }
}
