namespace ApiProject.Api.Tenancy;

/// <summary>O anki isteğin hangi firmaya ait olduğu. İstek başına (scoped) tek örnek.</summary>
public interface IFirmaBaglami
{
    int? FirmaId { get; }
    string? FirmaKodu { get; }
    bool CozulduMu { get; }
    void Ayarla(int firmaId, string firmaKodu);
}
