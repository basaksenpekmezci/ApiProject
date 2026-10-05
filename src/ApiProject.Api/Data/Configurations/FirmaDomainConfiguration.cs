using ApiProject.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiProject.Api.Data.Configurations;

public class FirmaDomainConfiguration : IEntityTypeConfiguration<FirmaDomain>
{
    public void Configure(EntityTypeBuilder<FirmaDomain> b)
    {
        b.ToTable("FirmaDomain");
        b.HasKey(x => x.Id);
        b.Property(x => x.Domain).HasMaxLength(253).IsUnicode(false).IsRequired();
        b.HasIndex(x => x.Domain).IsUnique();
        b.HasOne(x => x.Firma)
            .WithMany(f => f.Domainler)
            .HasForeignKey(x => x.FirmaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
