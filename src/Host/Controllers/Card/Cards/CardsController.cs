using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Models;
using Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Shared.Authorization;
using Application.Cards.Cards.Queries;
using Application.Cards.Cards.Dto;

namespace Host.Controllers.Card.Cards;
public class CardsController : VersionNeutralApiController
{
    [HttpPost("/active/search")]
    [MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Search active cards or get all active cards", "")]
    public Task<PaginationResponse<CardDto>> SearchAsync(SearchCardsRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

}
