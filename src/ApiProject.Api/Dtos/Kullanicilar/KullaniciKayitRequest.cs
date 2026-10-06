using System.ComponentModel.DataAnnotations;

namespace ApiProject.Api.Dtos;

public class KullaniciKayitRequest
{
    [Required, MaxLength(100)]
    public string KullaniciAdi { get; set; } = null!;

    [Required, MinLength(6), MaxLength(200)]
    public string Sifre { get; set; } = null!;

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? AdSoyad { get; set; }
}
