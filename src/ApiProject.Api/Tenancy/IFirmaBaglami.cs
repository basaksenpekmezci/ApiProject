namespace ApiProject.Api.Tenancy;

public interface IFirmaBaglami
{
    int? FirmaId { get; }
    string? FirmaKodu { get; }
    bool CozulduMu { get; }
    void Ayarla(int firmaId, string firmaKodu);
}
