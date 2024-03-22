﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Krialys.Common.Literals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    public partial class TRCLICL_CLIENTAPPLICATIONS_CLAIMS
    {
        [Key]
        [Display(Name = "Display_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCLICL_CLIENTAPPLICATION_CLAIMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRCLICL_CLIENTAPPLICATION_CLAIMID { get; set; }

        [Display(Name = "Display_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCL_CLAIMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRCL_CLAIMID { get; set; }

        [Display(Name = "Display_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCLI_CLIENTAPPLICATIONID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TRCLI_CLIENTAPPLICATIONID { get; set; }

        [StringLength(32)]
        [Display(Name = "Display_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCLICL_CLAIM_VALUE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public string TRCLICL_CLAIM_VALUE { get; set; }

        [Display(Name = "Display_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCLICL_DESCRIPTION", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(255)]
        public string TRCLICL_DESCRIPTION { get; set; }

        [Required(ErrorMessage = "{0} is required. Type A for active, I for inactive")]
        [StringLength(1)]
        [Display(Name = "Display_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCLICL_STATUS", ResourceType = typeof(Resources.DataAnnotationsResources))]

        public string TRCLICL_STATUS
        {
            get { return _statut; }
            set
            {
                _statut = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Substring(0, 1).ToUpper();
                if (!_statut.ToUpper().Equals(StatusLiteral.Available) && !_statut.ToUpper().Equals(StatusLiteral.Deactivated))
                {
                    _statut = string.Empty;
                }
            }
        }
        private string _statut = string.Empty;

        [ForeignKey(nameof(TRCLI_CLIENTAPPLICATIONID))]
        [InverseProperty(nameof(TRCLI_CLIENTAPPLICATIONS.TRCLICL_CLIENTAPPLICATIONS_CLAIMS))]
        public virtual TRCLI_CLIENTAPPLICATIONS TRCLI_CLIENTAPPLICATION { get; set; }

        [ForeignKey(nameof(TRCL_CLAIMID))]
        [InverseProperty(nameof(TRCL_CLAIMS.TRCLICL_CLIENTAPPLICATIONS_CLAIMS))]
        public virtual TRCL_CLAIMS TRCL_CLAIM { get; set; }
    }
}