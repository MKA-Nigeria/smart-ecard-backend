﻿using Application.Card.CardRequests;
using Application.Card.CardRequests.Commands;
using Application.Card.CardRequests.Queries;
using Application.Card.CardRequests.Queries.Dto;
using Application.Common.Models;
using Infrastructure.Auth.Permissions;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Shared.Authorization;

namespace Host.Controllers.Card.CardRequest;
public class CardRequestsController : VersionNeutralApiController
{
    [HttpPost("search")]
    [MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Search card or get all card requests")]
    public Task<PaginationResponse<CardRequestDto>> SearchAsync(SearchCardRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpGet("{id:guid}")]
    [MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Get card requests")]
    public Task<CardRequestDto> GetAsync(DefaultIdType id)
    {
        return Mediator.Send(new GetCardRequest { CardRequestId = id });
    }
    
    [HttpGet("member/{id}")]
    [MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Get member information")]
    public Task<CardRequestDto> GetMemberDataAsync(string id)
    {
        return Mediator.Send(new GetMemberRequest { EntityId = id });
    }

    [HttpPost]
    [MustHavePermission(AppAction.Create, Resource.CardRequest)]
    [OpenApiOperation("Request for new card with unique Id")]
    public Task<Guid> CreateAsync(CreateCardRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpPut("approve/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.CardRequest)]
    [OpenApiOperation("Approve card request")]
    public Task<Guid> ApproveAsync(DefaultIdType id)
    {
        return Mediator.Send(new ApproveCardRequest { CardRequestId = id });
    }

    [HttpPut("reject/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.CardRequest)]
    [OpenApiOperation("Reject card request")]
    public Task<Guid> RejectAsync(DefaultIdType id)
    {
        return Mediator.Send(new RejectCardRequest { CardRequestId = id });
    }

    [HttpPut("cancel/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.CardRequest)]
    [OpenApiOperation("Cancel card request")]
    public Task<Guid> CancelAsync(DefaultIdType id)
    {
        return Mediator.Send(new CancelCardRequest { CardRequestId = id });
    }
}
