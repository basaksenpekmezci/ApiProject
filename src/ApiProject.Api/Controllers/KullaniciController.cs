using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

[ApiController]
[Route("api/kullanicilar")]
public class KullaniciController : ControllerBase
{
    private readonly KullaniciService _kullaniciService;

    public KullaniciController(KullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<KullaniciDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> KayitOl(KullaniciKayitRequest istek, CancellationToken ct)
    {
        var sonuc = await _kullaniciService.KayitOlAsync(istek, ct);
        return sonuc.Hata switch
        {
            KayitHata.Yok => StatusCode(StatusCodes.Status201Created, sonuc.Kullanici),
            KayitHata.FirmaBelirlenemedi => BadRequest(new { hata = FirmaController.FirmaYokMesaji }),
            _ => Conflict(new { hata = "Bu kullanıcı adı bu firmada zaten kayıtlı." })
        };
    }
}
