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
    [Migration("20210615162419_InitialDbEtqCreation")]
    partial class InitialDbEtqCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS")
                .HasAnnotation("ProductVersion", "5.0.7");

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

                    b.Property<string>("TETQ_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TETQ_LIB")
                        .HasMaxLength(100)
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

                    b.Property<int>("TEQC_ETQ_CODIFID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TOBF_OBJ_FORMATID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TOBJE_CODE")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBJE_CODE_ETIQUETTAGE")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("TOBJE_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBJE_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TOBJE_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("TOBJE_VERSION")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TOBJE_VERSION_ETQ_STATUT")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TOBN_OBJ_NATUREID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TPRS_PROCESSUSID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRU_ACTEURID")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TOBJE_OBJET_ETIQUETTEID");

                    b.HasIndex("TEQC_ETQ_CODIFID");

                    b.HasIndex("TOBF_OBJ_FORMATID");

                    b.HasIndex("TOBN_OBJ_NATUREID");

                    b.HasIndex(new[] { "TOBJE_CODE" }, "UK_TOBJE_OBJET_ETIQUETTES")
                        .IsUnique();

                    b.HasIndex(new[] { "TPRS_PROCESSUSID", "TOBJE_CODE_ETIQUETTAGE" }, "UK_TOBJE_OBJET_ETIQUETTESBIS")
                        .IsUnique();

                    b.ToTable("TOBJE_OBJET_ETIQUETTES");
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

                    b.Property<string>("TPRCP_CODE")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("TPRCP_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRCP_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRCP_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("TPRS_PROCESSUSID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRU_ACTEURID")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TPRCP_PRC_PERIMETREID");

                    b.HasIndex("TPRS_PROCESSUSID");

                    b.HasIndex(new[] { "TPRCP_CODE", "TPRS_PROCESSUSID" }, "UK_TPRCP_PRC_PERIMETRES")
                        .IsUnique();

                    b.ToTable("TPRCP_PRC_PERIMETRES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRS_PROCESSUS", b =>
                {
                    b.Property<int>("TPRS_PROCESSUSID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TPRS_CODE")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("TPRS_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRS_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TPRS_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRU_ACTEURID")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TTPR_TYPE_PROCESSUSID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TPRS_PROCESSUSID");

                    b.HasIndex("TTPR_TYPE_PROCESSUSID");

                    b.HasIndex(new[] { "TPRS_CODE" }, "UK_TPRS_PROCESSUS")
                        .IsUnique();

                    b.ToTable("TPRS_PROCESSUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TSEQ_SUIVI_EVENEMENT_ETQS", b =>
                {
                    b.Property<int>("TSEQ_SUIVI_EVENEMENT_ETQID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TETQ_ETIQUETTEID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRU_ACTEURID")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("TSEQ_DATE_EVENEMENT")
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

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTPR_TYPE_PROCESSUS", b =>
                {
                    b.Property<int>("TTPR_TYPE_PROCESSUSID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTPR_CODE")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTPR_DESC")
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTPR_LIB")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TTPR_TYPE_PROCESSUSID");

                    b.HasIndex(new[] { "TTPR_CODE" }, "UK_TTPR_TYPE_PROCESSUS")
                        .IsUnique();

                    b.ToTable("TTPR_TYPE_PROCESSUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", "TOBJE_OBJET_ETIQUETTE")
                        .WithMany("TETQ_ETIQUETTES")
                        .HasForeignKey("TOBJE_OBJET_ETIQUETTEID")
                        .HasConstraintName("FK_TOBJE_OBJET_ETIQUETTES")
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", "TPRCP_PRC_PERIMETRE")
                        .WithMany("TETQ_ETIQUETTES")
                        .HasForeignKey("TPRCP_PRC_PERIMETREID")
                        .HasConstraintName("FK_TPRCP_PRC_PERIMETRES");

                    b.Navigation("TOBJE_OBJET_ETIQUETTE");

                    b.Navigation("TPRCP_PRC_PERIMETRE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TEQC_ETQ_CODIFS", "TEQC_ETQ_CODIF")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TEQC_ETQ_CODIFID")
                        .HasConstraintName("FK_TEQC_ETQ_CODIFS")
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TOBF_OBJ_FORMATS", "TOBF_OBJ_FORMAT")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TOBF_OBJ_FORMATID")
                        .HasConstraintName("FK_TOBF_OBJ_FORMATS")
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TOBN_OBJ_NATURES", "TOBN_OBJ_NATURE")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TOBN_OBJ_NATUREID")
                        .HasConstraintName("FK_TOBN_OBJ_NATURES")
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TPRS_PROCESSUS", "TPRS_PROCESSUS")
                        .WithMany("TOBJE_OBJET_ETIQUETTES")
                        .HasForeignKey("TPRS_PROCESSUSID")
                        .HasConstraintName("FK_TPRS_PROCESSUS")
                        .IsRequired();

                    b.Navigation("TEQC_ETQ_CODIF");

                    b.Navigation("TOBF_OBJ_FORMAT");

                    b.Navigation("TOBN_OBJ_NATURE");

                    b.Navigation("TPRS_PROCESSUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TPRS_PROCESSUS", "TPRS_PROCESSUS")
                        .WithMany("TPRCP_PRC_PERIMETRES")
                        .HasForeignKey("TPRS_PROCESSUSID")
                        .HasConstraintName("FK_TPRS_PROCESSUS_TPRCP_PRC_PERIMETRES")
                        .IsRequired();

                    b.Navigation("TPRS_PROCESSUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRS_PROCESSUS", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TTPR_TYPE_PROCESSUS", "TTPR_TYPE_PROCESSUS")
                        .WithMany("TPRS_PROCESSUS")
                        .HasForeignKey("TTPR_TYPE_PROCESSUSID")
                        .HasConstraintName("FK_TTPR_TYPE_PROCESSUS");

                    b.Navigation("TTPR_TYPE_PROCESSUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TSEQ_SUIVI_EVENEMENT_ETQS", b =>
                {
                    b.HasOne("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", "TETQ_ETIQUETTE")
                        .WithMany("TSEQ_SUIVI_EVENEMENT_ETQS")
                        .HasForeignKey("TETQ_ETIQUETTEID")
                        .HasConstraintName("FK_TETQ_ETIQUETTES")
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Etq.TTE_TYPE_EVENEMENTS", "TTE_TYPE_EVENEMENT")
                        .WithMany("TSEQ_SUIVI_EVENEMENT_ETQS")
                        .HasForeignKey("TTE_TYPE_EVENEMENTID")
                        .HasConstraintName("FK_TTE_TYPE_EVENEMENTS")
                        .IsRequired();

                    b.Navigation("TETQ_ETIQUETTE");

                    b.Navigation("TTE_TYPE_EVENEMENT");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TEQC_ETQ_CODIFS", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TETQ_ETIQUETTES", b =>
                {
                    b.Navigation("TSEQ_SUIVI_EVENEMENT_ETQS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBF_OBJ_FORMATS", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBJE_OBJET_ETIQUETTES", b =>
                {
                    b.Navigation("TETQ_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TOBN_OBJ_NATURES", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRCP_PRC_PERIMETRES", b =>
                {
                    b.Navigation("TETQ_ETIQUETTES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TPRS_PROCESSUS", b =>
                {
                    b.Navigation("TOBJE_OBJET_ETIQUETTES");

                    b.Navigation("TPRCP_PRC_PERIMETRES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTE_TYPE_EVENEMENTS", b =>
                {
                    b.Navigation("TSEQ_SUIVI_EVENEMENT_ETQS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Etq.TTPR_TYPE_PROCESSUS", b =>
                {
                    b.Navigation("TPRS_PROCESSUS");
                });
#pragma warning restore 612, 618
        }
    }
}
