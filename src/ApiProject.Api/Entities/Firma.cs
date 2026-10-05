namespace ApiProject.Api.Entities;

public class Firma
{
    public int Id { get; set; }
    public string FirmaKodu { get; set; } = null!;
    public string FirmaAdi { get; set; } = null!;
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturmaTarihi { get; set; }

    public FirmaAyar? Ayar { get; set; }
    public List<FirmaDomain> Domainler { get; set; } = new();
}
