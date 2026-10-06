namespace ApiProject.Api.Dtos;

public record LoginResponse(string Token, DateTime GecerlilikBitis, string FirmaKodu, string KullaniciAdi, bool YoneticiMi);
