using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Services;

// Yönetici firmasının tüm siparişlerini ve kullanıcılarını görür; firma filtresi sayesinde başka firmanınkileri göremez.
public class YonetimService
{
    private readonly AppDbContext _db;

    public YonetimService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<SiparisDto>> SiparisleriGetirAsync(CancellationToken ct = default)
    {
        return _db.Siparisler
            .AsNoTracking()
            .OrderByDescending(s => s.OlusturmaTarihi)
            .Select(SiparisService.SiparisDtoYap)
            .ToListAsync(ct);
    }

    public Task<List<KullaniciDto>> KullanicilariGetirAsync(CancellationToken ct = default)
    {
        return _db.Kullanicilar
            .AsNoTracking()
            .OrderBy(k => k.KullaniciAdi)
            .Select(k => new KullaniciDto(k.Id, k.KullaniciAdi, k.Email, k.AdSoyad, k.AktifMi, k.YoneticiMi, k.OlusturmaTarihi))
            .ToListAsync(ct);
    }
}
