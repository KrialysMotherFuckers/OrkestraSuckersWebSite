using Krialys.Common.Literals;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    [DisplayColumn("TE_NOM_ETAT")]
    [Index(nameof(TEM_ETAT_MASTERID), Name = "IX_TE_ETATS$TEM_ETAT_MASTERS")]
    public partial class TE_ETATS
    {
        public TE_ETATS()
        {
            TE_GENERE_CUBE = StatusLiteral.No;
            TE_ENV_VIERGE_TAILLE = 0;
            TE_ENV_VIERGE_UPLOADED = StatusLiteral.No;

            TD_DEMANDES = new HashSet<TD_DEMANDES>();
            TEB_ETAT_BATCHS = new HashSet<TEB_ETAT_BATCHS>();
            TEL_ETAT_LOGICIELS = new HashSet<TEL_ETAT_LOGICIELS>();
            TEP_ETAT_PREREQUISS = new HashSet<TEP_ETAT_PREREQUISS>();
            TER_ETAT_RESSOURCES = new HashSet<TER_ETAT_RESSOURCES>();
            TS_SCENARIOS = new HashSet<TS_SCENARIOS>();

            TCMD_COMMANDES_TE_ETAT = new HashSet<TCMD_COMMANDES>();
        }

        [Display(Name = "Display_TE_ETATS_TE_ETATID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Key]
        public int TE_ETATID { get; set; }

        [Required]
        [Display(Name = "Display_TE_ETATS_TEM_ETAT_MASTERID", ResourceType = typeof(Resources.DataAnnotationsResources))]

        public int? TEM_ETAT_MASTERID { get; set; }

        [Required]
        [Editable(false)]
        [StringLength(255)]
        [Display(Name = "Display_TE_ETATS_TE_NOM_ETAT", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_NOM_ETAT { get; set; }

        [StringLength(255)]
        [Display(Name = "Display_TE_ETATS_TE_NOM_DATABASE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_NOM_DATABASE { get; set; }

        [StringLength(255)]
        [Display(Name = "Display_TE_ETATS_TE_NOM_SERVEUR_CUBE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_NOM_SERVEUR_CUBE { get; set; }


        [Required]
        [Range(1, 99, ErrorMessage = "Une version commence à 1")]
        [Display(Name = "Display_TE_ETATS_TE_INDICE_REVISION_L1", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TE_INDICE_REVISION_L1 { get; set; }

        [Required]
        [Range(0, 99, ErrorMessage = "Valeur autorisée entre 0 et 99")]
        [Display(Name = "Display_TE_ETATS_TE_INDICE_REVISION_L2", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TE_INDICE_REVISION_L2 { get; set; }

        [Required]
        [Display(Name = "Display_TE_ETATS_TE_INDICE_REVISION_L3", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Range(0, 99, ErrorMessage = "valeur autorisée entre 0 et 99")]
        public int TE_INDICE_REVISION_L3 { get; set; }

        [Editable(false)]
        [Display(Name = "Display_TE_ETATS_TE_VERSION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_VERSION { get; set; }

        [Editable(false)]
        [Display(Name = "Display_TE_ETATS_TE_FULLNAME", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_FULLNAME { get; set; }

        /* TODO date a alimenter a ce niveau */
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TE_ETATS_TE_DATE_REVISION", ResourceType = typeof(Resources.DataAnnotationsResources))]

        public DateTime TE_DATE_REVISION { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Display_TE_ETATS_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRST_STATUTID { get; set; }

        [StringLength(3)]
        [Display(Name = "Display_TE_ETATS_TRST_STATUTID_OLD", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRST_STATUTID_OLD { get; set; }

        [Required]
        [StringLength(1)]
        [Display(Name = "Display_TE_ETATS_TE_TYPE_SORTIE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_TYPE_SORTIE { get; set; }

        [Required]
        [StringLength(1)]
        public string TE_GENERE_CUBE { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Display_TE_ETATS_TE_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_COMMENTAIRE { get; set; }


        [StringLength(255)]
        [Display(Name = "Display_TE_ETATS_TE_INFO_REVISION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_INFO_REVISION { get; set; }


        [Editable(false)]
        [Display(Name = "Display_TE_ETATS_TE_DUREE_PRODUCTION_ESTIMEE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int? TE_DUREE_PRODUCTION_ESTIMEE { get; set; }


        [Editable(false)]
        [Display(Name = "Display_TE_ETATS_TE_DUREE_DERNIERE_PRODUCTION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int? TE_DUREE_DERNIERE_PRODUCTION { get; set; }

        [Editable(false)]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TE_ETATS_TE_DATE_DERNIERE_PRODUCTION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime? TE_DATE_DERNIERE_PRODUCTION { get; set; }


        [StringLength(36)]
        [Editable(false)]
        [Display(Name = "Display_TE_ETATS_TE_GUID", ResourceType = typeof(Resources.DataAnnotationsResources))]

        public string TE_GUID { get; set; }

        [Display(Name = "Display_TE_ETATS_PARENT_ETATID", ResourceType = typeof(Resources.DataAnnotationsResources))]

        public int? PARENT_ETATID { get; set; }

        [StringLength(1)]
        [Display(Name = "Display_TE_ETATS_TE_VALIDATION_IMPLICITE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_VALIDATION_IMPLICITE { get; set; }

        [StringLength(1)]
        [Display(Name = "Display_TE_ETATS_TE_SEND_MAIL_GESTIONNAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_SEND_MAIL_GESTIONNAIRE { get; set; }

        [StringLength(1)]
        [Display(Name = "Display_TE_ETATS_TE_SEND_MAIL_CLIENT", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_SEND_MAIL_CLIENT { get; set; }

        [Editable(false)]
        [Display(Name = "Display_TE_ETATS_TE_ENV_VIERGE_TAILLE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int? TE_ENV_VIERGE_TAILLE { get; set; }

        [Required]
        [Editable(false)]
        [StringLength(1)]
        [Display(Name = "Display_TE_ETATS_TE_ENV_VIERGE_UPLOADED", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TE_ENV_VIERGE_UPLOADED { get; set; }


        [StringLength(36)]
        [Display(Name = "Display_TE_ETATS_TRU_ENV_VIERGE_AUTEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRU_ENV_VIERGE_AUTEURID { get; set; }

        [Required]
        [Editable(false)]
        [StringLength(36)]
        [Display(Name = "Display_TE_ETATS_TRU_DECLARANTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRU_DECLARANTID { get; set; }

        /* date de dernier diagnostic realisé avec succes et donc de creation du zip*/
        [Editable(false)]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TE_ETATS_TE_ENV_VIERGE_DATE_DIAG_VALIDE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime? TE_ENV_VIERGE_DATE_DIAG_VALIDE { get; set; }

        /*date de dernier upload d un fichier dans l'env vierge pour comparer avec la date du dernier diag valide */
        [Editable(false)]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TE_ETATS_TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime? TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP { get; set; }


        [InverseProperty("TE_ETAT")]
        public virtual ICollection<TD_DEMANDES> TD_DEMANDES { get; set; }
        [InverseProperty("TE_ETAT")]
        public virtual ICollection<TEB_ETAT_BATCHS> TEB_ETAT_BATCHS { get; set; }
        [InverseProperty("TE_ETAT")]
        public virtual ICollection<TEL_ETAT_LOGICIELS> TEL_ETAT_LOGICIELS { get; set; }
        [InverseProperty("TE_ETAT")]
        public virtual ICollection<TEP_ETAT_PREREQUISS> TEP_ETAT_PREREQUISS { get; set; }
        [InverseProperty("TE_ETAT")]
        public virtual ICollection<TER_ETAT_RESSOURCES> TER_ETAT_RESSOURCES { get; set; }
        [InverseProperty("TE_ETAT")]
        public virtual ICollection<TS_SCENARIOS> TS_SCENARIOS { get; set; }


        [InverseProperty(nameof(TCMD_COMMANDES.TE_ETAT))]
        public virtual ICollection<TCMD_COMMANDES> TCMD_COMMANDES_TE_ETAT { get; set; }


        /* FK*/

        [ForeignKey(nameof(TEM_ETAT_MASTERID))]
        [InverseProperty(nameof(TEM_ETAT_MASTERS.TE_ETATS))]
        public virtual TEM_ETAT_MASTERS TEM_ETAT_MASTER { get; set; }


        [ForeignKey(nameof(TRU_ENV_VIERGE_AUTEURID))]
        [InverseProperty(nameof(TRU_USERS.TE_ETATSTRU_ENV_VIERGE_AUTEUR))]
        public virtual TRU_USERS TRU_ENV_VIERGE_AUTEUR { get; set; }

        [ForeignKey(nameof(TRU_DECLARANTID))]
        [InverseProperty(nameof(TRU_USERS.TE_ETATSTRU_DECLARANT))]
        public virtual TRU_USERS TRU_DECLARANT { get; set; }

        [ForeignKey(nameof(TRST_STATUTID))]
        [InverseProperty(nameof(TRST_STATUTS.TE_ETATS))]
        public virtual TRST_STATUTS TRST_STATUT { get; set; }

    }
}