using Krialys.Entities.COMMON;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.FileStorage;

/// <summary>
/// DbContext for FileStorage entities
/// </summary>
public partial class KrialysDbContext : DbContext
{
    #region Constructor

    public KrialysDbContext() { }
    public KrialysDbContext(DbContextOptions<KrialysDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite("DataSource=../../../ApiUnivers/App_Data/Database/db-FileStorage.db3");
    }

    #endregion Constructor

    #region Tables

    public virtual DbSet<TR_SCT_StreamCategoryType> TR_SCT_StreamCategoryType { get; set; }
    public virtual DbSet<TM_STF_StorageFileRequest> TM_STF_StorageFileRequest { get; set; }

    #endregion Tables

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyAllConfigurations<KrialysDbContext>();

        base.OnModelCreating(modelBuilder);
    }
}