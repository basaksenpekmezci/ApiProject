using System.Reflection;
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
    public DbSet<Urun> Urunler => Set<Urun>();

    private Guid? AktifFirmaId => _firmaBaglami.FirmaId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // IFirmayaAit olan her tabloya firma filtresi eklenir. Firma belirlenmemişse hiçbir kayıt dönmez.
        foreach (var tablo in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IFirmayaAit).IsAssignableFrom(tablo.ClrType))
            {
                FirmaFiltresiMetodu.MakeGenericMethod(tablo.ClrType).Invoke(this, [modelBuilder]);
            }
        }

        if (Database.IsSqlServer())
        {
            modelBuilder.Entity<FirmaAyar>().ToTable(t =>
                t.HasCheckConstraint("CK_FirmaAyar_EkAyarlarJson", "[EkAyarlarJson] IS NULL OR ISJSON([EkAyarlarJson]) = 1"));
        }

        SeedData.Uygula(modelBuilder);
    }

    private static readonly MethodInfo FirmaFiltresiMetodu =
        typeof(AppDbContext).GetMethod(nameof(FirmaFiltresiEkle), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private void FirmaFiltresiEkle<T>(ModelBuilder modelBuilder) where T : class, IFirmayaAit
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => x.FirmaId == AktifFirmaId);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        FirmaBilgisiniKontrolEt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        FirmaBilgisiniKontrolEt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Yeni kayda aktif firma yazılır; başka firmaya ait bir kaydın eklenmesi veya değiştirilmesi engellenir.
    private void FirmaBilgisiniKontrolEt()
    {
        foreach (var kayit in ChangeTracker.Entries<IFirmayaAit>())
        {
            if (kayit.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            if (AktifFirmaId is null)
                throw new InvalidOperationException("Firma belirlenmeden firmaya ait kayıt değiştirilemez.");

            if (kayit.State == EntityState.Added)
                kayit.Entity.FirmaId = AktifFirmaId.Value;
            else if (kayit.Entity.FirmaId != AktifFirmaId.Value)
                throw new InvalidOperationException("Başka firmaya ait kayıt değiştirilemez.");
        }
    }
}
