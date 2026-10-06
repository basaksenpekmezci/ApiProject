namespace ApiProject.Api.Tenancy;

public interface IFirmaBaglami
{
    Guid? FirmaId { get; }
    string? FirmaKodu { get; }
    bool CozulduMu { get; }
    void Ayarla(Guid firmaId, string firmaKodu);
}
