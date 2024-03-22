using Krialys.Data.EF.Attributes;
using Krialys.Data.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.RefManager;

public partial class TR_CNX_Connections
{
    [Key]
    [Column("cnx_id")]
    public int Cnx_Id { get; set; }

    [Required]
    [Column("cnx_code")]
    [Display(Name = "Display_REF_DbConnection_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Cnx_Code { get; set; }

    [Required]
    [Column("cnx_label")]
    [Display(Name = "Display_REF_DbConnection_label", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Cnx_Label { get; set; }

    [Required]
    [Column("cnx_database_type")]
    [Display(Name = "Display_REF_DbConnection_database_type", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int Cnx_DatabaseType { get; set; }

    [Required]
    [Column("cnx_server_name")]
    [Display(Name = "Display_REF_DbConnection_server", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Cnx_ServerName { get; set; }

    [Required]
    [Column("cnx_port")]
    [Display(Name = "Display_REF_DbConnection_server_port", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int Cnx_Port { get; set; }

    [Required]
    [Column("cnx_database_name")]
    [Display(Name = "Display_REF_DbConnection_database_name", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Cnx_DatabaseName { get; set; }

    [Required]
    [Column("cnx_login")]
    [Display(Name = "Display_REF_DbConnection_login", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Cnx_Login { get; set; }

    [Required]
    [Column("cnx_password")]
    [Display(Name = "Display_REF_DbConnection_password", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string Cnx_Password { get; set; }

    [Required]
    [Column("cnx_creation_date", TypeName = "datetime")]
    //[DefaultDateTimeUTC]
    [Display(Name = "Display_REF_DbConnection_creation_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime Cnx_CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("cnx_is_active", TypeName = "boolean")]
    [DefaultValue(false)]
    [Display(Name = "Display_REF_DbConnection_is_active", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public bool Cnx_IsActive { get; set; }

    //public virtual ICollection<TM_RFS_ReferentialSettings> TM_RFS_ReferentialSettings { get; set; }
}
