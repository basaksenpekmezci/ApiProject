namespace ApiProject.Api.Dtos;

// Kaynak: "redis", "elasticsearch" veya "sql". Sonuclar en fazla 100 ürün içerir, ToplamKayit eşleşen tüm ürünlerin sayısıdır.
public record UrunAramaSonucu(List<UrunDto> Sonuclar, long ToplamKayit, long SureMs, string Kaynak);
