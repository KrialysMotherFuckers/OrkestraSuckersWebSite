﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable


namespace Krialys.Data.EF.Etq
{
    [Index(nameof(TTR_TYPE_ENTREE), Name = "UK_TTR_TYPE_RESSOURCES", IsUnique = true)]
   
 [DisplayColumn("TTR_TYPE_ENTREE")]
    public partial class TTR_TYPE_RESSOURCES
    {
        public TTR_TYPE_RESSOURCES()
        {
            TSR_SUIVI_RESSOURCES = new HashSet<TSR_SUIVI_RESSOURCES>();
        }

        [Key]
        [Display(Name = "Display_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID", ResourceType = typeof(DataAnnotationsResources))]

        public int TTR_TYPE_RESSOURCEID { get; set; }
		
       [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
        [StringLength(20)]
		[Display(Name = "Display_TTR_TYPE_RESSOURCES_TTR_TYPE_ENTREE", ResourceType = typeof(DataAnnotationsResources))]
        public string TTR_TYPE_ENTREE  { get; set; }
		
        [StringLength(100)]
		[Display(Name = "Display_TTR_TYPE_RESSOURCES_TTR_TYPE_ENTREE_LIB", ResourceType = typeof(DataAnnotationsResources))]
        public string TTR_TYPE_ENTREE_LIB { get; set; }
		
        [StringLength(250)]
		[Display(Name = "Display_TTR_TYPE_RESSOURCES_TTR_TYPE_ENTREE_DESC", ResourceType = typeof(DataAnnotationsResources))]
        public string TTR_TYPE_ENTREE_DESC { get; set; }

        [InverseProperty("TTR_TYPE_RESSOURCE")]
        public virtual ICollection<TSR_SUIVI_RESSOURCES> TSR_SUIVI_RESSOURCES { get; set; }
    }
}