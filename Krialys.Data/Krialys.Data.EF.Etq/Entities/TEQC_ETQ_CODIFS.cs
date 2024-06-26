﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Krialys.Data.EF.Etq
{
    
    [Index(nameof(TEQC_CODE), Name = "UK_TEQC_ETQ_CODIFS", IsUnique = true)]
    [DisplayColumn("TEQC_CODE")]
    public partial class TEQC_ETQ_CODIFS
    {
        public TEQC_ETQ_CODIFS()
        {
            TOBJE_OBJET_ETIQUETTES = new HashSet<TOBJE_OBJET_ETIQUETTES>();
        }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_ETQ_CODIFID", ResourceType = typeof(DataAnnotationsResources))]
        [Key]
        public int TEQC_ETQ_CODIFID { get; set; }
     
       [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(20)]
        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_CODE", ResourceType = typeof(DataAnnotationsResources))]
        public string TEQC_CODE { get; set; }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_CODE_PRC_ORDRE", ResourceType = typeof(DataAnnotationsResources))]
        public int? TEQC_CODE_PRC_ORDRE { get; set; }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE", ResourceType = typeof(DataAnnotationsResources))]
        public int? TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE { get; set; }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_CODE_PRM_ORDRE", ResourceType = typeof(DataAnnotationsResources))]
        public int? TEQC_CODE_PRM_ORDRE { get; set; }
       
        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_INCREMENT_ORDRE", ResourceType = typeof(DataAnnotationsResources))]
        public int TEQC_INCREMENT_ORDRE { get; set; }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_INCREMENT_TAILLE", ResourceType = typeof(DataAnnotationsResources))]
        public int TEQC_INCREMENT_TAILLE { get; set; }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_INCREMENT_VAL_INIT", ResourceType = typeof(DataAnnotationsResources))]
        public int? TEQC_INCREMENT_VAL_INIT { get; set; }

        [Display(Name = "Display_TEQC_ETQ_CODIFS_TEQC_SEPARATEUR", ResourceType = typeof(DataAnnotationsResources))]
       [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(1)]
        public string TEQC_SEPARATEUR { get; set; }

        [InverseProperty("TEQC_ETQ_CODIF")]
        public virtual ICollection<TOBJE_OBJET_ETIQUETTES> TOBJE_OBJET_ETIQUETTES { get; set; }
    }
}