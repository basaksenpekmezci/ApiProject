using ApiProject.Api.Entities;
using ApiProject.Api.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace ApiProject.Api.Data;

public class AppDbContext : DbContext
{
    private readonly IFirmaBaglami _firmaBaglami;

    public AppDbContext(DbContextOptions<AppDbContext> options, IFirmaBaglami firmaBaglami)
        : base(options)
    {
        _firmaBaglami = firmaBaglami;
    }

    public DbSet<Firma> Firmalar => Set<Firma>();
    public DbSet<FirmaDomain> FirmaDomainleri => Set<FirmaDomain>();
    public DbSet<FirmaAyar> FirmaAyarlari => Set<FirmaAyar>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();

    private int? AktifFirmaId => _firmaBaglami.FirmaId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Firma belirlenmemişse hiçbir kullanıcı dönmez.
        modelBuilder.Entity<Kullanici>().HasQueryFilter(k => k.FirmaId == AktifFirmaId);

        if (Database.IsSqlServer())
        {
            modelBuilder.Entity<FirmaAyar>().ToTable(t =>
                t.HasCheckConstraint("CK_FirmaAyar_EkAyarlarJson", "[EkAyarlarJson] IS NULL OR ISJSON([EkAyarlarJson]) = 1"));
        }

        SeedData.Uygula(modelBuilder);
    }
}
