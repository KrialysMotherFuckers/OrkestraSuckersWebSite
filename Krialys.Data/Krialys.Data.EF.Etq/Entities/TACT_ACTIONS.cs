using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Krialys.Data.EF.Etq;

[Index(nameof(TACT_CODE), Name = "UK_TACT_CODE", IsUnique = true)]
[DisplayColumn("TACT_LIB")]
public partial class TACT_ACTIONS
{
    public TACT_ACTIONS()
    {
        TRGLRV_REGLES_VALEURS = new HashSet<TRGLRV_REGLES_VALEURS>();
    }

    [Key]
    public int TACT_ACTIONID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(15)]
    [Display(Name = "Display_TACT_ACTIONS_TACT_CODE", ResourceType = typeof(DataAnnotationsResources))]
    public string TACT_CODE { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(50)]
    [Display(Name = "Display_TACT_ACTIONS_TACT_LIB", ResourceType = typeof(DataAnnotationsResources))]
    public string TACT_LIB { get; set; }

    [StringLength(100)]
    [Display(Name = "Display_TACT_ACTIONS_TACT_DESC", ResourceType = typeof(DataAnnotationsResources))]
    public string TACT_DESC { get; set; }


    [InverseProperty("TACT_ACTION")]
    public virtual ICollection<TRGLRV_REGLES_VALEURS> TRGLRV_REGLES_VALEURS { get; set; }

}