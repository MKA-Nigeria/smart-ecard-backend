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

namespace Host.Controllers.Card.Cards;
public class CardsController : VersionNeutralApiController
{
    [HttpPost("/active/search")]
    [MustHavePermission(AppAction.Search, Resource.Card)]
    [OpenApiOperation("Search active cards or get all active cards", "")]
    public Task<PaginationResponse<CardDto>> SearchActiveAsync(SearchActiveCardsRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpPost("/inactive/search")]
    [MustHavePermission(AppAction.Search, Resource.Card)]
    [OpenApiOperation("Search active cards or get all active cards", "")]
    public Task<PaginationResponse<CardDto>> SearchInActiveAsync(SearchInActiveCardsRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpPut("activate/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Approve card request", "")]
    public Task<Guid> Activate(DefaultIdType id)
    {
        return Mediator.Send(new ActivateCardRequest { CardId = id });
    }

    [HttpPut("deactivate/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Approve card request", "")]
    public Task<Guid> Deactivate(DefaultIdType id)
    {
        return Mediator.Send(new DeactivateCardRequest { CardId = id });
    }

}
