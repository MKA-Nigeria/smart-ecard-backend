using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Models;
using Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Shared.Authorization;
using Application.Cards.Cards.Queries;
using Application.Cards.Cards.Dto;
using Application.Cards.CardRequests.Commands;
using Application.Cards.Cards.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Host.Controllers.Card.Cards;
public class CardMigrationsController : VersionNeutralApiController
{
    [AllowAnonymous]
    [HttpPost]
    //[MustHavePermission(AppAction.Search, Resource.Card)]
    [OpenApiOperation("Migrate existing cards", "")]
    public Task<string> MigrateCardsAsync(MigrateCardsRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }
}
