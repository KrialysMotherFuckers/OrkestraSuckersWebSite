﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [Index(nameof(TSRV_SERVEURID), nameof(TSP_KEY), Name = "UQ_TSP_SERVEUR_PARAM", IsUnique = true)]
    public partial class TSP_SERVEUR_PARAMS
    {
        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TSP_SERVEUR_PARAMID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Key]
        public int TSP_SERVEUR_PARAMID { get; set; }

        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TSRV_SERVEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        public int TSRV_SERVEURID { get; set; }

        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TSP_KEY", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Required]
        [StringLength(255)]
        public string TSP_KEY { get; set; }

        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TSP_VALUE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Required]
        [StringLength(255)]
        public string TSP_VALUE { get; set; }

        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TSP_TYPE", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(255)]
        public string TSP_TYPE { get; set; }

        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TRST_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [Required]
        [StringLength(3)]
        public string TRST_STATUTID { get; set; }

        [Display(Name = "Display_TSP_SERVEUR_PARAMS_TSP_DESCR", ResourceType = typeof(Resources.DataAnnotationsResources))]
        [StringLength(255)]
        public string TSP_DESCR { get; set; }

        [ForeignKey(nameof(TRST_STATUTID))]
        [InverseProperty(nameof(TRST_STATUTS.TSP_SERVEUR_PARAMS))]
        public virtual TRST_STATUTS TRST_STATUT { get; set; }
        [ForeignKey(nameof(TSRV_SERVEURID))]
        [InverseProperty(nameof(TSRV_SERVEURS.TSP_SERVEUR_PARAMS))]
        public virtual TSRV_SERVEURS TSRV_SERVEUR { get; set; }
    }
}