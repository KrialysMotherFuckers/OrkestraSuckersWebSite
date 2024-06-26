﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Krialys.Data.EF.Etq
{
    [Index(nameof(TOBN_CODE), Name = "UK_TOBN_OBJ_NATURES", IsUnique = true)]
    [DisplayColumn("TOBN_LIB")]
    public partial class TOBN_OBJ_NATURES
    {
        public TOBN_OBJ_NATURES()
        {
            TOBJE_OBJET_ETIQUETTES = new HashSet<TOBJE_OBJET_ETIQUETTES>();
        }

        [Key]
        public int TOBN_OBJ_NATUREID { get; set; }

       [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(20)]
        [Display(Name = "Display_TOBN_OBJ_NATURES_TOBN_CODE", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBN_CODE { get; set; }

        [StringLength(100)]
        [Display(Name = "Display_TOBN_OBJ_NATURES_TOBN_LIB", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBN_LIB { get; set; }

        [StringLength(250)]
        [Display(Name = "Display_TOBN_OBJ_NATURES_TOBN_DESC", ResourceType = typeof(DataAnnotationsResources))]
        public string TOBN_DESC { get; set; }

        [InverseProperty("TOBN_OBJ_NATURE")]
        public virtual ICollection<TOBJE_OBJET_ETIQUETTES> TOBJE_OBJET_ETIQUETTES { get; set; }
    }
}