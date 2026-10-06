using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Entities;

// Her kullanıcının tek bir sepeti var; sepet, o kullanıcının SepetKalemi satırlarından oluşur.
public class SepetKalemi : IFirmayaAit
{
    public Guid Id { get; set; }
    public Guid FirmaId { get; set; }
    public Guid KullaniciId { get; set; }
    public Guid UrunId { get; set; }
    public int Adet { get; set; }
    public DateTime EklenmeTarihi { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
}
