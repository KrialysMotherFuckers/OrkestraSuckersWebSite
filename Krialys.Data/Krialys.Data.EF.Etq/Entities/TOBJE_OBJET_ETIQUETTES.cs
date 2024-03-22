﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Krialys.Data.EF.Etq
{
    [Index(nameof(TOBJE_CODE), nameof(TOBJE_VERSION), Name = "UK_TOBJE_OBJET_ETIQUETTES", IsUnique = true)]
    [Index(nameof(TDOM_DOMAINEID), nameof(TOBJE_CODE_ETIQUETTAGE), nameof(TOBJE_VERSION), Name = "UK_TOBJE_OBJET_ETIQUETTESBIS", IsUnique = true)]

    [DisplayColumn("TOBJE_CODE")]
    public partial class TOBJE_OBJET_ETIQUETTES
    {
        public TOBJE_OBJET_ETIQUETTES()
        {
            TETQ_ETIQUETTES = new HashSet<TETQ_ETIQUETTES>();
            TOBJR_OBJET_REGLES = new HashSet<TOBJR_OBJET_REGLES>();
        }

        public override bool Equals(object obj)
        {
            var item = obj as TOBJE_OBJET_ETIQUETTES;

            if (item == null)
            {
                return false;
            }

            return this.TOBJE_OBJET_ETIQUETTEID.Equals(item.TOBJE_OBJET_ETIQUETTEID);
        }

        public override int GetHashCode()
        {
            return this.TOBJE_OBJET_ETIQUETTEID.GetHashCode();
        }

        [Key]
        public int TOBJE_OBJET_ETIQUETTEID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(40, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 1)]
        [Editable(false)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_CODE", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBJE_CODE { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [Editable(false)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_VERSION", ResourceType = typeof(DataAnnotationsResources))]
        public int? TOBJE_VERSION { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [Range(0, 2, ErrorMessageResourceName = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_VERSION_ETQ_STATUT_Range", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_VERSION_ETQ_STATUT", ResourceType = typeof(DataAnnotationsResources))]
        public int? TOBJE_VERSION_ETQ_STATUT { get; set; }

        [Required(ErrorMessageResourceName = "Display_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINEID_Required", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [Editable(false)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINEID", ResourceType = typeof(DataAnnotationsResources))]
        public int? TDOM_DOMAINEID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(6, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 1)]
        [RegularExpression(@"[a-zA-Z0-9]*$", ErrorMessageResourceName = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_CODE_ETIQUETTAGE_RegularExpression", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [Editable(false)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_CODE_ETIQUETTAGE", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBJE_CODE_ETIQUETTAGE { get; set; }


        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(100, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 1)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_LIB", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBJE_LIB { get; set; }


        [StringLength(250, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 0)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_DESC", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBJE_DESC { get; set; }

        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBF_OBJ_FORMATID", ResourceType = typeof(DataAnnotationsResources))]
        public int? TOBF_OBJ_FORMATID { get; set; }

        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBN_OBJ_NATUREID", ResourceType = typeof(DataAnnotationsResources))]
        public int? TOBN_OBJ_NATUREID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TEQC_ETQ_CODIFID", ResourceType = typeof(DataAnnotationsResources))]
        public int? TEQC_ETQ_CODIFID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "g")]
        [Editable(false)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_DATE_CREATION", ResourceType = typeof(DataAnnotationsResources))]
        public DateTime TOBJE_DATE_CREATION { get; set; }

        [StringLength(36)]
        [Editable(false)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TRU_ACTEURID", ResourceType = typeof(DataAnnotationsResources))]
        public string TRU_ACTEURID { get; set; } = ""; // OB-367: please use a 'default' value, this avoids exception when used as a 'foreign key'

        [StringLength(250, ErrorMessageResourceName = "MIN_MAX_LENGTH", ErrorMessageResourceType = typeof(DataAnnotationsResources), MinimumLength = 0)]
        [Display(Name = "Display_TOBJE_OBJET_ETIQUETTES_TOBJE_VERSION_ETQ_DESC", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBJE_VERSION_ETQ_DESC { get; set; }

        // FOREIGN KEY
        [ForeignKey(nameof(TEQC_ETQ_CODIFID))]
        [InverseProperty(nameof(TEQC_ETQ_CODIFS.TOBJE_OBJET_ETIQUETTES))]
        public virtual TEQC_ETQ_CODIFS TEQC_ETQ_CODIF { get; set; }

        [ForeignKey(nameof(TOBF_OBJ_FORMATID))]
        [InverseProperty(nameof(TOBF_OBJ_FORMATS.TOBJE_OBJET_ETIQUETTES))]
        public virtual TOBF_OBJ_FORMATS TOBF_OBJ_FORMAT { get; set; }

        [ForeignKey(nameof(TOBN_OBJ_NATUREID))]
        [InverseProperty(nameof(TOBN_OBJ_NATURES.TOBJE_OBJET_ETIQUETTES))]
        public virtual TOBN_OBJ_NATURES TOBN_OBJ_NATURE { get; set; }

        [ForeignKey(nameof(TDOM_DOMAINEID))]
        [InverseProperty(nameof(TDOM_DOMAINES.TOBJE_OBJET_ETIQUETTES))]
        public virtual TDOM_DOMAINES TDOM_DOMAINE { get; set; }


        //NAVIGATION
        [InverseProperty("TOBJE_OBJET_ETIQUETTE")]
        public virtual ICollection<TETQ_ETIQUETTES> TETQ_ETIQUETTES { get; set; }

        [InverseProperty("TOBJE_OBJET_ETIQUETTE")]
        public virtual ICollection<TOBJR_OBJET_REGLES> TOBJR_OBJET_REGLES { get; set; }

    }
}