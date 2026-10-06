using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class UrunConfiguration : IEntityTypeConfiguration<Urun>
{
    public void Configure(EntityTypeBuilder<Urun> b)
    {
        b.ToTable("Urun");
        b.HasKey(x => x.Id);
        b.Property(x => x.Kod).HasMaxLength(50).IsRequired();
        b.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        b.Property(x => x.Aciklama).HasMaxLength(1000);
        b.Property(x => x.Fiyat).HasPrecision(18, 2);
        b.Property(x => x.AktifMi).HasDefaultValue(true).ValueGeneratedNever();

        // Ürün kodu firma içinde benzersiz, farklı firmalarda aynı kod olabilir.
        b.HasIndex(x => new { x.FirmaId, x.Kod }).IsUnique();
        b.HasIndex(x => new { x.FirmaId, x.Ad });

        b.HasOne(x => x.Firma)
            .WithMany()
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
