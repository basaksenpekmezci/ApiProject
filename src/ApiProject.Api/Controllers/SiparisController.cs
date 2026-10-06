using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using ApiProject.Api.Yetki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

[ApiController]
[Route("api/siparisler")]
[Authorize]
public class SiparisController : ControllerBase
{
    private readonly SiparisService _siparisService;

    public SiparisController(SiparisService siparisService)
    {
        _siparisService = siparisService;
    }

    // Sepetteki ürünlerle sipariş oluşturur ve sepeti boşaltır.
    [HttpPost]
    [ProducesResponseType<SiparisDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Olustur(CancellationToken ct)
    {
        var sonuc = await _siparisService.SepettenOlusturAsync(User.KullaniciId(), ct);
        return sonuc.Hata switch
        {
            SiparisHata.Yok => StatusCode(StatusCodes.Status201Created, sonuc.Siparis),
            SiparisHata.SepetBos => BadRequest(new { hata = "Sepet boş." }),
            SiparisHata.UrunPasif => BadRequest(new { hata = $"'{sonuc.UrunAdi}' artık satışta değil, sepetten çıkarın." }),
            _ => BadRequest(new { hata = $"'{sonuc.UrunAdi}' için yeterli stok yok." })
        };
    }

    [HttpGet]
    [ProducesResponseType<List<SiparisDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Siparislerim(CancellationToken ct)
    {
        return Ok(await _siparisService.KullanicininSiparisleriAsync(User.KullaniciId(), ct));
    }
}
