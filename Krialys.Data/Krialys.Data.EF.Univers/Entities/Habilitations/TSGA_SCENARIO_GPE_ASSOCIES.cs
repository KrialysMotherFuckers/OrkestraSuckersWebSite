using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;

[Index(nameof(TSG_SCENARIO_GPEID), nameof(TS_SCENARIOID), Name = "UQ_TSGA_SCENARIO_GPE_ASSOCIES", IsUnique = true)]
public partial class TSGA_SCENARIO_GPE_ASSOCIES
{
    [Display(Name = "Display_TSGA_SCENARIO_GPE_ASSOCIES_TSGA_SCENARIO_GPE_ASSOCIEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Key]
    public int TSGA_SCENARIO_GPE_ASSOCIEID { get; set; }

    [Display(Name = "Display_TSGA_SCENARIO_GPE_ASSOCIES_TSG_SCENARIO_GPEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public int TSG_SCENARIO_GPEID { get; set; }

    [Display(Name = "Display_TSGA_SCENARIO_GPE_ASSOCIES_TS_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    public int TS_SCENARIOID { get; set; }

    [Display(Name = "Display_TSGA_SCENARIO_GPE_ASSOCIES_TSGA_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TSGA_COMMENTAIRE { get; set; }

    [Display(Name = "Display_TSGA_SCENARIO_GPE_ASSOCIES_TSGA_DATE_CREATION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TSGA_DATE_CREATION { get; set; }

    [Display(Name = "Display_TSGA_SCENARIO_GPE_ASSOCIES_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(3)]
    public string TRST_STATUTID { get; set; }

    [ForeignKey(nameof(TSG_SCENARIO_GPEID))]
    [InverseProperty(nameof(TSG_SCENARIO_GPES.TSGA_SCENARIO_GPE_ASSOCIE))]
    public virtual TSG_SCENARIO_GPES TSG_SCENARIO_GPE { get; set; }

    [ForeignKey(nameof(TS_SCENARIOID))]
    [InverseProperty(nameof(TS_SCENARIOS.TSGA_SCENARIO_GPE_ASSOCIE))]
    public virtual TS_SCENARIOS TS_SCENARIO { get; set; }

    [ForeignKey(nameof(TRST_STATUTID))]
    [InverseProperty(nameof(TRST_STATUTS.TSGA_SCENARIO_GPE_ASSOCIE))]
    public virtual TRST_STATUTS TRST_STATUT { get; set; }
}