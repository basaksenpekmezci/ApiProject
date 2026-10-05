using ApiProject.Api.Services;
using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Middleware;

/// <summary>
/// UseAuthentication'dan sonra çalışır.
/// 1. İsteğin Host'undan firmayı bulup IFirmaBaglami'na yazar.
/// 2. Token varsa, token'ın firması domainin firmasıyla aynı olmalı; değilse 403.
///    Domainden firma çıkmadıysa (ör. localhost) firma token'dan alınır.
/// </summary>
public class FirmaCozumlemeMiddleware
{
    private readonly RequestDelegate _next;

    public FirmaCozumlemeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IFirmaBaglami firmaBaglami, FirmaService firmaService)
    {
        var firma = await firmaService.DomaindenBulAsync(context.Request.Host.Host, context.RequestAborted);
        if (firma is not null)
            firmaBaglami.Ayarla(firma.Id, firma.FirmaKodu);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idClaim = context.User.FindFirst(FirmaClaimTipleri.FirmaId)?.Value;
            var kodClaim = context.User.FindFirst(FirmaClaimTipleri.FirmaKodu)?.Value;
            if (!int.TryParse(idClaim, out var tokenFirmaId) || kodClaim is null)
            {
                await Reddet(context, "Token firma bilgisi içermiyor.");
                return;
            }

            if (firmaBaglami.CozulduMu && firmaBaglami.FirmaId != tokenFirmaId)
            {
                await Reddet(context, "Bu token bu domainin firmasına ait değil.");
                return;
            }

            if (!firmaBaglami.CozulduMu)
                firmaBaglami.Ayarla(tokenFirmaId, kodClaim);
        }

        await _next(context);
    }

    private static Task Reddet(HttpContext context, string mesaj)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return context.Response.WriteAsJsonAsync(new { hata = mesaj });
    }
}
