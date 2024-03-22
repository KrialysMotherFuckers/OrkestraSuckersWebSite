﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Krialys.Entities.COMMON;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Krialys.Data.EF.Etq
{
    public partial class KrialysDbContext : DbContext
    {
        #region CONSTRUCTORS

        public KrialysDbContext() { }
        public KrialysDbContext(DbContextOptions<KrialysDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
                options.UseSqlite("DataSource=../../../ApiUnivers/App_Data/Database/db-ETQ.db3");
        }

        #endregion

        public virtual DbSet<TEQC_ETQ_CODIFS> TEQC_ETQ_CODIFS { get; set; }
        public virtual DbSet<TETQ_ETIQUETTES> TETQ_ETIQUETTES { get; set; }
        public virtual DbSet<TOBF_OBJ_FORMATS> TOBF_OBJ_FORMATS { get; set; }
        public virtual DbSet<TOBJE_OBJET_ETIQUETTES> TOBJE_OBJET_ETIQUETTES { get; set; }
        public virtual DbSet<TOBN_OBJ_NATURES> TOBN_OBJ_NATURES { get; set; }
        public virtual DbSet<TPRCP_PRC_PERIMETRES> TPRCP_PRC_PERIMETRES { get; set; }
        public virtual DbSet<TSEQ_SUIVI_EVENEMENT_ETQS> TSEQ_SUIVI_EVENEMENT_ETQS { get; set; }
        public virtual DbSet<TTE_TYPE_EVENEMENTS> TTE_TYPE_EVENEMENTS { get; set; }
        public virtual DbSet<TTR_TYPE_RESSOURCES> TTR_TYPE_RESSOURCES { get; set; }
        public virtual DbSet<TSR_SUIVI_RESSOURCES> TSR_SUIVI_RESSOURCES { get; set; }
        public virtual DbSet<ETQ_TM_AET_Authorization> ETQ_TM_AET_Authorization { get; set; }

        public virtual DbSet<TACT_ACTIONS> TACT_ACTIONS { get; set; }
        public virtual DbSet<TETQR_ETQ_REGLES> TETQR_ETQ_REGLES { get; set; }
        public virtual DbSet<TOBJR_OBJET_REGLES> TOBJR_OBJET_REGLES { get; set; }
        public virtual DbSet<TRGL_REGLES> TRGL_REGLES { get; set; }
        public virtual DbSet<TRGLI_REGLES_LIEES> TRGLI_REGLES_LIEES { get; set; }
        public virtual DbSet<TRGLRV_REGLES_VALEURS> TRGLRV_REGLES_VALEURS { get; set; }

        public virtual DbSet<TTDOM_TYPE_DOMAINES> TTDOM_TYPE_DOMAINES { get; set; }
        public virtual DbSet<TDOM_DOMAINES> TDOM_DOMAINES { get; set; }

        #region accueil
        public virtual DbSet<VACCGET_ACCUEIL_GRAPHE_ETQS> VACCGET_ACCUEIL_GRAPHE_ETQS { get; set; }

        #endregion accueil

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyAllConfigurations<KrialysDbContext>();

            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            /* le rajout des FK est important pour pouvoir définir l effet des opérations de delete d une table parent sur table enfant */
            /* par defaut la suppression est en cascade */
            /* utiliser  .OnDelete(DeleteBehavior.NoAction) pour garantir de ne pas pouvoir supprimer si valeur utilisée sur une table fille */

            modelBuilder.Entity<TETQ_ETIQUETTES>(entity =>
            {
                entity.HasOne(d => d.TOBJE_OBJET_ETIQUETTE)
                    .WithMany(p => p.TETQ_ETIQUETTES)
                    .HasForeignKey(d => d.TOBJE_OBJET_ETIQUETTEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TOBJE_OBJET_ETIQUETTES");

                entity.HasOne(d => d.TPRCP_PRC_PERIMETRE)
                    .WithMany(p => p.TETQ_ETIQUETTES)
                    .HasForeignKey(d => d.TPRCP_PRC_PERIMETREID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TPRCP_PRC_PERIMETRES");
            });

            modelBuilder.Entity<TOBJE_OBJET_ETIQUETTES>(entity =>
            {
                entity.HasOne(d => d.TEQC_ETQ_CODIF)
                    .WithMany(p => p.TOBJE_OBJET_ETIQUETTES)
                    .HasForeignKey(d => d.TEQC_ETQ_CODIFID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TEQC_ETQ_CODIFS");

                entity.HasOne(d => d.TOBF_OBJ_FORMAT)
                    .WithMany(p => p.TOBJE_OBJET_ETIQUETTES)
                    .HasForeignKey(d => d.TOBF_OBJ_FORMATID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TOBF_OBJ_FORMATS");

                entity.HasOne(d => d.TOBN_OBJ_NATURE)
                    .WithMany(p => p.TOBJE_OBJET_ETIQUETTES)
                    .HasForeignKey(d => d.TOBN_OBJ_NATUREID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TOBN_OBJ_NATURES");

                entity.HasOne(d => d.TDOM_DOMAINE)
                    .WithMany(p => p.TOBJE_OBJET_ETIQUETTES)
                    .HasForeignKey(d => d.TDOM_DOMAINEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES");
            });

            modelBuilder.Entity<TPRCP_PRC_PERIMETRES>(entity =>
            {
                entity.HasOne(d => d.TDOM_DOMAINES)
                    .WithMany(p => p.TPRCP_PRC_PERIMETRES)
                    .HasForeignKey(d => d.TDOM_DOMAINEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES");
            });

            //modelBuilder.Entity<TPRS_PROCESSUS>(entity =>
            //{
            //    entity.HasOne(d => d.TTPR_TYPE_PROCESSUS)
            //        .WithMany(p => p.TPRS_PROCESSUS)
            //        .HasForeignKey(d => d.TTPR_TYPE_PROCESSUSID)
            //        .HasConstraintName("FK_TTPR_TYPE_PROCESSUS");
            //});

            modelBuilder.Entity<TSEQ_SUIVI_EVENEMENT_ETQS>(entity =>
            {
                entity.HasOne(d => d.TETQ_ETIQUETTE)
                    .WithMany(p => p.TSEQ_SUIVI_EVENEMENT_ETQS)
                    .HasForeignKey(d => d.TETQ_ETIQUETTEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TETQ_ETIQUETTES");

                entity.HasOne(d => d.TTE_TYPE_EVENEMENT)
                    .WithMany(p => p.TSEQ_SUIVI_EVENEMENT_ETQS)
                    .HasForeignKey(d => d.TTE_TYPE_EVENEMENTID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TTE_TYPE_EVENEMENTS");
            });

            modelBuilder.Entity<TETQR_ETQ_REGLES>(entity =>
            {
                entity.HasOne(d => d.TETQ_ETIQUETTE)
                    .WithMany(p => p.TETQR_ETQ_REGLES)
                    .HasForeignKey(d => d.TETQ_ETIQUETTEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE");

                entity.HasOne(d => d.TRGL_REGLE)
                   .WithMany(p => p.TETQR_ETQ_REGLES)
                   .HasForeignKey(d => d.TRGL_REGLEID)
                   .OnDelete(DeleteBehavior.NoAction)
                   .HasConstraintName("FK_TETQR_ETQ_REGLES_TRGL_REGLE");

                entity.HasOne(d => d.TRGLRV_REGLES_VALEUR)
                    .WithMany(p => p.TETQR_ETQ_REGLESS)
                    .HasForeignKey(d => d.TRGLRV_REGLES_VALEURID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR");
            });

            modelBuilder.Entity<TOBJR_OBJET_REGLES>(entity =>
            {
                entity.HasOne(d => d.TOBJE_OBJET_ETIQUETTE)
                    .WithMany(p => p.TOBJR_OBJET_REGLES)
                    .HasForeignKey(d => d.TOBJE_OBJET_ETIQUETTEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES");

                entity.HasOne(d => d.TRGL_REGLES)
                   .WithMany(p => p.TOBJR_OBJET_REGLES)
                   .HasForeignKey(d => d.TRGL_REGLEID)
                   .OnDelete(DeleteBehavior.NoAction)
                   .HasConstraintName("FK_TOBJR_OBJET_REGLES_TRGL_REGLES");

                entity.HasOne(d => d.TRGLRV_REGLES_VALEUR)
                   .WithMany(p => p.TOBJR_OBJET_REGLE)
                   .HasForeignKey(d => d.TRGLRV_REGLES_VALEURID)
                   .OnDelete(DeleteBehavior.NoAction)
                   .HasConstraintName("FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEUR");
            });

            modelBuilder.Entity<TSR_SUIVI_RESSOURCES>(entity =>
            {
                entity.HasOne(d => d.TTR_TYPE_RESSOURCE)
                    .WithMany(p => p.TSR_SUIVI_RESSOURCES)
                    .HasForeignKey(d => d.TTR_TYPE_RESSOURCEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCE");

                entity.HasOne(d => d.TETQ_ETIQUETTE)
                   .WithMany(p => p.TSR_SUIVI_RESSOURCES)
                   .HasForeignKey(d => d.TETQ_ETIQUETTEID)
                   .OnDelete(DeleteBehavior.NoAction)
                   .HasConstraintName("FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTE");
            });

            modelBuilder.Entity<ETQ_TM_AET_Authorization>(entity =>
            {
                entity.HasOne(d => d.TETQ_ETIQUETTE)
                    .WithMany(p => p.ETQ_TM_AET_Authorization)
                    .HasForeignKey(d => d.aet_etiquette_id)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_ETQ_TM_AET_Authorization_TETQ_ETIQUETTE");
            });

            modelBuilder.Entity<TDOM_DOMAINES>(entity =>
            {
                entity.HasOne(d => d.TTDOM_TYPE_DOMAINES)
                    .WithMany(p => p.TDOM_DOMAINES)
                    .HasForeignKey(d => d.TTDOM_TYPE_DOMAINEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES");
            });

            modelBuilder.Entity<TRGLRV_REGLES_VALEURS>(entity =>
            {
                entity.HasOne(d => d.TRGL_REGLE)
                    .WithMany(p => p.TRGLRV_REGLES_VALEURS)
                    .HasForeignKey(d => d.TRGL_REGLEID)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_TRGLRV_REGLES_VALEURS_TRGL_REGLE");

                entity.HasOne(d => d.TACT_ACTION)
                   .WithMany(p => p.TRGLRV_REGLES_VALEURS)
                   .HasForeignKey(d => d.TACT_ACTIONID)
                   .OnDelete(DeleteBehavior.NoAction)
                   .HasConstraintName("FK_TRGLRV_REGLES_VALEURS_TACT_ACTION");

            });

            modelBuilder.Entity<VACCGET_ACCUEIL_GRAPHE_ETQS>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.PERIODE
                })
                  .HasName("UQ_VACCGET_ACCUEIL_GRAPHE_ETQS");

                entity.ToView(nameof(VACCGET_ACCUEIL_GRAPHE_ETQS));
            });

            OnModelCreatingPartial(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}