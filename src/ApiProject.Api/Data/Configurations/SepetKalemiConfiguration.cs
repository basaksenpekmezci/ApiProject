using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class SepetKalemiConfiguration : IEntityTypeConfiguration<SepetKalemi>
{
    public void Configure(EntityTypeBuilder<SepetKalemi> b)
    {
        b.ToTable("SepetKalemi");
        b.HasKey(x => x.Id);

        // Bir ürün sepette tek satırdır, tekrar eklenince adedi artar.
        b.HasIndex(x => new { x.FirmaId, x.KullaniciId, x.UrunId }).IsUnique();

        b.HasOne<Firma>()
            .WithMany()
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Kullanici)
            .WithMany()
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Urun)
            .WithMany()
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
