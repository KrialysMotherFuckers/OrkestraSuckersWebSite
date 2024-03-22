using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;

[Index(nameof(TRU_USERID), nameof(TTE_TEAMID), Name = "UQ_TUTE_USER_TEAMS", IsUnique = true)]
public partial class TUTE_USER_TEAMS
{
    [Display(Name = "Display_TUTE_USER_TEAMS_TUTE_USER_TEAMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Key]
    public int TUTE_USER_TEAMID { get; set; }

    [Display(Name = "Display_TUTE_USER_TEAMS_TRU_USERID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(36)]
    public string TRU_USERID { get; set; }

    [Display(Name = "Display_TUTE_USER_TEAMS_TTE_TEAMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public int TTE_TEAMID { get; set; }

    [Display(Name = "Display_TUTE_USER_TEAMS_TUTE_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TUTE_COMMENTAIRE { get; set; }

    [Display(Name = "Display_TUTE_USER_TEAMS_TUTE_DATE_MAJ", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TUTE_DATE_MAJ { get; set; }

    [Display(Name = "Display_TUTE_USER_TEAMS_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(3)]
    public string TRST_STATUTID { get; set; }

    [ForeignKey(nameof(TRU_USERID))]
    [InverseProperty(nameof(TRU_USERS.TUTE_USER_TEAM))]
    public virtual TRU_USERS TRU_USER { get; set; }

    [ForeignKey(nameof(TTE_TEAMID))]
    [InverseProperty(nameof(TTE_TEAMS.TUTE_USER_TEAMS))]
    public virtual TTE_TEAMS TTE_TEAM { get; set; }

    [ForeignKey(nameof(TRST_STATUTID))]
    [InverseProperty(nameof(TRST_STATUTS.TUTE_USER_TEAM))]
    public virtual TRST_STATUTS TRST_STATUT { get; set; }
}