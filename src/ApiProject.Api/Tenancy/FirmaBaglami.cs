namespace ApiProject.Api.Tenancy;

public class FirmaBaglami : IFirmaBaglami
{
    public Guid? FirmaId { get; private set; }
    public string? FirmaKodu { get; private set; }
    public bool CozulduMu => FirmaId.HasValue;

    public void Ayarla(Guid firmaId, string firmaKodu)
    {
        FirmaId = firmaId;
        FirmaKodu = firmaKodu;
    }
}
