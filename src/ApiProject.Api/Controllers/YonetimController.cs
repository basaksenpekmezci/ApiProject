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

    public YonetimController(YonetimService yonetimService)
    {
        _yonetimService = yonetimService;
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
}
