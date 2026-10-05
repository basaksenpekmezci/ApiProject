using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class FirmaConfiguration : IEntityTypeConfiguration<Firma>
{
    public void Configure(EntityTypeBuilder<Firma> b)
    {
        b.ToTable("Firma");
        b.HasKey(x => x.Id);
        b.Property(x => x.FirmaKodu).HasMaxLength(50).IsUnicode(false).IsRequired();
        b.Property(x => x.FirmaAdi).HasMaxLength(200).IsRequired();
        b.Property(x => x.AktifMi).HasDefaultValue(true).ValueGeneratedNever();
        b.HasIndex(x => x.FirmaKodu).IsUnique();
    }
}
