﻿// <auto-generated />
using System;
using Krialys.Data.EF.Etq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Krialys.Entities.Migrations.ETQ
{
    [DbContext(typeof(KrialysDbContext))]
    [Migration("20220318114714_DbEtqRev16")]
    partial class DbEtqRev16
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS")
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("Krialys.Data.EF.Etq.TACT_ACTIONS", b =>
                {
                    b.Property<int>("TACT_ACTIONID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TACT_CODE")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("TACT_DESC")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TACT_LIB")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("TACT_ACTIONID");

                    b.HasIndex(new[] { "TACT_CODE" }, "UK_TACT_CODE")
                        .IsUnique();

                    b.ToTable("TACT_ACTIONS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TDOM_DOMAINES", b =>
                {
                    b.Property<int>("TDOM_DOMAINEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TDOM_CODE")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TDOM_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TDOM_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TDOM_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRU_ACTEURID")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TTDOM_TYPE_DOMAINEID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TDOM_DOMAINEID");

                    b.HasIndex("TTDOM_TYPE_DOMAINEID");

                    b.HasIndex(new[] { "TDOM_CODE" }, "UK_TDOM_DOMAINES")
                        .IsUnique();

                    b.ToTable("TDOM_DOMAINES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TEQC_ETQ_CODIFS", b =>
                {
                    b.Property<int>("TEQC_ETQ_CODIFID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TEQC_CODE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TEQC_CODE_PRC_ORDRE")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TEQC_CODE_PRM_ORDRE")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TEQC_INCREMENT_ORDRE")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TEQC_INCREMENT_TAILLE")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TEQC_INCREMENT_VAL_INIT")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TEQC_SEPARATEUR")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.HasKey("TEQC_ETQ_CODIFID");

                    b.HasIndex(new[] { "TEQC_CODE" }, "UK_TEQC_ETQ_CODIFS")
                        .IsUnique();

                    b.ToTable("TEQC_ETQ_CODIFS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQR_ETQ_REGLES", b =>
                {
                    b.Property<int>("TETQR_ETQ_REGLEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("TETQR_DATE_DEBUT")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("TETQR_DATE_FIN")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("TETQR_ECHEANCE")
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQR_ETQ_REGLES_ACTION")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQR_LIMITE_ATTEINTE")
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQR_REGLE_LIEE")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<int>("TETQ_ETIQUETTEID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRGLRV_REGLES_VALEURID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRGL_REGLEID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TETQR_ETQ_REGLEID");

                    b.HasIndex("TRGLRV_REGLES_VALEURID");

                    b.HasIndex("TRGL_REGLEID");

                    b.HasIndex(new[] { "TETQ_ETIQUETTEID", "TRGL_REGLEID" }, "UK_TETQR_ETQ_REGLES")
                        .IsUnique();

                    b.ToTable("TETQR_ETQ_REGLES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", b =>
                {
                    b.Property<int>("TETQ_ETIQUETTEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DEMANDEID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TETQ_CODE")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TETQ_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQ_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<int>("TETQ_INCREMENT")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TETQ_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQ_PATTERN")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQ_PRM_VAL")
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TETQ_VERSION_ETQ")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TOBJE_OBJET_ETIQUETTEID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TPRCP_PRC_PERIMETREID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TETQ_ETIQUETTEID");

                    b.HasIndex("TOBJE_OBJET_ETIQUETTEID");

                    b.HasIndex("TPRCP_PRC_PERIMETREID");

                    b.HasIndex(new[] { "TETQ_CODE" }, "UK_TETQ_ETIQUETTES")
                        .IsUnique();

                    b.ToTable("TETQ_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBF_OBJ_FORMATS", b =>
                {
                    b.Property<int>("TOBF_OBJ_FORMATID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TOBF_CODE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBF_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBF_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TOBF_OBJ_FORMATID");

                    b.HasIndex(new[] { "TOBF_CODE" }, "UK_TOBF_OBJ_FORMATS")
                        .IsUnique();

                    b.ToTable("TOBF_OBJ_FORMATS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", b =>
                {
                    b.Property<int>("TOBJE_OBJET_ETIQUETTEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TDOM_DOMAINEID")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TEQC_ETQ_CODIFID")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TOBF_OBJ_FORMATID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TOBJE_CODE")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBJE_CODE_ETIQUETTAGE")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TOBJE_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBJE_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBJE_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TOBJE_VERSION")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TOBJE_VERSION_ETQ_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TOBJE_VERSION_ETQ_STATUT")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TOBN_OBJ_NATUREID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRU_ACTEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TOBJE_OBJET_ETIQUETTEID");

                    b.HasIndex("TEQC_ETQ_CODIFID");

                    b.HasIndex("TOBF_OBJ_FORMATID");

                    b.HasIndex("TOBN_OBJ_NATUREID");

                    b.HasIndex(new[] { "TOBJE_CODE", "TOBJE_VERSION" }, "UK_TOBJE_OBJET_ETIQUETTES")
                        .IsUnique();

                    b.HasIndex(new[] { "TDOM_DOMAINEID", "TOBJE_CODE_ETIQUETTAGE", "TOBJE_VERSION" }, "UK_TOBJE_OBJET_ETIQUETTESBIS")
                        .IsUnique();

                    b.ToTable("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJR_OBJET_REGLES", b =>
                {
                    b.Property<int>("TOBJR_OBJET_REGLEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TOBJE_OBJET_ETIQUETTEID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TOBJR_APPLICABLE")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TOBJR_ECHEANCE_DUREE")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRGLRV_REGLES_VALEURID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRGL_REGLEID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TOBJR_OBJET_REGLEID");

                    b.HasIndex("TRGLRV_REGLES_VALEURID");

                    b.HasIndex("TRGL_REGLEID");

                    b.HasIndex(new[] { "TOBJE_OBJET_ETIQUETTEID", "TRGL_REGLEID" }, "UK_TOBJR_OBJET_REGLES")
                        .IsUnique();

                    b.ToTable("TOBJR_OBJET_REGLES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBN_OBJ_NATURES", b =>
                {
                    b.Property<int>("TOBN_OBJ_NATUREID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TOBN_CODE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBN_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBN_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TOBN_OBJ_NATUREID");

                    b.HasIndex(new[] { "TOBN_CODE" }, "UK_TOBN_OBJ_NATURES")
                        .IsUnique();

                    b.ToTable("TOBN_OBJ_NATURES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", b =>
                {
                    b.Property<int>("TPRCP_PRC_PERIMETREID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TDOM_DOMAINEID")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TPRCP_CODE")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TPRCP_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRCP_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRCP_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRCP_PRM_DYN")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRU_ACTEURID")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TPRCP_PRC_PERIMETREID");

                    b.HasIndex("TDOM_DOMAINEID");

                    b.HasIndex(new[] { "TPRCP_CODE", "TDOM_DOMAINEID" }, "UK_TPRCP_PRC_PERIMETRES")
                        .IsUnique();

                    b.ToTable("TPRCP_PRC_PERIMETRES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGLI_REGLES_LIEES", b =>
                {
                    b.Property<int>("TRGL_REGLELIEEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRGLRV_REGLES_VALEURID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRGLRV_REGLES_VALEURLIEEID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TRGL_REGLELIEEID");

                    b.HasIndex("TRGLRV_REGLES_VALEURLIEEID");

                    b.HasIndex(new[] { "TRGLRV_REGLES_VALEURID", "TRGLRV_REGLES_VALEURLIEEID" }, "UK_TRGLI_REGLES_LIEES")
                        .IsUnique();

                    b.ToTable("TRGLI_REGLES_LIEES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", b =>
                {
                    b.Property<int>("TRGLRV_REGLES_VALEURID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TACT_ACTIONID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRGLRV_DEPART_LIMITE_TEMPS")
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<int>("TRGLRV_ORDRE_AFFICHAGE")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRGLRV_VALEUR")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRGLRV_VALEUR_DEFAUT")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRGLRV_VALEUR_ECHEANCE")
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<int>("TRGL_REGLEID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TRGLRV_REGLES_VALEURID");

                    b.HasIndex("TACT_ACTIONID");

                    b.HasIndex(new[] { "TRGL_REGLEID", "TRGLRV_VALEUR" }, "UK_TRGLRV_REGLES_VALEURS")
                        .IsUnique();

                    b.ToTable("TRGLRV_REGLES_VALEURS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGL_REGLES", b =>
                {
                    b.Property<int>("TRGL_REGLEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRGL_CODE_REGLE")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRGL_DESC_REGLE")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRGL_LIB_REGLE")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRGL_LIMITE_TEMPS")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("TRGL_REGLEID");

                    b.HasIndex(new[] { "TRGL_CODE_REGLE" }, "UK_TRGL_REGLES")
                        .IsUnique();

                    b.ToTable("TRGL_REGLES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TSEQ_SUIVI_EVENEMENT_ETQS", b =>
                {
                    b.Property<int>("TSEQ_SUIVI_EVENEMENT_ETQID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TETQ_ETIQUETTEID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRU_ACTEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<string>("TSEQ_COMMENTAIRE")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TSEQ_DATE_EVENEMENT")
                        .HasColumnType("TEXT");

                    b.Property<string>("TSEQ_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<int>("TTE_TYPE_EVENEMENTID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TSEQ_SUIVI_EVENEMENT_ETQID");

                    b.HasIndex("TETQ_ETIQUETTEID");

                    b.HasIndex("TTE_TYPE_EVENEMENTID");

                    b.ToTable("TSEQ_SUIVI_EVENEMENT_ETQS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TSR_SUIVI_RESSOURCES", b =>
                {
                    b.Property<int>("TSR_SUIVI_RESSOURCEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TETQ_ETIQUETTEID")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TETQ_ETIQUETTE_ENTREEID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TSR_ENTREE")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("TSR_VALEUR_ENTREE")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TTR_TYPE_RESSOURCEID")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.HasKey("TSR_SUIVI_RESSOURCEID");

                    b.HasIndex("TTR_TYPE_RESSOURCEID");

                    b.HasIndex(new[] { "TETQ_ETIQUETTEID", "TSR_ENTREE" }, "UK_TSR_SUIVI_RESSOURCES")
                        .IsUnique();

                    b.ToTable("TSR_SUIVI_RESSOURCES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTDOM_TYPE_DOMAINES", b =>
                {
                    b.Property<int>("TTDOM_TYPE_DOMAINEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTDOM_CODE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTDOM_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTDOM_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TTDOM_TYPE_DOMAINEID");

                    b.HasIndex(new[] { "TTDOM_CODE" }, "UK_TTDOM_TYPE_DOMAINES")
                        .IsUnique();

                    b.ToTable("TTDOM_TYPE_DOMAINES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTE_TYPE_EVENEMENTS", b =>
                {
                    b.Property<int>("TTE_TYPE_EVENEMENTID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTE_CODE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTE_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTE_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TTE_TYPE_EVENEMENTID");

                    b.HasIndex(new[] { "TTE_CODE" }, "UK_TTE_TYPE_EVENEMENTS")
                        .IsUnique();

                    b.ToTable("TTE_TYPE_EVENEMENTS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTR_TYPE_RESSOURCES", b =>
                {
                    b.Property<int>("TTR_TYPE_RESSOURCEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTR_TYPE_ENTREE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTR_TYPE_ENTREE_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTR_TYPE_ENTREE_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TTR_TYPE_RESSOURCEID");

                    b.HasIndex(new[] { "TTR_TYPE_ENTREE" }, "UK_TTR_TYPE_RESSOURCES")
                        .IsUnique();

                    b.ToTable("TTR_TYPE_RESSOURCES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TDOM_DOMAINES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TTDOM_TYPE_DOMAINES", "TTDOM_TYPE_DOMAINES")
                        .WithMany("TDOM_DOMAINES")
                        .HasForeignKey("TTDOM_TYPE_DOMAINEID")
                        .HasConstraintName("FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("TTDOM_TYPE_DOMAINES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQR_ETQ_REGLES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", "TETQ_ETIQUETTE")
                        .WithMany("TETQR_ETQ_REGLES")
                        .HasForeignKey("TETQ_ETIQUETTEID")
                        .HasConstraintName("FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", "TRGLRV_REGLES_VALEUR")
                        .WithMany("TETQR_ETQ_REGLESS")
                        .HasForeignKey("TRGLRV_REGLES_VALEURID")
                        .HasConstraintName("FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TRGL_REGLES", "TRGL_REGLE")
                        .WithMany("TETQR_ETQ_REGLES")
                        .HasForeignKey("TRGL_REGLEID")
                        .HasConstraintName("FK_TETQR_ETQ_REGLES_TRGL_REGLE")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TETQ_ETIQUETTE");

                    b.Navigation("TRGL_REGLE");

                    b.Navigation("TRGLRV_REGLES_VALEUR");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", "TOBJE_OBJET_ETIQUETTE")
                        .WithMany("TETQ_ETIQUETTES")
                        .HasForeignKey("TOBJE_OBJET_ETIQUETTEID")
                        .HasConstraintName("FK_TOBJE_OBJET_ETIQUETTES")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", "TPRCP_PRC_PERIMETRE")
                        .WithMany("TETQ_ETIQUETTES")
                        .HasForeignKey("TPRCP_PRC_PERIMETREID")
                        .HasConstraintName("FK_TPRCP_PRC_PERIMETRES")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("TOBJE_OBJET_ETIQUETTE");

                    b.Navigation("TPRCP_PRC_PERIMETRE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TDOM_DOMAINES", "TDOM_DOMAINE")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TDOM_DOMAINEID")
                        .HasConstraintName("FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TEQC_ETQ_CODIFS", "TEQC_ETQ_CODIF")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TEQC_ETQ_CODIFID")
                        .HasConstraintName("FK_TEQC_ETQ_CODIFS")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TOBF_OBJ_FORMATS", "TOBF_OBJ_FORMAT")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TOBF_OBJ_FORMATID")
                        .HasConstraintName("FK_TOBF_OBJ_FORMATS")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Krialys.Data.EF.Etq.TOBN_OBJ_NATURES", "TOBN_OBJ_NATURE")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TOBN_OBJ_NATUREID")
                        .HasConstraintName("FK_TOBN_OBJ_NATURES")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("TDOM_DOMAINE");

                    b.Navigation("TEQC_ETQ_CODIF");

                    b.Navigation("TOBF_OBJ_FORMAT");

                    b.Navigation("TOBN_OBJ_NATURE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJR_OBJET_REGLES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", "TOBJE_OBJET_ETIQUETTE")
                        .WithMany("TOBJR_OBJET_REGLES")
                        .HasForeignKey("TOBJE_OBJET_ETIQUETTEID")
                        .HasConstraintName("FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", "TRGLRV_REGLES_VALEUR")
                        .WithMany("TOBJR_OBJET_REGLE")
                        .HasForeignKey("TRGLRV_REGLES_VALEURID")
                        .HasConstraintName("FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEUR")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TRGL_REGLES", "TRGL_REGLES")
                        .WithMany("TOBJR_OBJET_REGLES")
                        .HasForeignKey("TRGL_REGLEID")
                        .HasConstraintName("FK_TOBJR_OBJET_REGLES_TRGL_REGLES")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TOBJE_OBJET_ETIQUETTE");

                    b.Navigation("TRGL_REGLES");

                    b.Navigation("TRGLRV_REGLES_VALEUR");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TDOM_DOMAINES", "TDOM_DOMAINES")
                        .WithMany("TPRCP_PRC_PERIMETRES")
                        .HasForeignKey("TDOM_DOMAINEID")
                        .HasConstraintName("FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TDOM_DOMAINES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGLI_REGLES_LIEES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", "TRGLRV_REGLES_VALEUR")
                        .WithMany("TRGLI_REGLES_LIEES_SRC")
                        .HasForeignKey("TRGLRV_REGLES_VALEURID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", "TRGLRV_REGLES_VALEUR_CIBLE")
                        .WithMany("TRGLI_REGLES_LIEES_CIBLE")
                        .HasForeignKey("TRGLRV_REGLES_VALEURLIEEID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TRGLRV_REGLES_VALEUR");

                    b.Navigation("TRGLRV_REGLES_VALEUR_CIBLE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TACT_ACTIONS", "TACT_ACTION")
                        .WithMany("TRGLRV_REGLES_VALEURS")
                        .HasForeignKey("TACT_ACTIONID");

                    b.HasOne("Krialys.Data.EF.Etq.TRGL_REGLES", "TRGL_REGLE")
                        .WithMany("TRGLRV_REGLES_VALEURS")
                        .HasForeignKey("TRGL_REGLEID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TACT_ACTION");

                    b.Navigation("TRGL_REGLE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TSEQ_SUIVI_EVENEMENT_ETQS", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", "TETQ_ETIQUETTE")
                        .WithMany("TSEQ_SUIVI_EVENEMENT_ETQS")
                        .HasForeignKey("TETQ_ETIQUETTEID")
                        .HasConstraintName("FK_TETQ_ETIQUETTES")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TTE_TYPE_EVENEMENTS", "TTE_TYPE_EVENEMENT")
                        .WithMany("TSEQ_SUIVI_EVENEMENT_ETQS")
                        .HasForeignKey("TTE_TYPE_EVENEMENTID")
                        .HasConstraintName("FK_TTE_TYPE_EVENEMENTS")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TETQ_ETIQUETTE");

                    b.Navigation("TTE_TYPE_EVENEMENT");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TSR_SUIVI_RESSOURCES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", "TETQ_ETIQUETTE")
                        .WithMany("TSR_SUIVI_RESSOURCES")
                        .HasForeignKey("TETQ_ETIQUETTEID")
                        .HasConstraintName("FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTE")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TTR_TYPE_RESSOURCES", "TTR_TYPE_RESSOURCE")
                        .WithMany("TSR_SUIVI_RESSOURCES")
                        .HasForeignKey("TTR_TYPE_RESSOURCEID")
                        .HasConstraintName("FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCE")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TETQ_ETIQUETTE");

                    b.Navigation("TTR_TYPE_RESSOURCE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TACT_ACTIONS", b =>
                {
                    b.Navigation("TRGLRV_REGLES_VALEURS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TDOM_DOMAINES", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");

                    b.Navigation("TPRCP_PRC_PERIMETRES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TEQC_ETQ_CODIFS", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", b =>
                {
                    b.Navigation("TETQR_ETQ_REGLES");

                    b.Navigation("TSEQ_SUIVI_EVENEMENT_ETQS");

                    b.Navigation("TSR_SUIVI_RESSOURCES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBF_OBJ_FORMATS", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", b =>
                {
                    b.Navigation("TETQ_ETIQUETTES");

                    b.Navigation("TOBJR_OBJET_REGLES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBN_OBJ_NATURES", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", b =>
                {
                    b.Navigation("TETQ_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGLRV_REGLES_VALEURS", b =>
                {
                    b.Navigation("TETQR_ETQ_REGLESS");

                    b.Navigation("TOBJR_OBJET_REGLE");

                    b.Navigation("TRGLI_REGLES_LIEES_CIBLE");

                    b.Navigation("TRGLI_REGLES_LIEES_SRC");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TRGL_REGLES", b =>
                {
                    b.Navigation("TETQR_ETQ_REGLES");

                    b.Navigation("TOBJR_OBJET_REGLES");

                    b.Navigation("TRGLRV_REGLES_VALEURS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTDOM_TYPE_DOMAINES", b =>
                {
                    b.Navigation("TDOM_DOMAINES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTE_TYPE_EVENEMENTS", b =>
                {
                    b.Navigation("TSEQ_SUIVI_EVENEMENT_ETQS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTR_TYPE_RESSOURCES", b =>
                {
                    b.Navigation("TSR_SUIVI_RESSOURCES");
                });
#pragma warning restore 612, 618
        }
    }
}
