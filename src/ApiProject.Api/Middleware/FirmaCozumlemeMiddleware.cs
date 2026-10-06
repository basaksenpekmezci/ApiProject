using ApiProject.Api.Services;
using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Middleware;

// UseAuthentication'dan sonra çalışmalı: token'daki firma ile isteğin firması karşılaştırılıyor.
public class FirmaCozumlemeMiddleware
{
    private readonly RequestDelegate _next;

    public FirmaCozumlemeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IFirmaBaglami firmaBaglami, FirmaService firmaService)
    {
        var ct = context.RequestAborted;
        var hostFirma = await firmaService.DomaindenBulAsync(context.Request.Host.Host, ct);

        FirmaOzet? headerFirma = null;
        var xClient = context.Request.Headers[FirmaHeaderlari.XClient].ToString().Trim();
        if (xClient.Length > 0)
        {
            headerFirma = await firmaService.KoddanBulAsync(xClient, ct);
            if (headerFirma is null)
            {
                await Yanitla(context, StatusCodes.Status400BadRequest, $"'{xClient}' kodlu firma bulunamadı.");
                return;
            }

            if (hostFirma is not null && hostFirma.Id != headerFirma.Id)
            {
                await Yanitla(context, StatusCodes.Status400BadRequest, "Host ve X-Client farklı firmaları gösteriyor.");
                return;
            }
        }

        var firma = hostFirma ?? headerFirma;
        if (firma is not null)
            firmaBaglami.Ayarla(firma.Id, firma.FirmaKodu);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idClaim = context.User.FindFirst(FirmaClaimTipleri.FirmaId)?.Value;
            var kodClaim = context.User.FindFirst(FirmaClaimTipleri.FirmaKodu)?.Value;
            if (!Guid.TryParse(idClaim, out var tokenFirmaId) || kodClaim is null)
            {
                await Yanitla(context, StatusCodes.Status403Forbidden, "Token firma bilgisi içermiyor.");
                return;
            }

            if (firmaBaglami.CozulduMu && firmaBaglami.FirmaId != tokenFirmaId)
            {
                await Yanitla(context, StatusCodes.Status403Forbidden, "Bu token bu firmaya ait değil.");
                return;
            }

            if (!firmaBaglami.CozulduMu)
                firmaBaglami.Ayarla(tokenFirmaId, kodClaim);
        }

        await _next(context);
    }

    private static Task Yanitla(HttpContext context, int statusCode, string mesaj)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { hata = mesaj });
    }
}
