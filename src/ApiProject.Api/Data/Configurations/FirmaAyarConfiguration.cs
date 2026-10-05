using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class FirmaAyarConfiguration : IEntityTypeConfiguration<FirmaAyar>
{
    public void Configure(EntityTypeBuilder<FirmaAyar> b)
    {
        b.ToTable("FirmaAyar");
        b.HasKey(x => x.FirmaId);
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.Property(x => x.TemaRengi).HasMaxLength(20).IsUnicode(false);
        b.Property(x => x.Dil).HasMaxLength(10).IsUnicode(false).HasDefaultValue("tr-TR");
        b.Property(x => x.ZamanDilimi).HasMaxLength(50).IsUnicode(false).HasDefaultValue("Europe/Istanbul");
        b.HasOne(x => x.Firma)
            .WithOne(f => f.Ayar)
            .HasForeignKey<FirmaAyar>(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
