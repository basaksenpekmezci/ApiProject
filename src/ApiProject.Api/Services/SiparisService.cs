using System.Linq.Expressions;
using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Services;

public enum SiparisHata
{
    Yok,
    SepetBos,
    UrunPasif,
    StokYetersiz
}

public record SiparisSonuc(SiparisHata Hata, SiparisDto? Siparis = null, string? UrunAdi = null);

// Sorgular firma filtresinden geçer; kullanıcı ayrıca sadece kendi sepetini ve siparişlerini görür.
public class SiparisService
{
    private readonly AppDbContext _db;

    public SiparisService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SiparisSonuc> SepettenOlusturAsync(Guid kullaniciId, CancellationToken ct = default)
    {
        var sepet = await _db.SepetKalemleri
            .Include(k => k.Urun)
            .Where(k => k.KullaniciId == kullaniciId)
            .ToListAsync(ct);
        if (sepet.Count == 0)
            return new SiparisSonuc(SiparisHata.SepetBos);

        // Ürün sepete eklendikten sonra pasif olmuş ya da stoğu azalmış olabilir.
        foreach (var kalem in sepet)
        {
            if (!kalem.Urun.AktifMi)
                return new SiparisSonuc(SiparisHata.UrunPasif, UrunAdi: kalem.Urun.Ad);
            if (kalem.Adet > kalem.Urun.Stok)
                return new SiparisSonuc(SiparisHata.StokYetersiz, UrunAdi: kalem.Urun.Ad);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        // Stok, "yeterliyse düş" şeklinde tek bir UPDATE ile azaltılır. Aynı anda gelen iki sipariş
        // aynı stoğu kullanmaya çalışırsa, stok yetmeyen sipariş satır güncelleyemez ve geri alınır.
        foreach (var kalem in sepet)
        {
            var guncellenen = await _db.Urunler
                .Where(u => u.Id == kalem.UrunId && u.AktifMi && u.Stok >= kalem.Adet)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Stok, u => u.Stok - kalem.Adet), ct);
            if (guncellenen == 0)
            {
                await transaction.RollbackAsync(ct);
                return new SiparisSonuc(SiparisHata.StokYetersiz, UrunAdi: kalem.Urun.Ad);
            }
        }

        var siparis = new Siparis
        {
            KullaniciId = kullaniciId,
            OlusturmaTarihi = DateTime.UtcNow,
            Kalemler = sepet.Select(k => new SiparisKalemi
            {
                UrunId = k.UrunId,
                UrunKodu = k.Urun.Kod,
                UrunAdi = k.Urun.Ad,
                BirimFiyat = k.Urun.Fiyat,
                Adet = k.Adet
            }).ToList()
        };
        siparis.ToplamTutar = siparis.Kalemler.Sum(k => k.BirimFiyat * k.Adet);

        _db.Siparisler.Add(siparis);
        _db.SepetKalemleri.RemoveRange(sepet);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var dto = await _db.Siparisler.AsNoTracking().Where(s => s.Id == siparis.Id).Select(SiparisDtoYap).FirstAsync(ct);
        return new SiparisSonuc(SiparisHata.Yok, dto);
    }

    public Task<List<SiparisDto>> KullanicininSiparisleriAsync(Guid kullaniciId, CancellationToken ct = default)
    {
        return _db.Siparisler
            .AsNoTracking()
            .Where(s => s.KullaniciId == kullaniciId)
            .OrderByDescending(s => s.OlusturmaTarihi)
            .Select(SiparisDtoYap)
            .ToListAsync(ct);
    }

    public static readonly Expression<Func<Siparis, SiparisDto>> SiparisDtoYap = s => new SiparisDto(
        s.Id,
        s.OlusturmaTarihi,
        s.Kullanici.KullaniciAdi,
        s.ToplamTutar,
        s.Kalemler
            .OrderBy(k => k.UrunAdi)
            .Select(k => new SiparisKalemiDto(k.UrunId, k.UrunKodu, k.UrunAdi, k.BirimFiyat, k.Adet, k.BirimFiyat * k.Adet))
            .ToList());
}
