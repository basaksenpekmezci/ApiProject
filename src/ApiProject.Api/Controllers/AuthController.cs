using ApiProject.Api.Dtos;
using ApiProject.Api.Services;
using ApiProject.Api.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProject.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Firma önce isteğin domaininden bulunur. Bulunamazsa body'deki firmaKodu kullanılır.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest istek, CancellationToken ct)
    {
        var sonuc = await _authService.LoginAsync(istek, ct);
        return sonuc.Hata switch
        {
            LoginHata.Yok => Ok(sonuc.Yanit),
            LoginHata.FirmaKoduGerekli => BadRequest(new { hata = "Bu domain bir firmaya bağlı değil, firmaKodu gönderilmeli." }),
            LoginHata.FirmaKoduDomainleUyusmuyor => BadRequest(new { hata = "firmaKodu, isteğin geldiği domainin firmasıyla uyuşmuyor." }),
            _ => Unauthorized(new { hata = "Kullanıcı adı veya şifre hatalı." })
        };
    }

    /// <summary>Token'ı test etmek için: giriş yapan kullanıcının ve firmanın bilgisini döner.</summary>
    [HttpGet("ben")]
    [Authorize]
    public IActionResult Ben([FromServices] IFirmaBaglami firmaBaglami)
    {
        return Ok(new
        {
            kullaniciId = User.FindFirst("sub")?.Value,
            kullaniciAdi = User.Identity?.Name,
            firmaId = firmaBaglami.FirmaId,
            firmaKodu = firmaBaglami.FirmaKodu
        });
    }
}
