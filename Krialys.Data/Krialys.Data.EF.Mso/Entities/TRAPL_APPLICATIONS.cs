﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Mso
{
    [DisplayColumn("TRAPL_LIB")]
    public partial class TRAPL_APPLICATIONS
    {
        public TRAPL_APPLICATIONS()
        {
            TRA_ATTENDUS = new HashSet<TRA_ATTENDUS>();
        }

        [Key]
        [Display(Name = "Display_TRAPL_APPLICATIONS_TRAPL_APPLICATIONID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRAPL_APPLICATIONID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Display_TRAPL_APPLICATIONS_TRAPL_LIB", ResourceType = typeof(Resources.DataAnnotationsResources))]

        public string TRAPL_LIB { get; set; }
        [Required]
        [StringLength(255)]
        [Display(Name = "Display_TRAPL_APPLICATIONS_TRAPL_DESCRIPTION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRAPL_DESCRIPTION { get; set; }

        // Foreign Keys \\

        [InverseProperty("TRAPL_APPLICATION")]
        public virtual ICollection<TRA_ATTENDUS> TRA_ATTENDUS { get; set; }
    }
}