using System.ComponentModel.DataAnnotations;

namespace ApiProject.Api.Dtos;

// Hem ürün eklemede hem güncellemede kullanılır.
public class UrunKaydetRequest
{
    [Required, MaxLength(50)]
    public string Kod { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Ad { get; set; } = null!;

    [MaxLength(1000)]
    public string? Aciklama { get; set; }

    [Range(0.01, 10_000_000)]
    public decimal Fiyat { get; set; }

    [Range(0, 1_000_000)]
    public int Stok { get; set; }

    public bool AktifMi { get; set; } = true;
}
