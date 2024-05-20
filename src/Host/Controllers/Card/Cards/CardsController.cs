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
public class CardsController : VersionNeutralApiController
{
    [HttpPost("/search")]
    [MustHavePermission(AppAction.Search, Resource.Card)]
    [OpenApiOperation("Search active cards or get all active cards", "")]
    public Task<PaginationResponse<CardDto>> SearchActiveAsync(SearchCardsRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpPut("activate/{cardNumber}")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Activate card", "")]
    public Task<Guid> Activate(string cardNumber)
    {
        return Mediator.Send(new ActivateCardRequest { CardNumber = cardNumber });
    }

    [HttpPut("deactivate/{cardNumber}")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Deactivae card", "")]
    public Task<Guid> Deactivate(string cardNumber)
    {
        return Mediator.Send(new DeactivateCardRequest { CardNumber = cardNumber });
    }

    [HttpGet("{cardNumber}")]
    [MustHavePermission(AppAction.View, Resource.Card)]
    [OpenApiOperation("Get card requests", "")]
    public Task<CardDto> GetAsync(string cardNumber)
    {
        return Mediator.Send(new GetCardRequest { CardNumber = cardNumber });
    }

}
