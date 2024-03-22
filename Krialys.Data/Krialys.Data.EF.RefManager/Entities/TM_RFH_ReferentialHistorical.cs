using Krialys.Data.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.RefManager;

[Table("TX_RFH_ReferentialHistorical")]
public class TM_RFH_ReferentialHistorical
{
    [Key]
    [Column("rfh_id")]
    public int Rfh_id { get; set; }

    [Column("rfs_id")]
    public int Rfs_id { get; set; }

    [Column("rfh_table_name")]
    [Display(Name = "Display_REF_Historical_table_name", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_TableName { get; set; }

    [Column("rfh_table_functional_name")]
    [Display(Name = "Display_REF_Historical_table_functional_name", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_TableFunctionalName { get; set; }

    [Column("rfh_description")]
    [Display(Name = "Display_REF_Historical_table_description", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_Description { get; set; }

    [Column("rfh_status_code")]
    [Display(Name = "Display_REF_Historical_status_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_StatusCode { get; set; }

    [Column("rfh_update_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_REF_Historical_update_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? Rfh_UpdateDate { get; set; }

    [Column("rfh_update_by")]
    [Display(Name = "Display_REF_Historical_update_by", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_UpdateBy { get; set; }

    [Column("rfh_process_type")]
    [Display(Name = "Display_REF_Historical_process_type", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_ProcessType { get; set; }

    [Column("rfh_process_status")]
    [Display(Name = "Display_REF_Historical_process_status", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_ProcessStatus { get; set; }

    [Column("rfh_label_code")]
    [Display(Name = "Display_REF_Historical_label_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_LabelCode { get; set; }

    [Column("rfh_label_code_generated")]
    [Display(Name = "Display_REF_Historical_label_code_generated", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_LabelCodeGenerated { get; set; }

    [Column("rfh_request_id", TypeName = "int")]
    [Display(Name = "Display_REF_Settings_request_id", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? Rfh_RequestId { get; set; }

    [Column("rfh_treatment_id")]
    public string Rfh_TreatmentId { get; set; }

    [Column("rfh_error_message")]
    [Display(Name = "Display_REF_Settings_error_message", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfh_ErrorMessage { get; set; }

    [ForeignKey(nameof(Rfs_id))]
    public virtual TM_RFS_ReferentialSettings TM_RFS_ReferentialSettings { get; set; }
}