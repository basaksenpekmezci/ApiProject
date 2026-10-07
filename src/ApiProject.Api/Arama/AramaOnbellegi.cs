using System.Text.Json;
using ApiProject.Api.Dtos;
using ApiProject.Api.Tenancy;
using StackExchange.Redis;

namespace ApiProject.Api.Arama;

public record OnbellektekiArama(List<UrunDto> Sonuclar, long ToplamKayit);

// Arama sonuçları Redis'te 15 dakika tutulur. Anahtar firmaId'yi içerir, böylece bir firma başka firmanın sonucunu okuyamaz.
// Firmanın ürünleri değişince firmanın sürüm numarası artırılır; eski sürümlü anahtarlar bir daha okunmaz ve süreleri dolunca silinir.
// Redis'e ulaşılamazsa önbellek beklemeden atlanır, arama Elasticsearch'ten devam eder; bağlantı arka planda yeniden kurulur.
public class AramaOnbellegi
{
    private static readonly TimeSpan Sure = TimeSpan.FromMinutes(15);

    private readonly IConnectionMultiplexer _redis;
    private readonly IFirmaBaglami _firmaBaglami;
    private readonly ILogger<AramaOnbellegi> _logger;

    public AramaOnbellegi(IConnectionMultiplexer redis, IFirmaBaglami firmaBaglami, ILogger<AramaOnbellegi> logger)
    {
        _redis = redis;
        _firmaBaglami = firmaBaglami;
        _logger = logger;
    }

    private Guid AktifFirmaId => _firmaBaglami.FirmaId
        ?? throw new InvalidOperationException("Firma belirlenmeden önbellek kullanılamaz.");

    private string SurumAnahtari => $"urun-arama-surum:{AktifFirmaId}";

    public async Task<OnbellektekiArama?> OkuAsync(string? arama, bool sadeceAktif)
    {
        if (!_redis.IsConnected)
            return null;

        try
        {
            var db = _redis.GetDatabase();
            var deger = await db.StringGetAsync(await AnahtarAsync(db, arama, sadeceAktif));
            return deger.HasValue ? JsonSerializer.Deserialize<OnbellektekiArama>(deger.ToString()) : null;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException or JsonException)
        {
            _logger.LogWarning("Redis'ten okunamadı: {Hata}", ex.Message);
            return null;
        }
    }

    public async Task YazAsync(string? arama, bool sadeceAktif, OnbellektekiArama sonuc)
    {
        if (!_redis.IsConnected)
            return;

        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(await AnahtarAsync(db, arama, sadeceAktif), JsonSerializer.Serialize(sonuc), Sure);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning("Redis'e yazılamadı: {Hata}", ex.Message);
        }
    }

    public async Task FirmayiTemizleAsync()
    {
        if (!_redis.IsConnected)
            return;

        try
        {
            await _redis.GetDatabase().StringIncrementAsync(SurumAnahtari);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogWarning("Redis'teki arama sonuçları temizlenemedi: {Hata}", ex.Message);
        }
    }

    // Örnek: urun-arama:10000000-0000-0000-0000-000000000001:s3:aktif:mavi kalem
    private async Task<string> AnahtarAsync(IDatabase db, string? arama, bool sadeceAktif)
    {
        var surum = (long?)await db.StringGetAsync(SurumAnahtari) ?? 0;
        var metin = (arama ?? "").Trim().ToLowerInvariant();
        return $"urun-arama:{AktifFirmaId}:s{surum}:{(sadeceAktif ? "aktif" : "tumu")}:{metin}";
    }
}
