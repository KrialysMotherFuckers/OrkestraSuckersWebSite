using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;

[Index(nameof(TSG_NOM), Name = "UQ_TSG_SCENARIO_GPES_TSG_NOM", IsUnique = true)]
[DisplayColumn("TSG_NOM")]
public partial class TSG_SCENARIO_GPES
{
    public TSG_SCENARIO_GPES()
    {
        TSGA_SCENARIO_GPE_ASSOCIE = new HashSet<TSGA_SCENARIO_GPE_ASSOCIES>();
        TH_HABILITATION = new HashSet<TH_HABILITATIONS>();
    }

    [Display(Name = "Display_TSG_SCENARIO_GPES_TSG_SCENARIO_GPEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Key]
    public int TSG_SCENARIO_GPEID { get; set; }

    [Display(Name = "Display_TSG_SCENARIO_GPES_TSG_NOM", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(30)]
    public string TSG_NOM { get; set; }

    [Display(Name = "Display_TSG_SCENARIO_GPES_TSG_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TSG_COMMENTAIRE { get; set; }

    [Display(Name = "Display_TSG_SCENARIO_GPES_TSG_DESCR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [StringLength(255)]
    public string TSG_DESCR { get; set; }

    [Display(Name = "Display_TSG_SCENARIO_GPES_TSG_DATE_MAJ", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TSG_DATE_MAJ { get; set; }

    [Display(Name = "Display_TSG_SCENARIO_GPES_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [Required]
    [StringLength(3)]
    public string TRST_STATUTID { get; set; }

    [InverseProperty(nameof(TSGA_SCENARIO_GPE_ASSOCIES.TSG_SCENARIO_GPE))]
    public virtual ICollection<TSGA_SCENARIO_GPE_ASSOCIES> TSGA_SCENARIO_GPE_ASSOCIE { get; set; }


    [InverseProperty(nameof(TH_HABILITATIONS.TSG_SCENARIO_GPE))]
    public virtual ICollection<TH_HABILITATIONS> TH_HABILITATION { get; set; }


    [ForeignKey(nameof(TRST_STATUTID))]
    [InverseProperty(nameof(TRST_STATUTS.TSG_SCENARIO_GPE))]
    public virtual TRST_STATUTS TRST_STATUT { get; set; }
}