using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class SiparisConfiguration : IEntityTypeConfiguration<Siparis>
{
    public void Configure(EntityTypeBuilder<Siparis> b)
    {
        b.ToTable("Siparis");
        b.HasKey(x => x.Id);
        b.Property(x => x.ToplamTutar).HasPrecision(18, 2);
        b.HasIndex(x => new { x.FirmaId, x.KullaniciId });
        b.HasIndex(x => new { x.FirmaId, x.OlusturmaTarihi });

        b.HasOne<Firma>()
            .WithMany()
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Kullanici)
            .WithMany()
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
