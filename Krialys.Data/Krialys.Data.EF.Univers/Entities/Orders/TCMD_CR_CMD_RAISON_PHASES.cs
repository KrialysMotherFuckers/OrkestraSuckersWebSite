﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    public partial class TCMD_CR_CMD_RAISON_PHASES
    {
        [Key]
        [Display(Name = "Display_TCMD_CR_CMD_RAISON_PHASES_TCMD_CR_CMD_RAISON_PHASEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TCMD_CR_CMD_RAISON_PHASEID { get; set; }

        [Display(Name = "Display_TCMD_CR_CMD_RAISON_PHASES_TCMD_SP_SUIVI_PHASEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TCMD_SP_SUIVI_PHASEID { get; set; }

        [Display(Name = "Display_TCMD_CR_CMD_RAISON_PHASES_TCMD_RP_RAISON_PHASEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TCMD_RP_RAISON_PHASEID { get; set; }

        [ForeignKey("TCMD_RP_RAISON_PHASEID")]
        [InverseProperty("TCMD_CR_CMD_RAISON_PHASES")]
        public virtual TCMD_RP_RAISON_PHASES TCMD_RP_RAISON_PHASE { get; set; }
        [ForeignKey("TCMD_SP_SUIVI_PHASEID")]
        [InverseProperty("TCMD_CR_CMD_RAISON_PHASES")]
        public virtual TCMD_SP_SUIVI_PHASES TCMD_SP_SUIVI_PHASE { get; set; }
    }
}