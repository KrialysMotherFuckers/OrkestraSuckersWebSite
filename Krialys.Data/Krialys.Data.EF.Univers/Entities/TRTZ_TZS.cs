﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [DisplayColumn(nameof(TRTZ_TZS.TRTZ_TZID))]
    public partial class TRTZ_TZS
    {
        public TRTZ_TZS()
        {
            // mise en commentaire temporaire
            TRU_USER = new HashSet<TRU_USERS>();
        }

        [Key]
        [Display(Name = "Display_TRTZ_TZS_TRTZ_TZID", ResourceType = typeof(Resources.DataAnnotationsResources))]

        [StringLength(30)]
        public string TRTZ_TZID { get; set; }

        public int? TRTZ_PREFERED_TZ { get; set; }

        [StringLength(100)]
        public string TRTZ_INFO_TZ { get; set; }


        /*CHILD */
        [InverseProperty(nameof(TRU_USERS.TRTZ_TZ))]
        public virtual ICollection<TRU_USERS> TRU_USER { get; set; }
    }
}