﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Krialys.Common.Literals;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TP_PERIMETREID), Name = "IX_TEM_ETAT_MASTERS$PERIMETRE")]
    [Index(nameof(TEM_NOM_ETAT_MASTER), nameof(TP_PERIMETREID), Name = "UQ_TEM_ETAT_MASTER", IsUnique = true)]
    [DisplayColumn("TEM_NOM_ETAT_MASTER")]
    public partial class TEM_ETAT_MASTERS
    {
        public TEM_ETAT_MASTERS()
        {
            // Perimeter is not used yet but required.
            TP_PERIMETREID = 1;
            // Default statut is active.
            TRST_STATUTID = StatusLiteral.Available;
            // Generate GUID.
            TEM_GUID = Guid.NewGuid().ToString("N");

            TEMF_ETAT_MASTER_FERMES = new HashSet<TEMF_ETAT_MASTER_FERMES>();
            TE_ETATS = new HashSet<TE_ETATS>();
        }

        [Key]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TEM_ETAT_MASTERID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TEM_ETAT_MASTERID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(Resources.DataAnnotationsResources))]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TEM_NOM_ETAT_MASTER", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(255)]
        public string TEM_NOM_ETAT_MASTER { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(Resources.DataAnnotationsResources))]
        [Editable(false)]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TEM_DATE_CREATION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime TEM_DATE_CREATION { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(3)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRST_STATUTID { get; set; }

        [StringLength(3)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TRST_STATUTID_OLD", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRST_STATUTID_OLD { get; set; }

        [StringLength(255)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TEM_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TEM_COMMENTAIRE { get; set; }

        [StringLength(36)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TEM_ETAT_MASTER_PARENTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TEM_ETAT_MASTER_PARENTID { get; set; }

        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(36)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TRU_RESPONSABLE_FONCTIONNELID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRU_RESPONSABLE_FONCTIONNELID { get; set; } = ""; // OB-367: please use a 'default' value, this avoids exception when used as a 'foreign key'


        [StringLength(36)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TRU_RESPONSABLE_TECHNIQUEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRU_RESPONSABLE_TECHNIQUEID { get; set; } = ""; // OB-367: please use a 'default' value, this avoids exception when used as a 'foreign key'


        [Editable(false)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TP_PERIMETREID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TP_PERIMETREID { get; set; }


        [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(Resources.DataAnnotationsResources))]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TC_CATEGORIEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int? TC_CATEGORIEID { get; set; }


        [StringLength(36)]
        [Display(Name = "Display_TEM_ETAT_MASTERS_TEM_GUID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TEM_GUID { get; set; }

        [ForeignKey(nameof(TRU_RESPONSABLE_FONCTIONNELID))]
        [InverseProperty(nameof(TRU_USERS.TEM_ETAT_MASTERSTRU_RESPONSABLE_FONCTIONNEL))]
        public virtual TRU_USERS TRU_RESPONSABLE_FONCTIONNEL { get; set; }


        [ForeignKey(nameof(TRU_RESPONSABLE_TECHNIQUEID))]
        [InverseProperty(nameof(TRU_USERS.TEM_ETAT_MASTERSTRU_RESPONSABLE_TECHNIQUE))]
        public virtual TRU_USERS TRU_RESPONSABLE_TECHNIQUE { get; set; }

        [ForeignKey(nameof(TRST_STATUTID))]
        [InverseProperty(nameof(TRST_STATUTS.TEM_ETAT_MASTERS))]
        public virtual TRST_STATUTS TRST_STATUT { get; set; }


        [ForeignKey(nameof(TC_CATEGORIEID))]
        [InverseProperty(nameof(TC_CATEGORIES.TEM_ETAT_MASTERS))]
        public virtual TC_CATEGORIES TC_CATEGORIE { get; set; }

        [ForeignKey(nameof(TP_PERIMETREID))]
        [InverseProperty(nameof(TP_PERIMETRES.TEM_ETAT_MASTERS))]
        public virtual TP_PERIMETRES TP_PERIMETRE { get; set; }
        [InverseProperty("TEM_ETAT_MASTER")]
        public virtual ICollection<TEMF_ETAT_MASTER_FERMES> TEMF_ETAT_MASTER_FERMES { get; set; }
        [InverseProperty("TEM_ETAT_MASTER")]
        public virtual ICollection<TE_ETATS> TE_ETATS { get; set; }
    }
}