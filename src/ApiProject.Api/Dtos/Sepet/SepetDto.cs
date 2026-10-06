namespace ApiProject.Api.Dtos;

public record SepetKalemiDto(Guid UrunId, string UrunKodu, string UrunAdi, decimal BirimFiyat, int Adet, decimal AraToplam);

public record SepetDto(List<SepetKalemiDto> Kalemler, decimal Toplam);
