using System.Data;
using System.Diagnostics;
using ApiProject.Api.Arama;
using ApiProject.Api.Data;
using ApiProject.Api.Entities;
using ApiProject.Api.Tenancy;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ApiProject.Api.Services;

public record UrunSeedSonucu(string FirmaKodu, int IstenenAdet, int SqlYazilan, int ElasticYazilan, long SureMs, string? ElasticHatasi);

// Deneme ürünleri sadece aktif firmaya yazılır. SQL'e SqlBulkCopy, Elasticsearch'e bulk API ile batch'ler halinde yazılır.
public class UrunSeedService
{
    public const int VarsayilanAdet = 10_000;
    public const int EnFazlaAdet = 1_000_000;
    private const int BatchBoyutu = 5_000;

    private static readonly string[] Renkler = ["Mavi", "Kırmızı", "Yeşil", "Siyah", "Beyaz", "Sarı", "Mor", "Turuncu", "Gri", "Lacivert"];
    private static readonly string[] Urunler = ["Kalem", "Defter", "Silgi", "Cetvel", "Makas", "Klasör", "Ajanda", "Boya", "Fırça", "Zımba", "Kalemtıraş", "Dosya"];
    private static readonly string[] Markalar = ["Atlas", "Kuzey", "Yıldız", "Deniz", "Toros", "Ege", "Pera", "Nova"];

    private readonly AppDbContext _db;
    private readonly ElasticService _elastic;
    private readonly AramaOnbellegi _onbellek;
    private readonly IFirmaBaglami _firmaBaglami;
    private readonly ILogger<UrunSeedService> _logger;

    public UrunSeedService(AppDbContext db, ElasticService elastic, AramaOnbellegi onbellek, IFirmaBaglami firmaBaglami, ILogger<UrunSeedService> logger)
    {
        _db = db;
        _elastic = elastic;
        _onbellek = onbellek;
        _firmaBaglami = firmaBaglami;
        _logger = logger;
    }

    public async Task<UrunSeedSonucu> YukleAsync(int adet, CancellationToken ct = default)
    {
        var firmaId = _firmaBaglami.FirmaId
            ?? throw new InvalidOperationException("Firma belirlenmeden ürün yüklenemez.");

        var sure = Stopwatch.StartNew();
        var baslangic = DateTime.UtcNow;
        var kodOneki = $"DNM{baslangic:yyMMddHHmmss}";
        var rastgele = new Random();
        var idUretici = new SequentialGuidValueGenerator();

        string? elasticHatasi = null;
        try
        {
            await _elastic.YenilemeyiAyarlaAsync(acik: false, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            elasticHatasi = ex.Message;
        }

        var sqlYazilan = 0;
        var elasticYazilan = 0;

        try
        {
            while (sqlYazilan < adet)
            {
                var boyut = Math.Min(BatchBoyutu, adet - sqlYazilan);
                var batch = new List<Urun>(boyut);
                for (var i = 0; i < boyut; i++)
                {
                    var sira = sqlYazilan + i + 1;
                    var urunAdi = Urunler[rastgele.Next(Urunler.Length)];
                    batch.Add(new Urun
                    {
                        Id = idUretici.Next(null!),
                        FirmaId = firmaId,
                        Kod = $"{kodOneki}-{sira:D7}",
                        Ad = $"{Renkler[rastgele.Next(Renkler.Length)]} {urunAdi} {Markalar[rastgele.Next(Markalar.Length)]} {sira}",
                        Aciklama = $"Deneme ürünü: {urunAdi.ToLowerInvariant()}",
                        Fiyat = Math.Round((decimal)(rastgele.NextDouble() * 999 + 1), 2),
                        Stok = rastgele.Next(0, 1001),
                        AktifMi = rastgele.Next(10) != 0,
                        OlusturmaTarihi = baslangic
                    });
                }

                var sqlGorevi = SqlYazAsync(batch, ct);
                var elasticGorevi = elasticHatasi is null ? _elastic.TopluKaydetAsync(batch, ct) : Task.CompletedTask;

                await sqlGorevi;
                sqlYazilan += batch.Count;

                try
                {
                    await elasticGorevi;
                    if (elasticHatasi is null)
                        elasticYazilan += batch.Count;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    elasticHatasi = ex.Message;
                    _logger.LogWarning("Deneme ürünleri Elasticsearch'e yazılamadı, sadece SQL'e devam ediliyor: {Hata}", ex.Message);
                }
            }
        }
        finally
        {
            await _onbellek.FirmayiTemizleAsync();

            try
            {
                await _elastic.YenilemeyiAyarlaAsync(acik: true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Elasticsearch index yenilemesi açılamadı: {Hata}", ex.Message);
            }
        }

        return new UrunSeedSonucu(_firmaBaglami.FirmaKodu!, adet, sqlYazilan, elasticYazilan, sure.ElapsedMilliseconds, elasticHatasi);
    }

    private async Task SqlYazAsync(List<Urun> batch, CancellationToken ct)
    {
        var tablo = new DataTable();
        tablo.Columns.Add(nameof(Urun.Id), typeof(Guid));
        tablo.Columns.Add(nameof(Urun.FirmaId), typeof(Guid));
        tablo.Columns.Add(nameof(Urun.Kod), typeof(string));
        tablo.Columns.Add(nameof(Urun.Ad), typeof(string));
        tablo.Columns.Add(nameof(Urun.Aciklama), typeof(string));
        tablo.Columns.Add(nameof(Urun.Fiyat), typeof(decimal));
        tablo.Columns.Add(nameof(Urun.Stok), typeof(int));
        tablo.Columns.Add(nameof(Urun.AktifMi), typeof(bool));
        tablo.Columns.Add(nameof(Urun.OlusturmaTarihi), typeof(DateTime));

        foreach (var u in batch)
            tablo.Rows.Add(u.Id, u.FirmaId, u.Kod, u.Ad, u.Aciklama, u.Fiyat, u.Stok, u.AktifMi, u.OlusturmaTarihi);

        await using var baglanti = new SqlConnection(_db.Database.GetConnectionString());
        await baglanti.OpenAsync(ct);

        using var kopya = new SqlBulkCopy(baglanti, SqlBulkCopyOptions.CheckConstraints, null)
        {
            DestinationTableName = "Urun",
            BatchSize = batch.Count,
            BulkCopyTimeout = 0
        };
        foreach (DataColumn kolon in tablo.Columns)
            kopya.ColumnMappings.Add(kolon.ColumnName, kolon.ColumnName);

        await kopya.WriteToServerAsync(tablo, ct);
    }
}
