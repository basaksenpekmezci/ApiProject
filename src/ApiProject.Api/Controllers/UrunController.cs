using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using ApiProject.Api.Yetki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

[ApiController]
[Route("api/urunler")]
[Authorize]
public class UrunController : ControllerBase
{
    private const string KodAlinmisMesaji = "Bu ürün kodu bu firmada zaten kullanılıyor.";

    private readonly UrunService _urunService;

    public UrunController(UrunService urunService)
    {
        _urunService = urunService;
    }

    // Örnek: GET /api/urunler?arama=kalem  (pasifleri de görmek için &sadeceAktif=false)
    [HttpGet]
    [ProducesResponseType<List<UrunDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ara([FromQuery] string? arama, [FromQuery] bool sadeceAktif = true, CancellationToken ct = default)
    {
        return Ok(await _urunService.AraAsync(arama, sadeceAktif, ct));
    }

    [HttpPost]
    [Authorize(Policy = YetkiTanimlari.YoneticiPolitikasi)]
    [ProducesResponseType<UrunDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Ekle(UrunKaydetRequest istek, CancellationToken ct)
    {
        var sonuc = await _urunService.EkleAsync(istek, ct);
        return sonuc.Hata == UrunHata.Yok
            ? StatusCode(StatusCodes.Status201Created, sonuc.Urun)
            : Conflict(new { hata = KodAlinmisMesaji });
    }

    // Stok, fiyat ya da aktif/pasif durumunu değiştirmek için.
    [HttpPut("{id:guid}")]
    [Authorize(Policy = YetkiTanimlari.YoneticiPolitikasi)]
    [ProducesResponseType<UrunDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Guncelle(Guid id, UrunKaydetRequest istek, CancellationToken ct)
    {
        var sonuc = await _urunService.GuncelleAsync(id, istek, ct);
        return sonuc.Hata switch
        {
            UrunHata.Yok => Ok(sonuc.Urun),
            UrunHata.Bulunamadi => NotFound(new { hata = "Ürün bulunamadı." }),
            _ => Conflict(new { hata = KodAlinmisMesaji })
        };
    }
}
