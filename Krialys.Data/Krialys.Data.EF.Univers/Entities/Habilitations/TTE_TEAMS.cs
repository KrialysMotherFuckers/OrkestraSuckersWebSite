using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;

[Index(nameof(TTE_NOM), Name = "UQ_TTE_TEAMS_TTE_NOM", IsUnique = true)]
[DisplayColumn("TTE_NOM")]
public partial class TTE_TEAMS
{
    public TTE_TEAMS()
    {
        TUTE_USER_TEAMS = new HashSet<TUTE_USER_TEAMS>();
        TH_HABILITATIONS = new HashSet<TH_HABILITATIONS>();
    }

    [Display(Name = "Display_TTE_TEAMS_TTE_TEAMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Key]
    public int TTE_TEAMID { get; set; }

    [Display(Name = "Display_TTE_TEAMS_TTE_NOM", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(30)]
    public string TTE_NOM { get; set; }

    [Display(Name = "Display_TTE_TEAMS_TTE_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TTE_COMMENTAIRE { get; set; }

    [Display(Name = "Display_TTE_TEAMS_TTE_DESCR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TTE_DESCR { get; set; }

    [Display(Name = "Display_TTE_TEAMS_TTE_DATE_CREATION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TTE_DATE_CREATION { get; set; }

    [Display(Name = "Display_TTE_TEAMS_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(3)]
    public string TRST_STATUTID { get; set; }

    [InverseProperty("TTE_TEAM")]
    public virtual ICollection<TUTE_USER_TEAMS> TUTE_USER_TEAMS { get; set; }

    [InverseProperty("TTE_TEAM")]
    public virtual ICollection<TH_HABILITATIONS> TH_HABILITATIONS { get; set; }


    [ForeignKey(nameof(TRST_STATUTID))]
    [InverseProperty(nameof(TRST_STATUTS.TTE_TEAM))]
    public virtual TRST_STATUTS TRST_STATUT { get; set; }
}