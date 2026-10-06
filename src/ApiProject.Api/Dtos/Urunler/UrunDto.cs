namespace ApiProject.Api.Dtos;

public record UrunDto(Guid Id, string Kod, string Ad, string? Aciklama, decimal Fiyat, int Stok, bool AktifMi);
