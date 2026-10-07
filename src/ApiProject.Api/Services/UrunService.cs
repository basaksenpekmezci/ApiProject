using System.Diagnostics;
using ApiProject.Api.Arama;
using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Services;

public enum UrunHata
{
    Yok,
    KodAlinmis,
    Bulunamadi
}

public record UrunSonuc(UrunHata Hata, UrunDto? Urun = null);

// Tüm sorgular firma filtresinden geçer: bir firma sadece kendi ürünlerini görür ve değiştirir.
public class UrunService
{
    private const int AramaLimiti = 100;

    private readonly AppDbContext _db;
    private readonly ElasticService _elastic;
    private readonly ILogger<UrunService> _logger;

    public UrunService(AppDbContext db, ElasticService elastic, ILogger<UrunService> logger)
    {
        _db = db;
        _elastic = elastic;
        _logger = logger;
    }

    // Arama önce Elasticsearch'te yapılır; Elasticsearch'e ulaşılamazsa SQL'deki aramaya düşülür.
    public async Task<UrunAramaSonucu> AraAsync(string? arama, bool sadeceAktif, CancellationToken ct = default)
    {
        var sure = Stopwatch.StartNew();
        List<UrunDto> urunler;
        long toplam;
        string kaynak;

        try
        {
            (urunler, toplam) = await _elastic.AraAsync(arama, sadeceAktif, AramaLimiti, ct);
            kaynak = AramaKaynagi.Elasticsearch;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning("Elasticsearch'te arama yapılamadı, SQL'e düşülüyor: {Hata}", ex.Message);
            (urunler, toplam) = await SqlAraAsync(arama, sadeceAktif, ct);
            kaynak = AramaKaynagi.Sql;
        }

        return new UrunAramaSonucu(urunler, toplam, sure.ElapsedMilliseconds, kaynak);
    }

    private async Task<(List<UrunDto> Urunler, long Toplam)> SqlAraAsync(string? arama, bool sadeceAktif, CancellationToken ct)
    {
        var sorgu = _db.Urunler.AsNoTracking();

        if (sadeceAktif)
            sorgu = sorgu.Where(u => u.AktifMi);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var metin = arama.Trim();
            sorgu = sorgu.Where(u => u.Ad.Contains(metin) || u.Kod.Contains(metin));
        }

        var toplam = await sorgu.LongCountAsync(ct);
        var urunler = await sorgu
            .OrderBy(u => u.Ad)
            .Take(AramaLimiti)
            .Select(u => new UrunDto(u.Id, u.Kod, u.Ad, u.Aciklama, u.Fiyat, u.Stok, u.AktifMi))
            .ToListAsync(ct);
        return (urunler, toplam);
    }

    public async Task<UrunSonuc> EkleAsync(UrunKaydetRequest istek, CancellationToken ct = default)
    {
        var kod = istek.Kod.Trim();
        if (await _db.Urunler.AnyAsync(u => u.Kod == kod, ct))
            return new UrunSonuc(UrunHata.KodAlinmis);

        var urun = new Urun { Kod = kod, OlusturmaTarihi = DateTime.UtcNow };
        Doldur(urun, istek);
        _db.Urunler.Add(urun);
        return await KaydetAsync(urun, ct);
    }

    public async Task<UrunSonuc> GuncelleAsync(Guid id, UrunKaydetRequest istek, CancellationToken ct = default)
    {
        var urun = await _db.Urunler.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (urun is null)
            return new UrunSonuc(UrunHata.Bulunamadi);

        var kod = istek.Kod.Trim();
        if (kod != urun.Kod && await _db.Urunler.AnyAsync(u => u.Kod == kod, ct))
            return new UrunSonuc(UrunHata.KodAlinmis);

        urun.Kod = kod;
        Doldur(urun, istek);
        return await KaydetAsync(urun, ct);
    }

    private static void Doldur(Urun urun, UrunKaydetRequest istek)
    {
        urun.Ad = istek.Ad.Trim();
        urun.Aciklama = istek.Aciklama?.Trim();
        urun.Fiyat = istek.Fiyat;
        urun.Stok = istek.Stok;
        urun.AktifMi = istek.AktifMi;
    }

    private async Task<UrunSonuc> KaydetAsync(Urun urun, CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Aynı anda aynı kodla gelen iki istekte unique index ikincisini durdurur.
            return new UrunSonuc(UrunHata.KodAlinmis);
        }

        // SQL asıl kayıt yeri; Elasticsearch'e yazılamazsa ürün yine kaydedilmiş sayılır.
        try
        {
            await _elastic.KaydetAsync(urun, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Ürün {UrunId} Elasticsearch'e yazılamadı: {Hata}", urun.Id, ex.Message);
        }

        return new UrunSonuc(UrunHata.Yok, UrunDtoYap(urun));
    }

    public static UrunDto UrunDtoYap(Urun u) => new(u.Id, u.Kod, u.Ad, u.Aciklama, u.Fiyat, u.Stok, u.AktifMi);
}
