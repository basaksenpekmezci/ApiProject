using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using ApiProject.Api.Yetki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

// Yönetim paneli bu uç noktaları kullanır. Sadece yönetici kullanıcılar erişebilir.
[ApiController]
[Route("api/yonetim")]
[Authorize(Policy = YetkiTanimlari.YoneticiPolitikasi)]
public class YonetimController : ControllerBase
{
    private readonly YonetimService _yonetimService;
    private readonly UrunSeedService _urunSeedService;

    public YonetimController(YonetimService yonetimService, UrunSeedService urunSeedService)
    {
        _yonetimService = yonetimService;
        _urunSeedService = urunSeedService;
    }

    [HttpGet("siparisler")]
    [ProducesResponseType<List<SiparisDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Siparisler(CancellationToken ct)
    {
        return Ok(await _yonetimService.SiparisleriGetirAsync(ct));
    }

    [HttpGet("kullanicilar")]
    [ProducesResponseType<List<KullaniciDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Kullanicilar(CancellationToken ct)
    {
        return Ok(await _yonetimService.KullanicilariGetirAsync(ct));
    }

    // Giriş yapan yöneticinin firmasına deneme ürünleri ekler. Örnek: POST /api/yonetim/urunler/seed?adet=500000
    [HttpPost("urunler/seed")]
    [ProducesResponseType<UrunSeedSonucu>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UrunSeed([FromQuery] int adet = UrunSeedService.VarsayilanAdet, CancellationToken ct = default)
    {
        if (adet < 1 || adet > UrunSeedService.EnFazlaAdet)
            return BadRequest(new { hata = $"Adet 1 ile {UrunSeedService.EnFazlaAdet} arasında olmalı." });

        return Ok(await _urunSeedService.YukleAsync(adet, ct));
    }
}
