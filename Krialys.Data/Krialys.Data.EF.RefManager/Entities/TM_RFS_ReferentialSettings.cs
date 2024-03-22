using Krialys.Common.Extensions;
using Krialys.Data.EF.Resources;
using Krialys.Data.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.RefManager;

[Table("TM_RFS_ReferentialSettings")]
public class TM_RFS_ReferentialSettings
{
    public TM_RFS_ReferentialSettings()
    {
        TR_CNX_Connections = new TR_CNX_Connections();
        TX_RFX_ReferentialSettingsData = new TX_RFX_ReferentialSettingsData();
    }

    [Key]
    [Column("rfs_id")]
    public int Rfs_id { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Column("rfs_table_name")]
    [Display(Name = "Display_REF_Settings_table_name", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableName { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Column("rfs_table_functional_name")]
    [Display(Name = "Display_REF_Settings_table_functional_name", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableFunctionalName { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Column("cnx_id")]
    [Display(Name = "Display_REF_Settings_connection", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int Cnx_Id { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Column("rfs_table_schema")]
    [Display(Name = "Display_REF_Settings_schema", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableSchema { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Column("rfs_table_query_select")]
    [Display(Name = "Display_REF_Settings_table_query_select", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQuerySelect { get; set; }

    [Column("rfs_table_query_insert")]
    [Display(Name = "Display_REF_Settings_table_query_insert", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQueryInsert { get; set; }

    [Column("rfs_table_query_delete")]
    [Display(Name = "Display_REF_Settings_table_query_delete", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQueryDelete { get; set; }

    [Column("rfs_table_query_update")]
    [Display(Name = "Display_REF_Settings_table_query_update", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQueryUpdate { get; set; }

    [Column("rfs_table_query_update_columns")]
    [Display(Name = "Display_REF_Settings_table_query_update_columns", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQueryUpdateColumns { get; set; } // in the Talend job the values are expected as a string[]

    [Column("rfs_table_query_update_keys")]
    [Display(Name = "Display_REF_Settings_table_query_update_keys", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQueryUpdateKeys { get; set; } // in the Talend job the values are expected as a string[]

    [Column("rfs_table_query_criteria")]
    [Display(Name = "Display_REF_Settings_table_query_criteria", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableQueryCriteria { get; set; }

    [Column("rfs_description")]
    [Display(Name = "Display_REF_Settings_table_description", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_Description { get; set; }

    [Column("rfs_table_typology", TypeName = "int")]
    [Display(Name = "Display_REF_Settings_table_typology", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public RefManagerTypologyType Rfs_TableTypology { get; set; }

    [Column("rfs_scenario_id", TypeName = "int")]
    [Display(Name = "Display_REF_Settings_scenario_id", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? Rfs_ScenarioId { get; set; }

    [Column("rfs_param_label_object_code")]
    [Display(Name = "Display_REF_Settings_label_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_ParamLabelObjectCode { get; set; }

    [Column("rfs_label_code_fieldname")]
    [Display(Name = "Display_REF_Settings_label_code_field_name", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_LabelCodeFieldName { get; set; }

    [Column("rfs_documentation")]
    [Display(Name = "Display_REF_Settings_Documentation", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_Documentation { get; set; }

    [Column("rfs_manager_id")]
    [Display(Name = "Display_REF_Settings_data_stewart", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_ManagerId { get; set; }    

    [Column("rfs_label_data_cloned_in_progress_list_json")]
    [Display(Name = "Display_REF_Settings_data_cloned_in_progress_list", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_LabelDataClonedInProgressListJson { get; set; }

    [Column("rfs_table_data_to_approuve", TypeName = "boolean")]
    [Display(Name = "Display_REF_Settings_table_data_to_approuve", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public bool Rfs_TableDataToApprouve { get; set; }

    [Column("rfs_table_data_approved", TypeName = "boolean")]
    [DefaultValue(false)]
    [Display(Name = "Display_REF_Settings_table_data_approved", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public bool Rfs_TableDataApproved { get; set; }

    [Column("rfs_table_data_approval_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_REF_Settings_table_data_approval_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? Rfs_TableDataApprovalDate { get; set; }

    [Column("rfs_table_data_approved_by", TypeName = "datetime")]
    [Display(Name = "Display_REF_Settings_table_data_approved_by", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_TableDataApprovedBy { get; set; }

    [Column("rfs_table_data_need_to_be_refreshed", TypeName = "boolean")]
    [DefaultValue(false)]
    [Display(Name = "Display_REF_Settings_table_data_need_to_refreshed", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public bool Rfs_TableDataNeedToBeRefreshed { get; set; }

    [Column("rfs_last_refresh_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_REF_Settings_table_last_refresh_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? Rfs_LastRefreshDate { get; set; }

    [Column("rfs_is_update_sent_to_gdb", TypeName = "boolean")]
    [DefaultValue(false)]
    [Display(Name = "Display_REF_Settings_is_update_send_to_gdb", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public bool Rfs_IsUpdateSentToGdb { get; set; }

    [Column("rfs_send_date_to_gdb", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_REF_Settings_send_to_gdb_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? Rfs_SendDateToGdb { get; set; }

    [Column("rfs_is_backup_needed", TypeName = "boolean")]
    [DefaultValue(false)]
    [Display(Name = "Display_REF_Settings_is_backup_needed", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public bool Rfs_IsBackupNeeded { get; set; }   

    [Column("rfs_table_data_max_rows_expected_to_receive")]
    [Display(Name = "Display_REF_Settings_table_data_max_rows_expected_to_receive", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? Rfs_TableDataMaxRowsExpectedToReceive { get; set; }

    [Column("rfs_table_data_min_rows_expected_to_send")]
    [Display(Name = "Display_REF_Settings_table_data_min_rows_expected_to_send", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? Rfs_TableDataMinRowsExpectedToSend { get; set; }

    [Column("rfs_table_data_max_rows_expected_to_send")]
    [Display(Name = "Display_REF_Settings_table_data_max_rows_expected_to_send", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? Rfs_TableDataMaxRowsExpectedToSend { get; set; }

    [Column("rfs_status_code")]
    [Display(Name = "Display_REF_Settings_status_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_StatusCode { get; set; }

    [Required]
    [Column("rfs_creation_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_REF_Settings_creation_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime Rfs_CreationDate { get; set; } = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1));

    [Column("rfs_created_by")]
    [Display(Name = "Display_REF_Settings_created_by", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_CreatedBy { get; set; }

    [Column("rfs_update_date", TypeName = "datetime")]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_REF_Settings_update_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? Rfs_UpdateDate { get; set; }

    [Column("rfs_update_by")]
    [Display(Name = "Display_REF_Settings_update_by", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Rfs_UpdateBy { get; set; }

    [ForeignKey(nameof(Cnx_Id))]
    public virtual TR_CNX_Connections TR_CNX_Connections { get; set; }

    public virtual TX_RFX_ReferentialSettingsData TX_RFX_ReferentialSettingsData { get; set; }
}