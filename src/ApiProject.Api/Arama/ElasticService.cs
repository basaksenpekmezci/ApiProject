using ApiProject.Api.Dtos;
using ApiProject.Api.Entities;
using ApiProject.Api.Tenancy;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Elastic.Transport.Products.Elasticsearch;

namespace ApiProject.Api.Arama;

public class ElasticHatasi(string mesaj) : Exception(mesaj);

// Elasticsearch'teki her işlem aktif firmayla sınırlıdır: aramalara FirmaId filtresi her zaman eklenir,
// başka firmaya ait ürün index'e yazılamaz. Firma belirlenmemişse hiçbir işlem yapılmaz.
public class ElasticService
{
    public const string IndeksAdi = "urunler";

    private static volatile bool _indeksHazir;

    private readonly ElasticsearchClient _client;
    private readonly IFirmaBaglami _firmaBaglami;

    public ElasticService(ElasticsearchClient client, IFirmaBaglami firmaBaglami)
    {
        _client = client;
        _firmaBaglami = firmaBaglami;
    }

    private Guid AktifFirmaId => _firmaBaglami.FirmaId
        ?? throw new InvalidOperationException("Firma belirlenmeden Elasticsearch'te işlem yapılamaz.");

    // Ad ve Kod alanları 2-10 harflik parçalara bölünerek index'lenir, böylece kelimenin bir kısmı yazılınca da bulunur.
    // Türkçe büyük/küçük harf ve ı/i, ş/s gibi farklar aramada sayılmaz.
    private const string IndeksTanimi = """
    {
      "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "max_ngram_diff": 8,
        "analysis": {
          "tokenizer": {
            "ad_parca": { "type": "ngram", "min_gram": 2, "max_gram": 10, "token_chars": ["letter", "digit"] },
            "kod_parca": { "type": "ngram", "min_gram": 2, "max_gram": 10, "token_chars": ["letter", "digit", "punctuation", "symbol"] }
          },
          "filter": {
            "tr_kucuk": { "type": "lowercase", "language": "turkish" }
          },
          "analyzer": {
            "ad_parca": { "type": "custom", "tokenizer": "ad_parca", "filter": ["tr_kucuk", "asciifolding"] },
            "kod_parca": { "type": "custom", "tokenizer": "kod_parca", "filter": ["tr_kucuk", "asciifolding"] }
          },
          "normalizer": {
            "kucuk": { "type": "custom", "filter": ["lowercase", "asciifolding"] }
          }
        }
      },
      "mappings": {
        "dynamic": "strict",
        "properties": {
          "FirmaId": { "type": "keyword" },
          "Id": { "type": "keyword" },
          "Kod": { "type": "text", "analyzer": "kod_parca", "fields": { "ham": { "type": "keyword", "normalizer": "kucuk" } } },
          "Ad": { "type": "text", "analyzer": "ad_parca", "fields": { "sirala": { "type": "keyword", "normalizer": "kucuk" } } },
          "Aciklama": { "type": "text" },
          "Fiyat": { "type": "scaled_float", "scaling_factor": 100 },
          "Stok": { "type": "integer" },
          "AktifMi": { "type": "boolean" }
        }
      }
    }
    """;

    public async Task IndeksHazirlaAsync(CancellationToken ct = default)
    {
        if (_indeksHazir)
            return;

        var varMi = await _client.Indices.ExistsAsync(IndeksAdi, ct);
        if (!varMi.IsValidResponse && varMi.ApiCallDetails.HttpStatusCode != 404)
            throw Hata("Index kontrol edilemedi", varMi);

        if (!varMi.Exists)
        {
            var cevap = await _client.Transport.PutAsync<StringResponse>(IndeksAdi, PostData.String(IndeksTanimi), null, ct);
            // Aynı anda iki istek index'i oluşturmaya çalışırsa ikincisi "zaten var" hatası alır, bu sorun değil.
            if (!cevap.ApiCallDetails.HasSuccessfulStatusCode && !cevap.Body.Contains("resource_already_exists_exception"))
                throw new ElasticHatasi($"Index oluşturulamadı: {cevap.ApiCallDetails.HttpStatusCode} {cevap.Body}");
        }

        _indeksHazir = true;
    }

