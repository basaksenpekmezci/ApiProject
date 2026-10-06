namespace ApiProject.Api.Dtos;

public record SiparisKalemiDto(Guid UrunId, string UrunKodu, string UrunAdi, decimal BirimFiyat, int Adet, decimal AraToplam);

public record SiparisDto(Guid Id, DateTime OlusturmaTarihi, string KullaniciAdi, decimal ToplamTutar, List<SiparisKalemiDto> Kalemler);
