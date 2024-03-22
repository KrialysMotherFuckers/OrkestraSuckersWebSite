﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TEM_ETAT_MASTERID), nameof(TF_FERMEID), Name = "UK_TEMF_ETAT_MASTER_FERMES", IsUnique = true)]
    [Index(nameof(TEM_ETAT_MASTERID), nameof(TF_FERMEID), Name = "UQ_TEMF_ETAT_MASTER_FERMES", IsUnique = true)]
    public partial class TEMF_ETAT_MASTER_FERMES
    {
        [Key]
        public int TEMF_ETAT_MASTER_FERMEID { get; set; }
        public int TEM_ETAT_MASTERID { get; set; }
        public int TF_FERMEID { get; set; }
        public DateTime? TEMF_DATE_AJOUT { get; set; }
        public DateTime? TEMF_DATE_SUPPRESSION { get; set; }
        [StringLength(255)]
        public string TEMF_DESCR { get; set; }
        public int? TEMF_ORDRE_PRIORITE { get; set; }

        [ForeignKey(nameof(TEM_ETAT_MASTERID))]
        [InverseProperty(nameof(TEM_ETAT_MASTERS.TEMF_ETAT_MASTER_FERMES))]
        public virtual TEM_ETAT_MASTERS TEM_ETAT_MASTER { get; set; }
        [ForeignKey(nameof(TF_FERMEID))]
        [InverseProperty(nameof(TF_FERMES.TEMF_ETAT_MASTER_FERMES))]
        public virtual TF_FERMES TF_FERME { get; set; }
    }
}