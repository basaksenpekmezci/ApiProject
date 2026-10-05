using ApiProject.Api.Data;
using ApiProject.Api.Dtos;
using ApiProject.Api.Entities;
using ApiProject.Api.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Services;

public enum LoginHata
{
    Yok,
    FirmaKoduGerekli,
    FirmaKoduDomainleUyusmuyor,
    GecersizBilgi
}

public record LoginSonuc(LoginHata Hata, LoginResponse? Yanit = null);

public class AuthService
{
    private const int MaksHataliGiris = 5;
    private static readonly TimeSpan KilitSuresi = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;
    private readonly IFirmaBaglami _firmaBaglami;
    private readonly FirmaService _firmaService;
    private readonly JwtTokenService _jwt;
    private readonly IPasswordHasher<Kullanici> _hasher;

    public AuthService(AppDbContext db, IFirmaBaglami firmaBaglami, FirmaService firmaService,
        JwtTokenService jwt, IPasswordHasher<Kullanici> hasher)
    {
        _db = db;
        _firmaBaglami = firmaBaglami;
        _firmaService = firmaService;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<LoginSonuc> LoginAsync(LoginRequest istek, CancellationToken ct = default)
    {
        var govdeKodu = string.IsNullOrWhiteSpace(istek.FirmaKodu) ? null : istek.FirmaKodu.Trim();

        if (_firmaBaglami.CozulduMu)
        {
            // Firma domainden bulundu; body'de farklı bir firma kodu gelirse reddet.
            if (govdeKodu is not null && !string.Equals(govdeKodu, _firmaBaglami.FirmaKodu, StringComparison.OrdinalIgnoreCase))
                return new LoginSonuc(LoginHata.FirmaKoduDomainleUyusmuyor);
        }
        else
        {
            if (govdeKodu is null)
                return new LoginSonuc(LoginHata.FirmaKoduGerekli);

            var firma = await _firmaService.KoddanBulAsync(govdeKodu, ct);
            if (firma is null)
                return new LoginSonuc(LoginHata.GecersizBilgi);

            _firmaBaglami.Ayarla(firma.Id, firma.FirmaKodu);
        }

        // Global query filter kullanıcıyı otomatik olarak bu firmayla sınırlar.
        var normalize = istek.KullaniciAdi.Trim().ToUpperInvariant();
        var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.NormalizeKullaniciAdi == normalize, ct);
        if (kullanici is null || !kullanici.AktifMi)
            return new LoginSonuc(LoginHata.GecersizBilgi);

        var simdi = DateTime.UtcNow;
        if (kullanici.KilitBitisTarihi > simdi)
            return new LoginSonuc(LoginHata.GecersizBilgi);

        var sonuc = _hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, istek.Sifre);
        if (sonuc == PasswordVerificationResult.Failed)
        {
            kullanici.HataliGirisSayisi++;
            if (kullanici.HataliGirisSayisi >= MaksHataliGiris)
            {
                kullanici.KilitBitisTarihi = simdi.Add(KilitSuresi);
                kullanici.HataliGirisSayisi = 0;
            }
            await _db.SaveChangesAsync(ct);
            return new LoginSonuc(LoginHata.GecersizBilgi);
        }

        if (sonuc == PasswordVerificationResult.SuccessRehashNeeded)
            kullanici.SifreHash = _hasher.HashPassword(kullanici, istek.Sifre);

        kullanici.HataliGirisSayisi = 0;
        kullanici.KilitBitisTarihi = null;
        kullanici.SonGirisTarihi = simdi;
        await _db.SaveChangesAsync(ct);

        var (token, bitis) = _jwt.Uret(kullanici, _firmaBaglami.FirmaKodu!);
        return new LoginSonuc(LoginHata.Yok, new LoginResponse(token, bitis, _firmaBaglami.FirmaKodu!, kullanici.KullaniciAdi));
    }
}
