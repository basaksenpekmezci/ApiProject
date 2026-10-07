namespace ApiProject.Api.Arama;

// Elasticsearch'teki "urunler" index'inde tutulan ürün kaydı.
public class UrunDokumani
{
    public Guid FirmaId { get; set; }
    public Guid Id { get; set; }
    public string Kod { get; set; } = null!;
    public string Ad { get; set; } = null!;
    public string? Aciklama { get; set; }
    public decimal Fiyat { get; set; }
    public int Stok { get; set; }
    public bool AktifMi { get; set; }
}
