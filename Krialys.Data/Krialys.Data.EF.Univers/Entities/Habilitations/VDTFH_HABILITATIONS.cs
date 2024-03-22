using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;

public partial class VDTFH_HABILITATIONS
{
    [Key]
    [Display(Name = "Display_VDTFH_HABILITATIONS_TRU_USERID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(36)]
    public string TRU_USERID { get; set; }

    [Key]
    [Display(Name = "Display_VDTFH_HABILITATIONS_TS_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public int TS_SCENARIOID { get; set; }

    public int PRODUCTEUR { get; set; }
    public int CONTREMAITRE { get; set; }
    public int CONTROLEUR { get; set; }

    [ForeignKey(nameof(TRU_USERID))]
    [InverseProperty(nameof(TRU_USERS.VDTFH_HABILITATION))]
    public virtual TRU_USERS TRU_USER { get; set; }

    [ForeignKey(nameof(TS_SCENARIOID))]
    [InverseProperty(nameof(TS_SCENARIOS.VDTFH_HABILITATION))]
    public virtual TS_SCENARIOS TS_SCENARIO { get; set; }
}