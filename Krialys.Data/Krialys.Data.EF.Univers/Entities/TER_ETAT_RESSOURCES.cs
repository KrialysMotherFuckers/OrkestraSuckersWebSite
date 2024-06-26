﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TE_ETATID), Name = "IX_TER_ETAT_RESSOURCES$ETATID")]
    [Index(nameof(TE_ETATID), nameof(TER_NOM_FICHIER), Name = "UK_TER_ETAT_RESSOURCES$ETATIDNOM_FICHIER", IsUnique = true)]
    public partial class TER_ETAT_RESSOURCES
    {
        public TER_ETAT_RESSOURCES()
        {
            TRD_RESSOURCE_DEMANDES = new HashSet<TRD_RESSOURCE_DEMANDES>();
            TRS_RESSOURCE_SCENARIOS = new HashSet<TRS_RESSOURCE_SCENARIOS>();
        }

        [Key]
        public int TER_ETAT_RESSOURCEID { get; set; }

        public int TE_ETATID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Display_TER_ETAT_RESSOURCES_TER_NOM_FICHIER", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TER_NOM_FICHIER { get; set; }

        [StringLength(255)]
        [Display(Name = "Display_TER_ETAT_RESSOURCES_TER_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TER_COMMENTAIRE { get; set; }

        [StringLength(255)]
        [Display(Name = "Display_TER_ETAT_RESSOURCES_TER_PATH_RELATIF", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TER_PATH_RELATIF { get; set; }

        [StringLength(1)]
        public string TER_MODELE_DOC { get; set; }

        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TER_ETAT_RESSOURCES_TER_MODELE_DATE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime? TER_MODELE_DATE { get; set; }

        [Display(Name = "Display_TER_ETAT_RESSOURCES_TER_MODELE_TAILLE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int? TER_MODELE_TAILLE { get; set; }

        [Required]
        [StringLength(1)]
        [Display(Name = "Display_TER_ETAT_RESSOURCES_TER_IS_PATTERN", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TER_IS_PATTERN { get; set; }

        [StringLength(255)]
        public string TER_NOM_MODELE { get; set; }


        [ForeignKey(nameof(TE_ETATID))]
        [InverseProperty(nameof(TE_ETATS.TER_ETAT_RESSOURCES))]
        public virtual TE_ETATS TE_ETAT { get; set; }

        [InverseProperty("TER_ETAT_RESSOURCE")]
        public virtual ICollection<TRD_RESSOURCE_DEMANDES> TRD_RESSOURCE_DEMANDES { get; set; }

        [InverseProperty("TER_ETAT_RESSOURCE")]
        public virtual ICollection<TRS_RESSOURCE_SCENARIOS> TRS_RESSOURCE_SCENARIOS { get; set; }
    }
}