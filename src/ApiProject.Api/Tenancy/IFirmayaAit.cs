namespace ApiProject.Api.Tenancy;

// Bu arayüzü taşıyan her tablo AppDbContext'te otomatik olarak aktif firmaya göre filtrelenir.
public interface IFirmayaAit
{
    Guid FirmaId { get; set; }
}
