﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    [DisplayColumn(nameof(TR_LNG_Languages.lng_code))]
    public partial class TR_LNG_Languages
    {
        public TR_LNG_Languages()
        {
            TR_MEL_EMail_Templates = new HashSet<TR_MEL_EMail_Templates>();

            // mise en commentaire temporaire
            TRU_USERS = new HashSet<TRU_USERS>();
        }

        [Display(Name = "Display_TR_LNG_Languages_lng_id", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public Guid lng_id { get; set; } = new Guid();

        [Key]
        [Display(Name = "Display_TR_LNG_Languages_lng_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(3)]
        public string lng_code { get; set; }

        [Required]
        [Display(Name = "Display_TR_LNG_Languages_lng_label", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string lng_label { get; set; }

        [Display(Name = "Display_TR_LNG_Languages_lng_is_preferred_language", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [DefaultValue(false)]
        public bool lng_is_preferred_language { get; set; } = false;

        [InverseProperty(nameof(Univers.TR_MEL_EMail_Templates.TR_LNG_Languages))]
        public virtual ICollection<TR_MEL_EMail_Templates> TR_MEL_EMail_Templates { get; set; }

        [InverseProperty(nameof(Univers.TRU_USERS.TR_LNG_Languages))]
        public virtual ICollection<TRU_USERS> TRU_USERS { get; set; }
    }
}