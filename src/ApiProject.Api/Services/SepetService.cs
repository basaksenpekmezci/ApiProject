using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Services;

public enum SepetHata
{
    Yok,
    UrunBulunamadi,
    UrunPasif,
    StokYetersiz,
    SepetteYok
}

public record SepetSonuc(SepetHata Hata, SepetDto? Sepet = null, int MevcutStok = 0);

// Firma filtresi başka firmanın ürününü ve sepetini gizler; ayrıca her sorgu giriş yapan kullanıcıyla sınırlanır.
public class SepetService
{
    private readonly AppDbContext _db;

    public SepetService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SepetDto> GetirAsync(Guid kullaniciId, CancellationToken ct = default)
    {
        var kalemler = await _db.SepetKalemleri
            .AsNoTracking()
            .Where(k => k.KullaniciId == kullaniciId)
            .OrderBy(k => k.EklenmeTarihi)
            .Select(k => new { k.UrunId, k.Urun.Kod, k.Urun.Ad, k.Urun.Fiyat, k.Adet })
            .ToListAsync(ct);

        var dto = kalemler
            .Select(k => new SepetKalemiDto(k.UrunId, k.Kod, k.Ad, k.Fiyat, k.Adet, k.Fiyat * k.Adet))
            .ToList();
        return new SepetDto(dto, dto.Sum(k => k.AraToplam));
    }

    public async Task<SepetSonuc> UrunEkleAsync(Guid kullaniciId, SepeteEkleRequest istek, CancellationToken ct = default)
    {
        var urun = await _db.Urunler.FirstOrDefaultAsync(u => u.Id == istek.UrunId, ct);
        if (urun is null)
            return new SepetSonuc(SepetHata.UrunBulunamadi);
        if (!urun.AktifMi)
            return new SepetSonuc(SepetHata.UrunPasif);

        var kalem = await _db.SepetKalemleri
            .FirstOrDefaultAsync(k => k.KullaniciId == kullaniciId && k.UrunId == urun.Id, ct);

        // Sepetteki adet ile yeni adet birlikte stoğu aşamaz.
        var yeniAdet = (kalem?.Adet ?? 0) + istek.Adet;
        if (yeniAdet > urun.Stok)
            return new SepetSonuc(SepetHata.StokYetersiz, MevcutStok: urun.Stok);

        if (kalem is null)
        {
            _db.SepetKalemleri.Add(new SepetKalemi
            {
                KullaniciId = kullaniciId,
                UrunId = urun.Id,
                Adet = yeniAdet,
                EklenmeTarihi = DateTime.UtcNow
            });
        }
        else
        {
            kalem.Adet = yeniAdet;
        }

        await _db.SaveChangesAsync(ct);
        return new SepetSonuc(SepetHata.Yok, await GetirAsync(kullaniciId, ct));
    }

    public async Task<SepetSonuc> UrunCikarAsync(Guid kullaniciId, Guid urunId, CancellationToken ct = default)
    {
        var kalem = await _db.SepetKalemleri
            .FirstOrDefaultAsync(k => k.KullaniciId == kullaniciId && k.UrunId == urunId, ct);
        if (kalem is null)
            return new SepetSonuc(SepetHata.SepetteYok);

        _db.SepetKalemleri.Remove(kalem);
        await _db.SaveChangesAsync(ct);
        return new SepetSonuc(SepetHata.Yok, await GetirAsync(kullaniciId, ct));
    }
}
