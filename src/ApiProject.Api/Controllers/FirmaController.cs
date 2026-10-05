using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using ApiProject.Api.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

[ApiController]
[Route("api/firma")]
public class FirmaController : ControllerBase
{
    private readonly IFirmaBaglami _firmaBaglami;
    private readonly FirmaService _firmaService;

    public FirmaController(IFirmaBaglami firmaBaglami, FirmaService firmaService)
    {
        _firmaBaglami = firmaBaglami;
        _firmaService = firmaService;
    }

    /// <summary>İsteğin geldiği domaine göre firmanın config bilgilerini döner.</summary>
    [HttpGet("config")]
    [AllowAnonymous]
    [ProducesResponseType<FirmaConfigDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Config(CancellationToken ct)
    {
        if (!_firmaBaglami.CozulduMu)
            return NotFound(new { hata = $"'{Request.Host.Host}' domaini için firma bulunamadı." });

        var config = await _firmaService.ConfigGetirAsync(_firmaBaglami.FirmaId!.Value, ct);
        return config is null ? NotFound(new { hata = "Firma bulunamadı." }) : Ok(config);
    }
}
