using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using ApiProject.Api.Entities;
using ApiProject.Api.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Services;

public enum KayitHata
{
    Yok,
    FirmaBelirlenemedi,
    KullaniciAdiAlinmis
}

public record KayitSonuc(KayitHata Hata, KullaniciDto? Kullanici = null);

public class KullaniciService
{
    private readonly AppDbContext _db;
    private readonly IFirmaBaglami _firmaBaglami;
    private readonly IPasswordHasher<Kullanici> _hasher;

    public KullaniciService(AppDbContext db, IFirmaBaglami firmaBaglami, IPasswordHasher<Kullanici> hasher)
    {
        _db = db;
        _firmaBaglami = firmaBaglami;
        _hasher = hasher;
    }

    public async Task<KayitSonuc> KayitOlAsync(KullaniciKayitRequest istek, CancellationToken ct = default)
    {
        if (!_firmaBaglami.CozulduMu)
            return new KayitSonuc(KayitHata.FirmaBelirlenemedi);

        // Firma filtresi sayesinde sadece bu firmadaki kullanıcılar kontrol edilir,
        // aynı ad başka firmada olsa bile kayıt açılabilir.
        var kullaniciAdi = istek.KullaniciAdi.Trim();
        var normalize = kullaniciAdi.ToUpperInvariant();
        if (await _db.Kullanicilar.AnyAsync(k => k.NormalizeKullaniciAdi == normalize, ct))
            return new KayitSonuc(KayitHata.KullaniciAdiAlinmis);

        var kullanici = new Kullanici
        {
            KullaniciAdi = kullaniciAdi,
            NormalizeKullaniciAdi = normalize,
            Email = istek.Email?.Trim(),
            AdSoyad = istek.AdSoyad?.Trim(),
            AktifMi = true,
            OlusturmaTarihi = DateTime.UtcNow
        };
        kullanici.SifreHash = _hasher.HashPassword(kullanici, istek.Sifre);
        _db.Kullanicilar.Add(kullanici);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Aynı anda gelen iki kayıt isteğinde unique index ikincisini durdurur.
            return new KayitSonuc(KayitHata.KullaniciAdiAlinmis);
        }

        return new KayitSonuc(KayitHata.Yok, new KullaniciDto(
            kullanici.Id, kullanici.KullaniciAdi, kullanici.Email, kullanici.AdSoyad, kullanici.AktifMi, kullanici.YoneticiMi, kullanici.OlusturmaTarihi));
    }
}
