﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TD_DEMANDEID), nameof(TE_ETATID), nameof(TRD_NOM_FICHIER), Name = "IX_TRD_RESSOURCE_DEMANDES$ETAT_RESSOURCE")]
    [Index(nameof(TE_ETATID), Name = "IX_TRD_RESSOURCE_DEMANDES$TE_ETATID")]
    public partial class TRD_RESSOURCE_DEMANDES
    {
        [Key]
        public int TRD_RESSOURCE_DEMANDEID { get; set; }
        public int TE_ETATID { get; set; }
        public int TD_DEMANDEID { get; set; }
        public int TER_ETAT_RESSOURCEID { get; set; }
        [Required]
        [StringLength(255)]
        public string TRD_NOM_FICHIER { get; set; }
        [StringLength(255)]
        public string TRD_NOM_FICHIER_ORIGINAL { get; set; }
        [StringLength(1)]
        public string TRD_FICHIER_PRESENT { get; set; }
        [StringLength(255)]
        public string TRD_COMMENTAIRE { get; set; }
        public int? TRD_TAILLE_FICHIER { get; set; }

        public virtual TD_DEMANDES T { get; set; }
        [ForeignKey(nameof(TER_ETAT_RESSOURCEID))]
        [InverseProperty(nameof(TER_ETAT_RESSOURCES.TRD_RESSOURCE_DEMANDES))]
        public virtual TER_ETAT_RESSOURCES TER_ETAT_RESSOURCE { get; set; }
    }
}