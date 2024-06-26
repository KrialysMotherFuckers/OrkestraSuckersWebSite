﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    [DisplayColumn("TLE_EDITEUR")]
    public partial class TLE_LOGICIEL_EDITEURS
    {
        public TLE_LOGICIEL_EDITEURS()
        {
            TLEM_LOGICIEL_EDITEUR_MODELES = new HashSet<TLEM_LOGICIEL_EDITEUR_MODELES>();
            TL_LOGICIELS = new HashSet<TL_LOGICIELS>();
        }

        [Display(Name = "Display_TLE_LOGICIEL_EDITEURS_TLE_LOGICIEL_EDITEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Key]
        public int TLE_LOGICIEL_EDITEURID { get; set; }

        [Display(Name = "Display_TLE_LOGICIEL_EDITEURS_TLE_EDITEUR", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(30)]
        [Required]
        public string TLE_EDITEUR { get; set; }

        [InverseProperty("TLE_LOGICIEL_EDITEUR")]
        public virtual ICollection<TLEM_LOGICIEL_EDITEUR_MODELES> TLEM_LOGICIEL_EDITEUR_MODELES { get; set; }
        [InverseProperty("TLE_LOGICIEL_EDITEUR")]
        public virtual ICollection<TL_LOGICIELS> TL_LOGICIELS { get; set; }
    }
}