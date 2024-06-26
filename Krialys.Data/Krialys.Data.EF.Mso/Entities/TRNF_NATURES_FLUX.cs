﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Mso
{
    [DisplayColumn("TRNF_LIB")]
    public partial class TRNF_NATURES_FLUX
    {
        public TRNF_NATURES_FLUX()
        {
            TRA_ATTENDUSTRNF_NATURE_DESTINATION = new HashSet<TRA_ATTENDUS>();
            TRA_ATTENDUSTRNF_NATURE_ORIGINE = new HashSet<TRA_ATTENDUS>();
        }

        [Key]
        [Display(Name = "Display_TRNF_NATURES_FLUX_TRNF_NATURE_FLUXID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRNF_NATURE_FLUXID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Display_TRNF_NATURES_FLUX_TRNF_LIB", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRNF_LIB { get; set; }

        [StringLength(255)]
        [Display(Name = "Display_TRNF_NATURES_FLUX_TRNF_DESCRIPTION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRNF_DESCRIPTION { get; set; }

        // Foreign Keys \\

        [InverseProperty(nameof(TRA_ATTENDUS.TRNF_NATURE_DESTINATION))]
        public virtual ICollection<TRA_ATTENDUS> TRA_ATTENDUSTRNF_NATURE_DESTINATION { get; set; }

        [InverseProperty(nameof(TRA_ATTENDUS.TRNF_NATURE_ORIGINE))]
        public virtual ICollection<TRA_ATTENDUS> TRA_ATTENDUSTRNF_NATURE_ORIGINE { get; set; }
    }
}