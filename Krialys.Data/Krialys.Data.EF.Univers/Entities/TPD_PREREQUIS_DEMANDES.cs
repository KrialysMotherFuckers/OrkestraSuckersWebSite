﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TD_DEMANDEID), nameof(TE_ETATID), Name = "IX_TPD_PREREQUIS_DEMANDES")]
    [Index(nameof(TD_DEMANDEID), nameof(TEP_ETAT_PREREQUISID), IsUnique = true, Name = "UK_TPD_PREREQUIS_DEMANDES")]
    public partial class TPD_PREREQUIS_DEMANDES
    {
        [Key]
        public int TPD_PREREQUIS_DEMANDEID { get; set; }

        public int TD_DEMANDEID { get; set; }

        public int TE_ETATID { get; set; }

        public int TEP_ETAT_PREREQUISID { get; set; }
        [StringLength(1)]
        public string TPD_VALIDE { get; set; }

        [DisplayFormat(DataFormatString = "g")]
        public DateTime? TPD_DATE_VALIDATION { get; set; }
        
        [DisplayFormat(DataFormatString = "g")]
        public DateTime? TPD_DATE_LAST_CHECK { get; set; }
        public int? TPD_NB_FILE_TROUVE { get; set; }
        [StringLength(200)]
        public string TPD_MSG_LAST_CHECK { get; set; }

        [ForeignKey(nameof(TD_DEMANDEID))]
        [InverseProperty(nameof(TD_DEMANDES.TPD_PREREQUIS_DEMANDES))]
        public virtual TD_DEMANDES TD_DEMANDE { get; set; }

        [ForeignKey(nameof(TEP_ETAT_PREREQUISID))]
        [InverseProperty(nameof(TEP_ETAT_PREREQUISS.TPD_PREREQUIS_DEMANDES))]
        public virtual TEP_ETAT_PREREQUISS TEP_ETAT_PREREQUIS { get; set; }
    }
}