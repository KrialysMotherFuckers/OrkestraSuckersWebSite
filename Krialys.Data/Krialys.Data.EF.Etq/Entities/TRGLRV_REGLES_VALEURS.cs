using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Krialys.Data.EF.Etq;

[Index(nameof(TRGL_REGLEID), nameof(TRGLRV_VALEUR), Name = "UK_TRGLRV_REGLES_VALEURS", IsUnique = true)]

[DisplayColumn("TRGLRV_VALEUR")]
public partial class TRGLRV_REGLES_VALEURS
{
    public TRGLRV_REGLES_VALEURS()
    {
        TRGLI_REGLES_LIEES_SRC = new HashSet<TRGLI_REGLES_LIEES>();
        TRGLI_REGLES_LIEES_CIBLE = new HashSet<TRGLI_REGLES_LIEES>();
        TETQR_ETQ_REGLESS = new HashSet<TETQR_ETQ_REGLES>();
        TOBJR_OBJET_REGLE = new HashSet<TOBJR_OBJET_REGLES>();
    }

    [Key]
    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGLRV_REGLES_VALEURID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGL_REGLEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGL_REGLEID { get; set; }

    [StringLength(50)]
    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGLRV_VALEUR", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGLRV_VALEUR { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGLRV_ORDRE_AFFICHAGE", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGLRV_ORDRE_AFFICHAGE { get; set; }

    [StringLength(1)]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGLRV_DEPART_LIMITE_TEMPS", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGLRV_DEPART_LIMITE_TEMPS { get; set; }


    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TACT_ACTIONID", ResourceType = typeof(DataAnnotationsResources))]
    public int? TACT_ACTIONID { get; set; }

    [StringLength(1)]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGLRV_VALEUR_ECHEANCE", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGLRV_VALEUR_ECHEANCE { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(1)]
    [Display(Name = "Display_TRGLRV_REGLES_VALEURS_TRGLRV_VALEUR_DEFAUT", ResourceType = typeof(DataAnnotationsResources))]
    public string TRGLRV_VALEUR_DEFAUT { get; set; }


    //FOREIGN KEY

    [ForeignKey(nameof(TRGL_REGLEID))]
    [InverseProperty("TRGLRV_REGLES_VALEURS")]
    public virtual TRGL_REGLES TRGL_REGLE { get; set; }

    [ForeignKey(nameof(TACT_ACTIONID))]
    [InverseProperty(nameof(TACT_ACTIONS.TRGLRV_REGLES_VALEURS))]
    public virtual TACT_ACTIONS TACT_ACTION { get; set; }


    // NAVIGATION

    // la table TRGLI_REGLES_LIEES est fille de TRGLRV_REGLES_VALEUR
    // on y associe un couple de 2 regles de TRGLRV_REGLES_VALEUR
    [InverseProperty(nameof(TRGLI_REGLES_LIEES.TRGLRV_REGLES_VALEUR))]
    public virtual ICollection<TRGLI_REGLES_LIEES> TRGLI_REGLES_LIEES_SRC { get; set; }

    [InverseProperty(nameof(TRGLI_REGLES_LIEES.TRGLRV_REGLES_VALEUR_CIBLE))]
    public virtual ICollection<TRGLI_REGLES_LIEES> TRGLI_REGLES_LIEES_CIBLE { get; set; }


    [InverseProperty(nameof(TETQR_ETQ_REGLES.TRGLRV_REGLES_VALEUR))]
    public virtual ICollection<TETQR_ETQ_REGLES> TETQR_ETQ_REGLESS { get; set; }


    [InverseProperty(nameof(TOBJR_OBJET_REGLES.TRGLRV_REGLES_VALEUR))]
    public virtual ICollection<TOBJR_OBJET_REGLES> TOBJR_OBJET_REGLE { get; set; }


}