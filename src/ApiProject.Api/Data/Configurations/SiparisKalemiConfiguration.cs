using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class SiparisKalemiConfiguration : IEntityTypeConfiguration<SiparisKalemi>
{
    public void Configure(EntityTypeBuilder<SiparisKalemi> b)
    {
        b.ToTable("SiparisKalemi");
        b.HasKey(x => x.Id);
        b.Property(x => x.UrunKodu).HasMaxLength(50).IsRequired();
        b.Property(x => x.UrunAdi).HasMaxLength(200).IsRequired();
        b.Property(x => x.BirimFiyat).HasPrecision(18, 2);

        b.HasOne<Firma>()
            .WithMany()
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Siparis)
            .WithMany(s => s.Kalemler)
            .HasForeignKey(x => x.SiparisId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Urun)
            .WithMany()
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
