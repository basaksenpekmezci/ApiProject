using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using ApiProject.Api.Yetki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

[ApiController]
[Route("api/sepet")]
[Authorize]
public class SepetController : ControllerBase
{
    private readonly SepetService _sepetService;

    public SepetController(SepetService sepetService)
    {
        _sepetService = sepetService;
    }

    [HttpGet]
    [ProducesResponseType<SepetDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Getir(CancellationToken ct)
    {
        return Ok(await _sepetService.GetirAsync(User.KullaniciId(), ct));
    }

    [HttpPost("urunler")]
    [ProducesResponseType<SepetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UrunEkle(SepeteEkleRequest istek, CancellationToken ct)
    {
        var sonuc = await _sepetService.UrunEkleAsync(User.KullaniciId(), istek, ct);
        return Yanit(sonuc);
    }

    [HttpDelete("urunler/{urunId:guid}")]
    [ProducesResponseType<SepetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UrunCikar(Guid urunId, CancellationToken ct)
    {
        var sonuc = await _sepetService.UrunCikarAsync(User.KullaniciId(), urunId, ct);
        return Yanit(sonuc);
    }

    private IActionResult Yanit(SepetSonuc sonuc)
    {
        return sonuc.Hata switch
        {
            SepetHata.Yok => Ok(sonuc.Sepet),
            SepetHata.UrunBulunamadi => NotFound(new { hata = "Ürün bulunamadı." }),
            SepetHata.UrunPasif => BadRequest(new { hata = "Bu ürün satışta değil (pasif)." }),
            SepetHata.StokYetersiz => BadRequest(new { hata = $"Yeterli stok yok. Stokta {sonuc.MevcutStok} adet var." }),
            _ => NotFound(new { hata = "Bu ürün sepette yok." })
        };
    }
}
