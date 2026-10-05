using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Data;

/// <summary>
/// Test için örnek veri. İki firmada da "admin" kullanıcısı var, şifreleri farklı:
///   ABC / admin / Abc123!
///   XYZ / admin / Xyz123!
/// </summary>
public static class SeedData
{
    private static readonly DateTime Tarih = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Uygula(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firma>().HasData(
            new Firma { Id = 1, FirmaKodu = "ABC", FirmaAdi = "ABC Teknoloji", AktifMi = true, OlusturmaTarihi = Tarih },
            new Firma { Id = 2, FirmaKodu = "XYZ", FirmaAdi = "XYZ Lojistik", AktifMi = true, OlusturmaTarihi = Tarih });

        modelBuilder.Entity<FirmaDomain>().HasData(
            new FirmaDomain { Id = 1, FirmaId = 1, Domain = "abc.localhost", VarsayilanMi = true },
            new FirmaDomain { Id = 2, FirmaId = 1, Domain = "abc.ornek.com", VarsayilanMi = false },
            new FirmaDomain { Id = 3, FirmaId = 2, Domain = "xyz.localhost", VarsayilanMi = true },
            new FirmaDomain { Id = 4, FirmaId = 2, Domain = "xyz.ornek.com", VarsayilanMi = false });

        modelBuilder.Entity<FirmaAyar>().HasData(
            new FirmaAyar
            {
                FirmaId = 1, LogoUrl = "https://abc.ornek.com/logo.png", TemaRengi = "#1E88E5",
                Dil = "tr-TR", ZamanDilimi = "Europe/Istanbul",
                EkAyarlarJson = "{\"destekTelefonu\":\"0212 000 00 00\",\"modulStok\":true}",
                GuncellemeTarihi = Tarih
            },
            new FirmaAyar
            {
                FirmaId = 2, LogoUrl = "https://xyz.ornek.com/logo.png", TemaRengi = "#43A047",
                Dil = "en-US", ZamanDilimi = "Europe/London",
                EkAyarlarJson = "{\"destekTelefonu\":\"+44 20 0000 0000\",\"modulStok\":false}",
                GuncellemeTarihi = Tarih
            });

        // Hash'ler PasswordHasher<Kullanici> ile üretildi (seed sabit olmalı, o yüzden önceden hesaplandı).
        modelBuilder.Entity<Kullanici>().HasData(
            new Kullanici
            {
                Id = 1, FirmaId = 1, KullaniciAdi = "admin", NormalizeKullaniciAdi = "ADMIN",
                SifreHash = "AQAAAAIAAYagAAAAEK7v3g7bOp36PP3Pt7XkS2ojtYQEV+xmJhHfv4IlNTvFfB/NvtBp/7YvrsT7oR7ZmA==",
                Email = "admin@abc.ornek.com", AdSoyad = "ABC Yönetici", AktifMi = true, OlusturmaTarihi = Tarih
            },
            new Kullanici
            {
                Id = 2, FirmaId = 2, KullaniciAdi = "admin", NormalizeKullaniciAdi = "ADMIN",
                SifreHash = "AQAAAAIAAYagAAAAEHmK/9vv7Kzqx3rpwssF1OXu0reqQ5Q7zWSuv1u86VH45HQ5xgEQh6RrkJKJ9gjVzg==",
                Email = "admin@xyz.ornek.com", AdSoyad = "XYZ Yönetici", AktifMi = true, OlusturmaTarihi = Tarih
            });
    }
}
