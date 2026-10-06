using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Entities;

// Ürün adı ve fiyatı sipariş anındaki haliyle saklanır; ürün sonradan değişse de sipariş değişmez.
public class SiparisKalemi : IFirmayaAit
{
    public Guid Id { get; set; }
    public Guid FirmaId { get; set; }
    public Guid SiparisId { get; set; }
    public Guid UrunId { get; set; }
    public string UrunKodu { get; set; } = null!;
    public string UrunAdi { get; set; } = null!;
    public decimal BirimFiyat { get; set; }
    public int Adet { get; set; }

    public Siparis Siparis { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
}
