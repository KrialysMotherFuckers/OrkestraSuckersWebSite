﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TEB_ETAT_BATCHID), Name = "IX_TBS_BATCH_SCENARIOS$TBS_BATCH_SCENARIOS")]
    public partial class TBS_BATCH_SCENARIOS
    {
        [Key]
        public int TEB_ETAT_BATCHID { get; set; }
        [Key]
        public int TS_SCENARIOID { get; set; }
        public int TBS_ORDRE_EXECUTION { get; set; }

        [ForeignKey(nameof(TEB_ETAT_BATCHID))]
        [InverseProperty(nameof(TEB_ETAT_BATCHS.TBS_BATCH_SCENARIOS))]
        public virtual TEB_ETAT_BATCHS TEB_ETAT_BATCH { get; set; }
        [ForeignKey(nameof(TS_SCENARIOID))]
        [InverseProperty(nameof(TS_SCENARIOS.TBS_BATCH_SCENARIOS))]
        public virtual TS_SCENARIOS TS_SCENARIO { get; set; }
    }
}