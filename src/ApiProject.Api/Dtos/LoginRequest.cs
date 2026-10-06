using System.ComponentModel.DataAnnotations;

namespace ApiProject.Api.Dtos;

public class LoginRequest
{
    [Required, MaxLength(100)]
    public string KullaniciAdi { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Sifre { get; set; } = null!;
}
