using Krialys.Data.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.RefManager;

[Table("TX_RFX_ReferentialSettingsData")]
public class TX_RFX_ReferentialSettingsData
{
    [Key]
    [Column("rfx_id")]
    public int Rfx_id { get; set; }

    [Column("rfs_id")]
    public int Rfs_id { get; set; }

    [Column("rfx_table_data")]
    public string Rfx_TableData { get; set; }

    [Column("rfx_table_metadata")]
    public string Rfx_TableMetadata { get; set; }

    [Column("rfx_table_data_update_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? Rfx_TableDataUpdateDate { get; set; }

    [Column("rfx_table_data_updated_by")]
    public string Rfx_TableDataUpdatedBy { get; set; }

    [Column("rfx_table_data_backup")]
    public string Rfx_TableDataBackup { get; set; }

    [Column("rfx_table_metadata_backup")]
    public string Rfx_TableMetadataBackup { get; set; }

    [Column("rfx_table_data_backup_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? Rfx_TableDataBackupDate { get; set; }

    [Column("rfx_table_data_backup_updated_by")]
    public string Rfx_TableDataBackupUpdatedBy { get; set; }

    [ForeignKey(nameof(Rfs_id))]
    public virtual TM_RFS_ReferentialSettings TM_RFS_ReferentialSettings { get; set; }
}