﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Krialys.Data.EF.Etq
{
    [Index(nameof(TETQ_CODE), Name = "UK_TETQ_ETIQUETTES", IsUnique = true)]
    [DisplayColumn("TETQ_CODE")]
    public partial class TETQ_ETIQUETTES
    {
        public TETQ_ETIQUETTES()
        {
            TSEQ_SUIVI_EVENEMENT_ETQS = new HashSet<TSEQ_SUIVI_EVENEMENT_ETQS>();
            TSR_SUIVI_RESSOURCES = new HashSet<TSR_SUIVI_RESSOURCES>();
            TETQR_ETQ_REGLES = new HashSet<TETQR_ETQ_REGLES>();
            ETQ_TM_AET_Authorization = new HashSet<ETQ_TM_AET_Authorization>();
        }


        [Key]
        public int TETQ_ETIQUETTEID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(60)]
        [Display(Name = "Display_TETQ_ETIQUETTES_TETQ_CODE", ResourceType = typeof(DataAnnotationsResources))]
        public string TETQ_CODE { get; set; }

        [Display(Name = "Display_TETQ_ETIQUETTES_TOBJE_OBJET_ETIQUETTEID", ResourceType = typeof(DataAnnotationsResources))]
        public int TOBJE_OBJET_ETIQUETTEID { get; set; }

        [StringLength(100)]
        [MaxLength(100)]
        [Display(Name = "Display_TETQ_ETIQUETTES_TETQ_LIB", ResourceType = typeof(DataAnnotationsResources))]
        public string TETQ_LIB { get; set; }

        [StringLength(250)]
        [MaxLength(250)]
        [Display(Name = "Display_TETQ_ETIQUETTES_TETQ_DESC", ResourceType = typeof(DataAnnotationsResources))]
        public string TETQ_DESC { get; set; }

        [Display(Name = "Display_TETQ_ETIQUETTES_TPRCP_PRC_PERIMETREID", ResourceType = typeof(DataAnnotationsResources))]
        public int? TPRCP_PRC_PERIMETREID { get; set; }

        [StringLength(32)]
        [Display(Name = "Display_TETQ_ETIQUETTES_TETQ_PRM_VAL", ResourceType = typeof(DataAnnotationsResources))]
        public string TETQ_PRM_VAL { get; set; }

        [Display(Name = "Display_TETQ_ETIQUETTES_DEMANDEID", ResourceType = typeof(DataAnnotationsResources))]
        public int? DEMANDEID { get; set; }

        [Display(Name = "Display_TETQ_ETIQUETTES_TETQ_VERSION_ETQ", ResourceType = typeof(DataAnnotationsResources))]
        public int? TETQ_VERSION_ETQ { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TETQ_ETIQUETTES_TETQ_DATE_CREATION", ResourceType = typeof(DataAnnotationsResources))]
        public DateTime TETQ_DATE_CREATION { get; set; }

        [Display(Name = "Display_TETQ_ETIQUETTES_etq_is_public_access", ResourceType = typeof(DataAnnotationsResources))]
        [Column(TypeName = "boolean")]
        [DefaultValue(false)]
        public bool etq_is_public_access { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [ScaffoldColumn(false)]
        [Editable(false)]
        public string TETQ_PATTERN { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [ScaffoldColumn(false)]
        [Editable(false)]
        public int TETQ_INCREMENT { get; set; }

        [ForeignKey(nameof(TOBJE_OBJET_ETIQUETTEID))]
        [InverseProperty(nameof(TOBJE_OBJET_ETIQUETTES.TETQ_ETIQUETTES))]
        public virtual TOBJE_OBJET_ETIQUETTES TOBJE_OBJET_ETIQUETTE { get; set; }

        [ForeignKey(nameof(TPRCP_PRC_PERIMETREID))]
        [InverseProperty(nameof(TPRCP_PRC_PERIMETRES.TETQ_ETIQUETTES))]
        public virtual TPRCP_PRC_PERIMETRES TPRCP_PRC_PERIMETRE { get; set; }

        [InverseProperty("TETQ_ETIQUETTE")]
        public virtual ICollection<TSEQ_SUIVI_EVENEMENT_ETQS> TSEQ_SUIVI_EVENEMENT_ETQS { get; set; }

        [InverseProperty("TETQ_ETIQUETTE")]
        public virtual ICollection<TSR_SUIVI_RESSOURCES> TSR_SUIVI_RESSOURCES { get; set; }


        [InverseProperty("TETQ_ETIQUETTE")]
        public virtual ICollection<TETQR_ETQ_REGLES> TETQR_ETQ_REGLES { get; set; }

        [InverseProperty("TETQ_ETIQUETTE")]
        public virtual ICollection<ETQ_TM_AET_Authorization> ETQ_TM_AET_Authorization { get; set; }
    }
}