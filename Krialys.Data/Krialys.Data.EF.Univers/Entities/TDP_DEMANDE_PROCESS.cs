using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers;


public partial class TDP_DEMANDE_PROCESS
{

    [Key]
    public int TDP_DEMANDE_PROCESSID { get; set; }

    public int TD_DEMANDEID { get; set; }

    public int TDP_NUM_ETAPE { get; set; }
    [StringLength(30)]
    public string TDP_ETAPE { get; set; }

    [StringLength(2)]
    public string TDP_STATUT { get; set; }

    [StringLength(30)]
    public string TDP_EXTRA_INFO { get; set; }

    [ForeignKey(nameof(TD_DEMANDEID))]
    [InverseProperty(nameof(TD_DEMANDES.TDP_DEMANDE_PROCESS))]
    public virtual TD_DEMANDES TD_DEMANDE { get; set; }
}