    // Kayıt aramada hemen görünsün diye index yenilenene kadar beklenir.
    public async Task KaydetAsync(Urun urun, CancellationToken ct = default)
    {
        var dokuman = DokumanYap(urun);
        await IndeksHazirlaAsync(ct);

        var cevap = await _client.IndexAsync(dokuman, i => i.Index(IndeksAdi).Id(dokuman.Id.ToString()).Refresh(Refresh.WaitFor), ct);
        if (!cevap.IsValidResponse)
            throw Hata("Ürün index'e yazılamadı", cevap);
    }

    public async Task TopluKaydetAsync(IReadOnlyCollection<Urun> urunler, CancellationToken ct = default)
    {
        if (urunler.Count == 0)
            return;

        var dokumanlar = urunler.Select(DokumanYap).ToList();
        await IndeksHazirlaAsync(ct);

        var istek = new BulkRequest(IndeksAdi)
        {
            Operations = dokumanlar
                .Select(d => (IBulkOperation)new BulkIndexOperation<UrunDokumani>(d) { Id = d.Id.ToString() })
                .ToList()
        };

        var cevap = await _client.BulkAsync(istek, ct);
        if (!cevap.IsValidResponse || cevap.Errors)
        {
            var ilkHata = cevap.ItemsWithErrors.FirstOrDefault()?.Error?.Reason;
            throw new ElasticHatasi($"Toplu yazma başarısız: {ilkHata ?? cevap.DebugInformation}");
        }
    }

    public async Task<(List<UrunDto> Urunler, long Toplam)> AraAsync(string? arama, bool sadeceAktif, int limit, CancellationToken ct = default)
    {
        await IndeksHazirlaAsync(ct);

        var filtreler = new List<Query>
        {
            new TermQuery(new Field(nameof(UrunDokumani.FirmaId))) { Value = AktifFirmaId.ToString() }
        };
        if (sadeceAktif)
            filtreler.Add(new TermQuery(new Field(nameof(UrunDokumani.AktifMi))) { Value = true });

        var sorgu = new BoolQuery { Filter = filtreler };
        var siralama = new List<SortOptions>();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var metin = arama.Trim();
            sorgu.Should =
            [
                new MatchQuery(new Field(nameof(UrunDokumani.Ad))) { Query = metin, Operator = Operator.And },
                new MatchQuery(new Field(nameof(UrunDokumani.Kod))) { Query = metin, Operator = Operator.And }
            ];
            sorgu.MinimumShouldMatch = 1;
            siralama.Add(SortOptions.Score(new ScoreSort { Order = SortOrder.Desc }));
        }

        siralama.Add(SortOptions.Field(new Field("Ad.sirala"), new FieldSort { Order = SortOrder.Asc }));

        var istek = new SearchRequest<UrunDokumani>(IndeksAdi)
        {
            Query = sorgu,
            Size = limit,
            TrackTotalHits = new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(true),
            Sort = siralama
        };

        var cevap = await _client.SearchAsync<UrunDokumani>(istek, ct);
        if (!cevap.IsValidResponse)
            throw Hata("Arama yapılamadı", cevap);

        var sonuclar = cevap.Documents
            .Select(d => new UrunDto(d.Id, d.Kod, d.Ad, d.Aciklama, d.Fiyat, d.Stok, d.AktifMi))
            .ToList();
        return (sonuclar, cevap.Total);
    }

    private UrunDokumani DokumanYap(Urun urun)
    {
        if (urun.FirmaId != AktifFirmaId)
            throw new InvalidOperationException("Başka firmaya ait ürün index'e yazılamaz.");

        return new UrunDokumani
        {
            FirmaId = urun.FirmaId,
            Id = urun.Id,
            Kod = urun.Kod,
            Ad = urun.Ad,
            Aciklama = urun.Aciklama,
            Fiyat = urun.Fiyat,
            Stok = urun.Stok,
            AktifMi = urun.AktifMi
        };
    }

    private static ElasticHatasi Hata(string mesaj, ElasticsearchResponse cevap) =>
        new($"{mesaj}: {cevap.ElasticsearchServerError?.Error?.Reason ?? cevap.ApiCallDetails.OriginalException?.Message ?? cevap.ApiCallDetails.HttpStatusCode?.ToString()}");
}
