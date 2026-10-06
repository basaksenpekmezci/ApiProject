using System.Text.Json;
using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using ApiProject.Api.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ApiProject.Api.Services;

public record FirmaOzet(Guid Id, string FirmaKodu);

public class FirmaService
{
    private static readonly TimeSpan BulunduSure = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan BulunamadiSure = TimeSpan.FromMinutes(1);

    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public FirmaService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // Bulunamayan domainler de kısa süreliğine cache'lenir.
    public async Task<FirmaOzet?> DomaindenBulAsync(string host, CancellationToken ct = default)
    {
        var domain = DomainYardimci.Normalize(host);
        var anahtar = $"firma-domain:{domain}";
        if (_cache.TryGetValue(anahtar, out FirmaOzet? ozet))
            return ozet;

        ozet = await _db.FirmaDomainleri
            .Where(d => d.Domain == domain && d.Firma.AktifMi)
            .Select(d => new FirmaOzet(d.FirmaId, d.Firma.FirmaKodu))
            .FirstOrDefaultAsync(ct);

        _cache.Set(anahtar, ozet, ozet is null ? BulunamadiSure : BulunduSure);
        return ozet;
    }

    public Task<FirmaOzet?> KoddanBulAsync(string firmaKodu, CancellationToken ct = default)
    {
        var kod = firmaKodu.Trim();
        return _db.Firmalar
            .Where(f => f.FirmaKodu == kod && f.AktifMi)
            .Select(f => new FirmaOzet(f.Id, f.FirmaKodu))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FirmaConfigDto?> ConfigGetirAsync(Guid firmaId, CancellationToken ct = default)
    {
        var f = await _db.Firmalar
            .Where(x => x.Id == firmaId && x.AktifMi)
            .Select(x => new
            {
                x.FirmaKodu,
                x.FirmaAdi,
                x.Ayar
            })
            .FirstOrDefaultAsync(ct);
        if (f is null)
            return null;

        JsonElement? ek = null;
        if (!string.IsNullOrWhiteSpace(f.Ayar?.EkAyarlarJson))
        {
            using var doc = JsonDocument.Parse(f.Ayar.EkAyarlarJson);
            ek = doc.RootElement.Clone();
        }

        return new FirmaConfigDto(
            f.FirmaKodu,
            f.FirmaAdi,
            f.Ayar?.LogoUrl,
            f.Ayar?.TemaRengi,
            f.Ayar?.Dil ?? "tr-TR",
            f.Ayar?.ZamanDilimi ?? "Europe/Istanbul",
            ek);
    }
}
