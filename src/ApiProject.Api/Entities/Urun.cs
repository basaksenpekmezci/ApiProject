using ApiProject.Api.Tenancy;

namespace ApiProject.Api.Entities;

public class Urun : IFirmayaAit
{
    public Guid Id { get; set; }
    public Guid FirmaId { get; set; }
    public string Kod { get; set; } = null!;
    public string Ad { get; set; } = null!;
    public string? Aciklama { get; set; }
    public decimal Fiyat { get; set; }
    public int Stok { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturmaTarihi { get; set; }

    public Firma Firma { get; set; } = null!;
}
