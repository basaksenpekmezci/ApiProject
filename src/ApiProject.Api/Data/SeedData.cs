using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Data;

public static class SeedData
{
    private static readonly DateTime Tarih = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // HasData sabit değer istediği için örnek kayıtların ID'leri elle verildi.
    public static readonly Guid AbcFirmaId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid XyzFirmaId = Guid.Parse("10000000-0000-0000-0000-000000000002");

    public static void Uygula(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firma>().HasData(
            new Firma { Id = AbcFirmaId, FirmaKodu = "ABC", FirmaAdi = "ABC Teknoloji", AktifMi = true, OlusturmaTarihi = Tarih },
            new Firma { Id = XyzFirmaId, FirmaKodu = "XYZ", FirmaAdi = "XYZ Lojistik", AktifMi = true, OlusturmaTarihi = Tarih });

        modelBuilder.Entity<FirmaDomain>().HasData(
            new FirmaDomain { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), FirmaId = AbcFirmaId, Domain = "abc.localhost", VarsayilanMi = true },
            new FirmaDomain { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), FirmaId = AbcFirmaId, Domain = "abc.ornek.com", VarsayilanMi = false },
            new FirmaDomain { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), FirmaId = XyzFirmaId, Domain = "xyz.localhost", VarsayilanMi = true },
            new FirmaDomain { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), FirmaId = XyzFirmaId, Domain = "xyz.ornek.com", VarsayilanMi = false });

        modelBuilder.Entity<FirmaAyar>().HasData(
            new FirmaAyar
            {
                FirmaId = AbcFirmaId, LogoUrl = "https://abc.ornek.com/logo.png", TemaRengi = "#1E88E5",
                Dil = "tr-TR", ZamanDilimi = "Europe/Istanbul",
                EkAyarlarJson = "{\"destekTelefonu\":\"0212 000 00 00\",\"modulStok\":true}",
                GuncellemeTarihi = Tarih
            },
            new FirmaAyar
            {
                FirmaId = XyzFirmaId, LogoUrl = "https://xyz.ornek.com/logo.png", TemaRengi = "#43A047",
                Dil = "en-US", ZamanDilimi = "Europe/London",
                EkAyarlarJson = "{\"destekTelefonu\":\"+44 20 0000 0000\",\"modulStok\":false}",
                GuncellemeTarihi = Tarih
            });

        // HasData sabit değer istediği için hash'ler önceden PasswordHasher ile üretildi.
        modelBuilder.Entity<Kullanici>().HasData(
            new Kullanici
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), FirmaId = AbcFirmaId, KullaniciAdi = "admin", NormalizeKullaniciAdi = "ADMIN",
                SifreHash = "AQAAAAIAAYagAAAAEK7v3g7bOp36PP3Pt7XkS2ojtYQEV+xmJhHfv4IlNTvFfB/NvtBp/7YvrsT7oR7ZmA==",
                Email = "admin@abc.ornek.com", AdSoyad = "ABC Yönetici", AktifMi = true, OlusturmaTarihi = Tarih
            },
            new Kullanici
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), FirmaId = XyzFirmaId, KullaniciAdi = "admin", NormalizeKullaniciAdi = "ADMIN",
                SifreHash = "AQAAAAIAAYagAAAAEHmK/9vv7Kzqx3rpwssF1OXu0reqQ5Q7zWSuv1u86VH45HQ5xgEQh6RrkJKJ9gjVzg==",
                Email = "admin@xyz.ornek.com", AdSoyad = "XYZ Yönetici", AktifMi = true, OlusturmaTarihi = Tarih
            });
    }
}
