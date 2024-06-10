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
using System.Drawing.Imaging;
using System.Drawing;
using QRCoder;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using SkiaSharp;
using static QRCoder.QRCodeGenerator;

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

    [HttpPut("{cardNumber}/print")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Print card", "")]
    public Task<Guid> PrintCard(string cardNumber)
    {
        return Mediator.Send(new SetCardPrintedRequest { CardNumber = cardNumber });
    }

    [HttpPut("{cardNumber}/collect")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Collect card", "")]
    public Task<Guid> CollectCard(string cardNumber)
    {
        return Mediator.Send(new SetCardCollectedRequest { CardNumber = cardNumber });
    }

    [HttpPut("deactivate/{cardNumber}")]
    [MustHavePermission(AppAction.Update, Resource.Card)]
    [OpenApiOperation("Deactivae card", "")]
    public Task<Guid> Deactivate(string cardNumber)
    {
        return Mediator.Send(new DeactivateCardRequest { CardNumber = cardNumber });
    }

    [AllowAnonymous]
    [HttpGet("{cardNumber}")]
   // [MustHavePermission(AppAction.View, Resource.Card)]
    [OpenApiOperation("Get card requests", "")]
    public async Task<CardDto> GetAsync(string cardNumber)
    {
        var response = await Mediator.Send(new GetCardRequest { CardNumber = cardNumber });
        //response.QrCode = GenerateQRCode(response.CardNumber);
        return response;
    }

    /*public async Task<string> GenerateQRCode(string qRCodeText)
    {
        if (!string.IsNullOrEmpty(qRCodeText))
        {
            using MemoryStream ms = new();
            QRCodeGenerator qrCodeGenerate = new();
            QRCodeData qrCodeData = qrCodeGenerate.CreateQrCode(qRCodeText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new(qrCodeData);
            using Bitmap qrBitMap = qrCode.GetGraphic(20);
            qrBitMap.Save(ms, ImageFormat.Png);
            string base64 = Convert.ToBase64String(ms.ToArray());
            return string.Format("data:image/png;base64,{0}", base64);
        }
        return null;
    }*/

    public string GenerateQRCode(string text)
    {
        var qrCodeGenerator = new QRCodeGenerator();
        var qrCode = qrCodeGenerator.CreateQrCode(text, ECCLevel.Q);

        var info = new SKImageInfo(300, 300);
        using (var surface = SKSurface.Create(info))
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            float scale = info.Width / (float)qrCode.ModuleMatrix.Count;
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill
            };

            for (int y = 0; y < qrCode.ModuleMatrix.Count; y++)
            {
                for (int x = 0; x < qrCode.ModuleMatrix[y].Count; x++)
                {
                    if (qrCode.ModuleMatrix[y][x])
                    {
                        canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                    }
                }
            }

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                string base64 = Convert.ToBase64String(data.ToArray());
                return $"data:image/png;base64,{base64}";
            }
        }
    }
}
