using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Entities;

public class Siparis : IFirmayaAit
{
    public Guid Id { get; set; }
    public Guid FirmaId { get; set; }
    public Guid KullaniciId { get; set; }
    public decimal ToplamTutar { get; set; }
    public DateTime OlusturmaTarihi { get; set; }

    public Kullanici Kullanici { get; set; } = null!;
    public List<SiparisKalemi> Kalemler { get; set; } = new();
}
