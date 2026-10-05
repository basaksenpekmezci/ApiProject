using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> b)
    {
        b.ToTable("Kullanici");
        b.HasKey(x => x.Id);
        b.Property(x => x.KullaniciAdi).HasMaxLength(100).IsRequired();
        b.Property(x => x.NormalizeKullaniciAdi).HasMaxLength(100).IsRequired();
        b.Property(x => x.SifreHash).HasMaxLength(500).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256);
        b.Property(x => x.AdSoyad).HasMaxLength(200);
        b.Property(x => x.AktifMi).HasDefaultValue(true).ValueGeneratedNever();

        // Aynı kullanıcı adı farklı firmalarda olabilir, aynı firmada tekrar edemez.
        b.HasIndex(x => new { x.FirmaId, x.NormalizeKullaniciAdi }).IsUnique();

        b.HasOne(x => x.Firma)
            .WithMany()
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
