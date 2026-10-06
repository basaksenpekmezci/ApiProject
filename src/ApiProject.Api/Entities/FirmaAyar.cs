namespace ApiProject.Api.Entities;

public class FirmaAyar
{
    public Guid FirmaId { get; set; }
    public string? LogoUrl { get; set; }
    public string? TemaRengi { get; set; }
    public string Dil { get; set; } = "tr-TR";
    public string ZamanDilimi { get; set; } = "Europe/Istanbul";
    public string? EkAyarlarJson { get; set; }
    public DateTime GuncellemeTarihi { get; set; }

    public Firma Firma { get; set; } = null!;
}
