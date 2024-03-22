using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Krialys.Data.EF.Etq;

[Index(nameof(TDOM_CODE), Name = "UK_TDOM_DOMAINES", IsUnique = true)]
[DisplayColumn("TDOM_CODE")]
public partial class TDOM_DOMAINES
{
    public TDOM_DOMAINES()
    {
        TOBJE_OBJET_ETIQUETTES = new HashSet<TOBJE_OBJET_ETIQUETTES>();
        TPRCP_PRC_PERIMETRES = new HashSet<TPRCP_PRC_PERIMETRES>();
    }

    public override bool Equals(object obj)
    {
        return obj is TDOM_DOMAINES item && this.TDOM_DOMAINEID.Equals(item.TDOM_DOMAINEID);
    }

    public override int GetHashCode()
    {
        return this.TDOM_DOMAINEID.GetHashCode();
    }

    [Key]
    public int TDOM_DOMAINEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(3, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 1)]
    [Display(Name = "Display_TDOM_DOMAINES_TDOM_CODE", ResourceType = typeof(DataAnnotationsResources))]
    public string TDOM_CODE { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(100, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 1)]
    [Display(Name = "Display_TDOM_DOMAINES_TDOM_LIB", ResourceType = typeof(DataAnnotationsResources))]
    public string TDOM_LIB { get; set; }

    [StringLength(250)]
    [Display(Name = "Display_TDOM_DOMAINES_TDOM_DESC", ResourceType = typeof(DataAnnotationsResources))]
    public string TDOM_DESC { get; set; }

    [Display(Name = "Display_TDOM_DOMAINES_TTDOM_TYPE_DOMAINEID", ResourceType = typeof(DataAnnotationsResources))]
    public int? TTDOM_TYPE_DOMAINEID { get; set; }


    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TDOM_DOMAINES_TDOM_DATE_CREATION", ResourceType = typeof(DataAnnotationsResources))]
    public DateTime TDOM_DATE_CREATION { get; set; }


    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [StringLength(36)]
    [Display(Name = "Display_TDOM_DOMAINES_TRU_ACTEURID", ResourceType = typeof(DataAnnotationsResources))]
    public string TRU_ACTEURID { get; set; } = ""; // OB-367: please use a 'default' value, this avoids exception when used as a 'foreign key'

    [ForeignKey(nameof(TTDOM_TYPE_DOMAINEID))]
    [InverseProperty("TDOM_DOMAINES")]
    public virtual TTDOM_TYPE_DOMAINES TTDOM_TYPE_DOMAINES { get; set; }


    [InverseProperty("TDOM_DOMAINE")]
    public virtual ICollection<TOBJE_OBJET_ETIQUETTES> TOBJE_OBJET_ETIQUETTES { get; set; }

    [InverseProperty("TDOM_DOMAINES")]
    public virtual ICollection<TPRCP_PRC_PERIMETRES> TPRCP_PRC_PERIMETRES { get; set; }
}