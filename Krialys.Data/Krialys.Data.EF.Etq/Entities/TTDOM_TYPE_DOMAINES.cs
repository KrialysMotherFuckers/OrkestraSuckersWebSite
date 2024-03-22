using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Krialys.Data.EF.Etq;

[Index(nameof(TTDOM_CODE), Name = "UK_TTDOM_TYPE_DOMAINES", IsUnique = true)]
[DisplayColumn("TTDOM_LIB")]
public partial class TTDOM_TYPE_DOMAINES
{
    public TTDOM_TYPE_DOMAINES()
    {
        TDOM_DOMAINES = new HashSet<TDOM_DOMAINES>();
    }

    [Key]

    [Display(Name = "Display_TTDOM_TYPE_DOMAINES_TTDOM_TYPE_DOMAINEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TTDOM_TYPE_DOMAINEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(20)]
    [Display(Name = "Display_TTDOM_TYPE_DOMAINES_TTDOM_CODE", ResourceType = typeof(DataAnnotationsResources))]
    public string TTDOM_CODE { get; set; }

    [StringLength(100)]
    [Display(Name = "Display_TTDOM_TYPE_DOMAINES_TTDOM_LIB", ResourceType = typeof(DataAnnotationsResources))]
    public string TTDOM_LIB { get; set; }

    [StringLength(250)]
    [Display(Name = "Display_TTDOM_TYPE_DOMAINES_TTDOM_DESC", ResourceType = typeof(DataAnnotationsResources))]
    public string TTDOM_DESC { get; set; }

    [InverseProperty("TTDOM_TYPE_DOMAINES")]
    public virtual ICollection<TDOM_DOMAINES> TDOM_DOMAINES { get; set; }
}