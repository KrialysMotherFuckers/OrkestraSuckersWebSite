using Krialys.Entities.COMMON;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Krialys.Data.EF.Logs;

/// <summary>
/// DbContext for LOGS entities
/// </summary>
public partial class KrialysDbContext : DbContext
{
    #region Constructor

    public KrialysDbContext() { }
    public KrialysDbContext(DbContextOptions<KrialysDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite("DataSource=../../../ApiUnivers/App_Data/Database/db-Logs.db3");
    }

    #endregion Constructor

    #region Tables

    public virtual DbSet<TM_LOG_Logs> TM_LOG_Logs { get; set; }

    #endregion Tables

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //var dateTimeConverter = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));

        if (modelBuilder == null)
            throw new ArgumentNullException(nameof(modelBuilder));

        modelBuilder.ApplyAllConfigurations<KrialysDbContext>();

        _ = modelBuilder.Entity<TM_LOG_Logs>(entity =>
        {
            entity.HasKey(e => new { e.Log_Id })
                .HasName("log_id");

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p =>
                        p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));

                    foreach (var property in properties)
                    {
                        modelBuilder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }

            entity.Property(e => e.Log_Id).ValueGeneratedOnAdd();
        });

        base.OnModelCreating(modelBuilder);
    }
}