using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Krialys.Data.EF.Etq;

//[Index(nameof(TETQ_ETIQUETTEID), nameof(TRGLRV_REGLES_VALEURID), Name = "UK_TETQR_ETQ_REGLES", IsUnique = true)]
[Index(nameof(TETQ_ETIQUETTEID), nameof(TRGL_REGLEID), Name = "UK_TETQR_ETQ_REGLES", IsUnique = true)]

public partial class TETQR_ETQ_REGLES
{
    public TETQR_ETQ_REGLES()
    {
    }

    [Key]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_ETQ_REGLEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TETQR_ETQ_REGLEID { get; set; }


    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQ_ETIQUETTEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TETQ_ETIQUETTEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURID", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGLRV_REGLES_VALEURID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TRGL_REGLEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGL_REGLEID { get; set; }

    //[Required]
    //[StringLength(50)]
    ////     [Display(Name = "Display_TETQR_ETQ_REGLES_TOBJR_VALEUR", ResourceType = typeof(DataAnnotationsResources))]
    //public string TETQR_VALEUR { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_ECHEANCE", ResourceType = typeof(DataAnnotationsResources))]
    public DateTime? TETQR_ECHEANCE { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_DATE_DEBUT", ResourceType = typeof(DataAnnotationsResources))]
    public DateTime? TETQR_DATE_DEBUT { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_DATE_FIN", ResourceType = typeof(DataAnnotationsResources))]
    public DateTime? TETQR_DATE_FIN { get; set; }


    [StringLength(1)]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_LIMITE_ATTEINTE", ResourceType = typeof(DataAnnotationsResources))]
    public string TETQR_LIMITE_ATTEINTE { get; set; }

    [StringLength(50)]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_ETQ_REGLES_ACTION", ResourceType = typeof(DataAnnotationsResources))]
    public string TETQR_ETQ_REGLES_ACTION { get; set; }

    [StringLength(10)]
    [Display(Name = "Display_TETQR_ETQ_REGLES_TETQR_REGLE_LIEE", ResourceType = typeof(DataAnnotationsResources))]
    public string TETQR_REGLE_LIEE { get; set; }

    //ForeignKey

    [ForeignKey(nameof(TRGLRV_REGLES_VALEURID))]
    [InverseProperty(nameof(TRGLRV_REGLES_VALEURS.TETQR_ETQ_REGLESS))]
    public virtual TRGLRV_REGLES_VALEURS TRGLRV_REGLES_VALEUR { get; set; }

    [ForeignKey(nameof(TETQ_ETIQUETTEID))]
    [InverseProperty(nameof(TETQ_ETIQUETTES.TETQR_ETQ_REGLES))]
    public virtual TETQ_ETIQUETTES TETQ_ETIQUETTE { get; set; }

    [ForeignKey(nameof(TRGL_REGLEID))]
    [InverseProperty(nameof(TRGL_REGLES.TETQR_ETQ_REGLES))]
    public virtual TRGL_REGLES TRGL_REGLE { get; set; }

}