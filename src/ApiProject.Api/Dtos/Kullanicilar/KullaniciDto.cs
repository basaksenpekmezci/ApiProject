namespace ApiProject.Api.Dtos;

public record KullaniciDto(Guid Id, string KullaniciAdi, string? Email, string? AdSoyad, bool AktifMi, bool YoneticiMi, DateTime OlusturmaTarihi);
