using Krialys.Common.Literals;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Etq;

[Index(nameof(aet_etiquette_id), nameof(aet_user_id), Name = "IDX_ETQ_TM_AET_Authorization", IsUnique = true)]
public partial class ETQ_TM_AET_Authorization
{
    [Key]
    public int aet_id { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_etiquette_id", ResourceType = typeof(DataAnnotationsResources))]
    [Column(TypeName = "int")]
    public int aet_etiquette_id { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_user_id", ResourceType = typeof(DataAnnotationsResources))]
    public string aet_user_id { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_status_id", ResourceType = typeof(DataAnnotationsResources))]
    public string aet_status_id { get; set; } = StatusLiteral.Available;

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_initializing_user_id", ResourceType = typeof(DataAnnotationsResources))]
    public string aet_initializing_user_id { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_initializing_date", ResourceType = typeof(DataAnnotationsResources))]
    [Column(TypeName = "datetime")]
    public DateTime aet_initializing_date { get; set; }

    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_comments", ResourceType = typeof(DataAnnotationsResources))]
    [StringLength(255)]
    public string aet_comments { get; set; }

    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_update_by", ResourceType = typeof(DataAnnotationsResources))]
    public string aet_update_by { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_ETQ_TM_AET_Authorization_aet_update_date", ResourceType = typeof(DataAnnotationsResources))]
    [Column(TypeName = "datetime")]
    public DateTime? aet_update_date { get; set; }


    [ForeignKey(nameof(aet_etiquette_id))]
    [InverseProperty(nameof(TETQ_ETIQUETTES.ETQ_TM_AET_Authorization))]
    public virtual TETQ_ETIQUETTES TETQ_ETIQUETTE { get; set; }
}
