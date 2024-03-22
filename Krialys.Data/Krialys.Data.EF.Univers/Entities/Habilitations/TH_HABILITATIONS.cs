using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;

[Index(nameof(TRU_USERID), nameof(TS_SCENARIOID), Name = "TH_HABILITATIONS_IDX1", IsUnique = false)]
public partial class TH_HABILITATIONS
{
    [Display(Name = "Display_TH_HABILITATIONS_TH_HABILITATIONID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Key]
    public int TH_HABILITATIONID { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TRU_USERID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(36)]
    public string TRU_USERID { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TTE_TEAMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTE_TEAMID { get; set; }


    [Display(Name = "Display_TH_HABILITATIONS_TS_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TS_SCENARIOID { get; set; }


    [Display(Name = "Display_TH_HABILITATIONS_TSG_SCENARIO_GPEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TSG_SCENARIO_GPEID { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(3)]
    public string TRST_STATUTID { get; set; }


    [Display(Name = "Display_TH_HABILITATIONS_TH_DROIT_CONCERNE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(20)]
    public string TH_DROIT_CONCERNE { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TH_EST_HABILITE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public int TH_EST_HABILITE { get; set; }


    [Display(Name = "Display_TH_HABILITATIONS_TH_DATE_INITIALISATION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    [Required]
    public DateTime TH_DATE_INITIALISATION { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TRU_INITIALISATION_AUTEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public string TRU_INITIALISATION_AUTEURID { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TH_MAJ_DATE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TH_MAJ_DATE { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TRU_MAJ_AUTEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public string TRU_MAJ_AUTEURID { get; set; }



    [Display(Name = "Display_TH_HABILITATIONS_TH_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TH_COMMENTAIRE { get; set; }

    [Display(Name = "Display_TH_HABILITATIONS_TH_HERITE_HABILITATIONID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TH_HERITE_HABILITATIONID { get; set; }



    [ForeignKey(nameof(TRU_USERID))]
    [InverseProperty(nameof(TRU_USERS.TH_HABILITATION))]
    public virtual TRU_USERS TRU_USER { get; set; }

    [ForeignKey(nameof(TTE_TEAMID))]
    [InverseProperty(nameof(TTE_TEAMS.TH_HABILITATIONS))]
    public virtual TTE_TEAMS TTE_TEAM { get; set; }


    [ForeignKey(nameof(TS_SCENARIOID))]
    [InverseProperty(nameof(TS_SCENARIOS.TH_HABILITATION))]
    public virtual TS_SCENARIOS TS_SCENARIO { get; set; }

    [ForeignKey(nameof(TSG_SCENARIO_GPEID))]
    [InverseProperty(nameof(TSG_SCENARIO_GPES.TH_HABILITATION))]
    public virtual TSG_SCENARIO_GPES TSG_SCENARIO_GPE { get; set; }

    [ForeignKey(nameof(TRST_STATUTID))]
    [InverseProperty(nameof(TRST_STATUTS.TH_HABILITATION))]
    public virtual TRST_STATUTS TRST_STATUT { get; set; }


    [ForeignKey(nameof(TRU_INITIALISATION_AUTEURID))]
    [InverseProperty(nameof(TRU_USERS.TRU_INITIALISATION_AUTEUR_TH_HABILITATION))]
    public virtual TRU_USERS TRU_INITIALISATION_AUTEUR { get; set; }

    [ForeignKey(nameof(TRU_MAJ_AUTEURID))]
    [InverseProperty(nameof(TRU_USERS.TRU_MAJ_AUTEUR_TH_HABILITATION))]
    public virtual TRU_USERS TRU_MAJ_AUTEUR { get; set; }

    public class ModeDroit
    {
        public static readonly string PRODUCTEUR = "PRODUCTEUR";
        public static readonly string CONTROLEUR = "CONTROLEUR";
        public static readonly string CONTREMAITRE = "CONTREMAITRE";


        public string Id { get; set; }
        public string Droit { get; set; }


        public IEnumerable<ModeDroit> DroitList()
        {
            List<ModeDroit> Veg = new List<ModeDroit>();
            Veg.Add(new ModeDroit { Id = CONTREMAITRE, Droit = CONTREMAITRE });
            Veg.Add(new ModeDroit { Id = CONTROLEUR, Droit = CONTROLEUR });
            Veg.Add(new ModeDroit { Id = PRODUCTEUR, Droit = PRODUCTEUR });
            return Veg;
        }


    }
}