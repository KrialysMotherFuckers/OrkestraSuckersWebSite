﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TR_MEL_EMail_Templates.mel_code), Name = "IX_TMT_CODE$TR_MEL_EMail_Templates")]
    public partial class TR_MEL_EMail_Templates
    {
        [Key]
        public Guid mel_id { get; set; }

        [Required]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_code { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_additional_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_additional_code { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_description", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_description { get; set; }

        [StringLength(3)]
        [Display(Name = "Display_TR_MEL_EMail_Templates_sta_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string sta_code { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_lng_code", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string lng_code { get; set; }

        [Required]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_subject", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_subject { get; set; }

        [Required]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_body", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_body { get; set; }

        [Required]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_footer", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_footer { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_comments", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_comments { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_recipients", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_recipients { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_recipients_in_cc", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_recipients_in_cc { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_recipients_in_bcc", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_recipients_in_bcc { get; set; }

        //N Normal , L Low, H High
        [StringLength(1)]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_email_importance", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_email_importance { get; set; } = "L";

        [Column(TypeName = "datetime")]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_creation_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime mel_creation_date { get; set; } = DateTime.UtcNow;

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_created_by", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_created_by { get; set; } = "admin";

        [Column(TypeName = "datetime")]
        [DisplayFormat(DataFormatString = "g")]
        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_update_date", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public DateTime? mel_update_date { get; set; }

        [Display(Name = "Display_TR_MEL_EMail_Templates_mel_update_by", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string mel_update_by { get; set; }

        [ForeignKey(nameof(sta_code))]
        [InverseProperty(nameof(TRST_STATUTS.TR_MEL_EMail_Templates))]
        public virtual TRST_STATUTS TRST_STATUT { get; set; }

        [ForeignKey(nameof(lng_code))]
        [InverseProperty(nameof(Univers.TR_LNG_Languages.TR_MEL_EMail_Templates))]
        public virtual TR_LNG_Languages TR_LNG_Languages { get; set; }

    }
}