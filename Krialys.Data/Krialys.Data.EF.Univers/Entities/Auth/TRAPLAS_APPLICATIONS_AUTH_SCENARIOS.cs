﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    public partial class TRAPLAS_APPLICATIONS_AUTH_SCENARIOS
    {

        [Key]
        [Display(Name = "Display_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAPLAS_APPLICATIONS_AUTH_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRAPLAS_APPLICATIONS_AUTH_SCENARIOID { get; set; }

        [Display(Name = "Display_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRAS_AUTH_SCENARIOID { get; set; }

        [Display(Name = "Display_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRCLI_CLIENTAPPLICATIONID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRCLI_CLIENTAPPLICATIONID { get; set; }

        [ForeignKey(nameof(TRAS_AUTH_SCENARIOID))]
        [InverseProperty(nameof(TRAS_AUTH_SCENARIOS.TRAPLAS_APPLICATIONS_AUTH_SCENARIOS))]
        public virtual TRAS_AUTH_SCENARIOS TRAS_AUTH_SCENARIO { get; set; }

        [ForeignKey(nameof(TRCLI_CLIENTAPPLICATIONID))]
        [InverseProperty(nameof(TRCLI_CLIENTAPPLICATIONS.TRAPLAS_APPLICATIONS_AUTH_SCENARIOS))]
        public virtual TRCLI_CLIENTAPPLICATIONS TRCLI_CLIENTAPPLICATION { get; set; }


    }
}