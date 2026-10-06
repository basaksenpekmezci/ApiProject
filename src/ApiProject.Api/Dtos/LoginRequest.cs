using System.ComponentModel.DataAnnotations;

namespace ApiProject.Api.Dtos;

public class LoginRequest
{
    [MaxLength(50)]
    public string? FirmaKodu { get; set; }

    [Required, MaxLength(100)]
    public string KullaniciAdi { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Sifre { get; set; } = null!;
}
