using Krialys.Data.EF.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Créer les modèles dans le répertoire "Entities"
/// NB : Les classes EF seront à placer ici
/// NB : Les champs NotMapped sont des champs (virtuels) calculés dans Sqlite
/// </summary>
namespace Krialys.Data.EF.Logs;

//[Page(MaxTop = 100)]
public partial class TM_LOG_Logs
{
    [Key]
    [Display(Name = "log_id")]
    [Column("log_id")]
    public int Log_Id { get; set; }

    [Editable(false)]
    [DisplayFormat(DataFormatString = "G")]
    [DefaultDateTimeUTCAttribute]
    [Column("log_creation_date")]
    public DateTime Log_CreationDate { get; set; }

    [Editable(false)]
    [StringLength(10)]
    [Column("log_type")]
    public string Log_Type { get; set; }

    [Editable(false)]
    [Column("log_exception")]
    public string Log_Exception { get; set; }

    [Editable(false)]
    [Column("log_message")]
    public string Log_Message { get; set; }

    [Editable(false)]
    [Column("log_message_details")]
    public string Log_MessageDetails { get; set; }

    [NotMapped] // Virtual column, contains: 'STD', 'CPU' or 'ETL' from RenderedMessage
    [Column("log_origin")]
    public string Log_Origin { get; set; }

    [NotMapped] // Virtual column, contains: DemandeId extracted EtlLogException[0] or CpuLogException
    [Column("req_id")]
    public int? Req_Id { get; set; }
}