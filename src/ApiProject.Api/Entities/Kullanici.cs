using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Entities;

public class Kullanici : IFirmayaAit
{
    public Guid Id { get; set; }
    public Guid FirmaId { get; set; }
    public string KullaniciAdi { get; set; } = null!;
    public string NormalizeKullaniciAdi { get; set; } = null!;
    public string SifreHash { get; set; } = null!;
    public string? Email { get; set; }
    public string? AdSoyad { get; set; }
    public bool AktifMi { get; set; } = true;
    public int HataliGirisSayisi { get; set; }
    public DateTime? KilitBitisTarihi { get; set; }
    public DateTime? SonGirisTarihi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }

    public Firma Firma { get; set; } = null!;
}
