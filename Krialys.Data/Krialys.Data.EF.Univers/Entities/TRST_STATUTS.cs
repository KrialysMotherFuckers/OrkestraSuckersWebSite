﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Krialys.Data.EF.Univers
{
    [DisplayColumn("TRST_STATUTID")]
    public partial class TRST_STATUTS
    {
        public TRST_STATUTS()
        {
            TEP_ETAT_PREREQUISS = new HashSet<TEP_ETAT_PREREQUISS>();
            TF_FERMES = new HashSet<TF_FERMES>();
            TI_INFOS = new HashSet<TI_INFOS>();
            TPF_PLANIFS = new HashSet<TPF_PLANIFS>();
            TPR_PROFILS = new HashSet<TPR_PROFILS>();
            TPUF_PARALLELEU_FERMES = new HashSet<TPUF_PARALLELEU_FERMES>();
            TPUP_PARALLELEU_PARAMS = new HashSet<TPUP_PARALLELEU_PARAMS>();
            TPU_PARALLELEUS = new HashSet<TPU_PARALLELEUS>();
            TP_PERIMETRES = new HashSet<TP_PERIMETRES>();
            TSP_SERVEUR_PARAMS = new HashSet<TSP_SERVEUR_PARAMS>();
            TSRV_SERVEURS = new HashSet<TSRV_SERVEURS>();
            TEM_ETAT_MASTERS = new HashSet<TEM_ETAT_MASTERS>();
            TE_ETATS = new HashSet<TE_ETATS>();
            TS_SCENARIOS = new HashSet<TS_SCENARIOS>();
            TEB_ETAT_BATCHS = new HashSet<TEB_ETAT_BATCHS>();

            TSGA_SCENARIO_GPE_ASSOCIE = new HashSet<TSGA_SCENARIO_GPE_ASSOCIES>();
            TH_HABILITATION = new HashSet<TH_HABILITATIONS>();

            TSG_SCENARIO_GPE = new HashSet<TSG_SCENARIO_GPES>();
            TTE_TEAM = new HashSet<TTE_TEAMS>();

            TUTE_USER_TEAM = new HashSet<TUTE_USER_TEAMS>();
            TR_MEL_EMail_Templates = new HashSet<TR_MEL_EMail_Templates>();

            }

        [Key]
        [StringLength(3)]
        public string TRST_STATUTID { get; set; }
        [StringLength(255)]
        public string TRST_INFO { get; set; }
        [StringLength(255)]
        public string TRST_INFO_EN { get; set; }
        public int? TRST_REGLE01 { get; set; }
        public int? TRST_REGLE02 { get; set; }
        public int? TRST_REGLE03 { get; set; }
        public int? TRST_REGLE04 { get; set; }
        public int? TRST_REGLE05 { get; set; }
        public int? TRST_REGLE06 { get; set; }
        public int? TRST_REGLE07 { get; set; }
        public int? TRST_REGLE08 { get; set; }
        public int? TRST_REGLE09 { get; set; }
        public int? TRST_REGLE10 { get; set; }
        [StringLength(255)]
        public string TRST_DESCR { get; set; }

        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TEP_ETAT_PREREQUISS> TEP_ETAT_PREREQUISS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TF_FERMES> TF_FERMES { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TI_INFOS> TI_INFOS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TPF_PLANIFS> TPF_PLANIFS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TPR_PROFILS> TPR_PROFILS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TPUF_PARALLELEU_FERMES> TPUF_PARALLELEU_FERMES { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TPUP_PARALLELEU_PARAMS> TPUP_PARALLELEU_PARAMS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TPU_PARALLELEUS> TPU_PARALLELEUS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TP_PERIMETRES> TP_PERIMETRES { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TSP_SERVEUR_PARAMS> TSP_SERVEUR_PARAMS { get; set; }
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TSRV_SERVEURS> TSRV_SERVEURS { get; set; }

        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TEM_ETAT_MASTERS> TEM_ETAT_MASTERS { get; set; }

        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TE_ETATS> TE_ETATS { get; set; }
       
        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TS_SCENARIOS> TS_SCENARIOS { get; set; }

        [InverseProperty("TRST_STATUT")]
        public virtual ICollection<TEB_ETAT_BATCHS> TEB_ETAT_BATCHS { get; set; }


        [InverseProperty(nameof(TSGA_SCENARIO_GPE_ASSOCIES.TRST_STATUT))]
        public virtual ICollection<TSGA_SCENARIO_GPE_ASSOCIES> TSGA_SCENARIO_GPE_ASSOCIE  { get; set; }

        [InverseProperty(nameof(TH_HABILITATIONS.TRST_STATUT))]
        public virtual ICollection<TH_HABILITATIONS> TH_HABILITATION  { get; set; }

        [InverseProperty(nameof(TSG_SCENARIO_GPES.TRST_STATUT))]
        public virtual ICollection<TSG_SCENARIO_GPES> TSG_SCENARIO_GPE  { get; set; }

        [InverseProperty(nameof(TTE_TEAMS.TRST_STATUT))]
        public virtual ICollection<TTE_TEAMS> TTE_TEAM  { get; set; }

        [InverseProperty(nameof(TUTE_USER_TEAMS.TRST_STATUT))]
        public virtual ICollection<TUTE_USER_TEAMS> TUTE_USER_TEAM  { get; set; }

        [InverseProperty(nameof(Univers.TR_MEL_EMail_Templates.TRST_STATUT))]
        public virtual ICollection<TR_MEL_EMail_Templates> TR_MEL_EMail_Templates { get; set; }
        
    }
}