using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Krialys.Data.EF.Etq;

[Index(nameof(TRGL_CODE_REGLE), Name = "UK_TRGL_REGLES", IsUnique = true)]
[DisplayColumn("TRGL_LIB_REGLE")]
public partial class TRGL_REGLES
{
    public TRGL_REGLES()
    {
        TRGLRV_REGLES_VALEURS = new HashSet<TRGLRV_REGLES_VALEURS>();
        TOBJR_OBJET_REGLES = new HashSet<TOBJR_OBJET_REGLES>();
        TETQR_ETQ_REGLES = new HashSet<TETQR_ETQ_REGLES>();
    }

    [Key]
    [Display(Name = "Display_TRGL_REGLES_TRGL_REGLEID", ResourceType = typeof(DataAnnotationsResources))]

    public int TRGL_REGLEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(10)]
    [Display(Name = "Display_TRGL_REGLES_TRGL_CODE_REGLE", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGL_CODE_REGLE { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(40)]
    [Display(Name = "Display_TRGL_REGLES_TRGL_LIB_REGLE", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGL_LIB_REGLE { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(100)]
    [Display(Name = "Display_TRGL_REGLES_TRGL_DESC_REGLE", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGL_DESC_REGLE { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(10)]
    [Display(Name = "Display_TRGL_REGLES_TRGL_LIMITE_TEMPS", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGL_LIMITE_TEMPS { get; set; }

    // NAVIGATION

    [InverseProperty("TRGL_REGLE")]
    public virtual ICollection<TRGLRV_REGLES_VALEURS> TRGLRV_REGLES_VALEURS { get; set; }

    [InverseProperty("TRGL_REGLES")]
    public virtual ICollection<TOBJR_OBJET_REGLES> TOBJR_OBJET_REGLES { get; set; }

    [InverseProperty("TRGL_REGLE")]
    public virtual ICollection<TETQR_ETQ_REGLES> TETQR_ETQ_REGLES { get; set; }

}