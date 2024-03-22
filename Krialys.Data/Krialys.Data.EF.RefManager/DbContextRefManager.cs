using Krialys.Entities.COMMON;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.RefManager;

/// <summary>
/// DbContext for RefManager entities
/// </summary>
public partial class KrialysDbContext : DbContext
{
    public KrialysDbContext() { }
    public KrialysDbContext(DbContextOptions<KrialysDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite("DataSource=../../../ApiUnivers/App_Data/Database/db-RefManager.db3");
    }

    public virtual DbSet<TR_CNX_Connections> TR_CNX_Connections { get; set; }
    public virtual DbSet<TM_RFS_ReferentialSettings> TM_RFS_ReferentialSettings { get; set; }
    public virtual DbSet<TX_RFX_ReferentialSettingsData> TX_RFX_ReferentialSettingsData { get; set; }
    public virtual DbSet<TM_RFH_ReferentialHistorical> TM_RFH_ReferentialHistorical { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

        modelBuilder.ApplyAllConfigurations<KrialysDbContext>();

        base.OnModelCreating(modelBuilder);
    }
}