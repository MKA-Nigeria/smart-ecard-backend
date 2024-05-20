using Application.Cards.CardRequests.Commands;
using Application.Cards.CardRequests.Queries;
using Application.Cards.CardRequests.Queries.Dto;
using Application.Common.Dtos;
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
    [OpenApiOperation("Search card or get all card requests", "")]
    public Task<PaginationResponse<CardRequestDto>> SearchAsync(SearchCardRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpPost("search2")]
    [MustHavePermission(AppAction.Search, Resource.CardRequest)]
    [OpenApiOperation("Search card or get all card requests", "")]
    public async Task<IActionResult> SearchAsync2(SearchCardRequest cardRequest)
    {
        return Ok(Mediator.Send(cardRequest));
    }


    [HttpGet("{id:guid}")]
    [MustHavePermission(AppAction.View, Resource.CardRequest)]
    [OpenApiOperation("Get card requests", "")]
    public Task<CardRequestDto> GetAsync(DefaultIdType id)
    {
        return Mediator.Send(new GetCardRequest { CardRequestId = id });
    }


    [HttpGet("{entityId}")]
    [MustHavePermission(AppAction.View, Resource.CardRequest)]
    [OpenApiOperation("Get card requests", "")]
    public Task<CardRequestDto> GetAsync(string entityId)
    {
        return Mediator.Send(new GetCardRequestByExternalId { EntityId = entityId });
    }
    
    [HttpGet("member/{id}")]
    [MustHavePermission(AppAction.View, Resource.CardRequest)]
    [OpenApiOperation("Get member information", "")]
    public Task<BaseResponse<MemberData>> GetMemberDataAsync(string id)
    {
        return Mediator.Send(new GetMemberRequest { EntityId = id });
    }

    [HttpPost]
    [MustHavePermission(AppAction.Create, Resource.CardRequest)]
    [OpenApiOperation("Request for new card with unique Id", "")]
    public Task<Guid> CreateAsync(CreateCardRequest cardRequest)
    {
        return Mediator.Send(cardRequest);
    }

    [HttpPut("approve/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.CardRequest)]
    [OpenApiOperation("Approve card request", "")]
    public Task<Guid> ApproveAsync(DefaultIdType id)
    {
        return Mediator.Send(new ApproveCardRequest { CardRequestId = id });
    }

    [HttpPut("reject/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.CardRequest)]
    [OpenApiOperation("Reject card request", "")]
    public Task<Guid> RejectAsync(DefaultIdType id)
    {
        return Mediator.Send(new RejectCardRequest { CardRequestId = id });
    }

    [HttpPut("cancel/{id:guid}")]
    [MustHavePermission(AppAction.Update, Resource.CardRequest)]
    [OpenApiOperation("Cancel card request", "")]
    public Task<Guid> CancelAsync(DefaultIdType id)
    {
        return Mediator.Send(new CancelCardRequest { CardRequestId = id });
    }

    //[HttpPost("image")]
    //[OpenApiOperation("Upload Image for a card request.", "")]
    //public async Task<IActionResult> UploadImage(FileUploadRequest imageRequest)
    //{
    //    string imageName = await _fileStorage.UploadAsync<string>(imageRequest, FileType.Image);
    //    var stream = await StorageHelper.RetrieveImage(imageName);
    //    return File(stream, "image/jpeg");
    //}
}
