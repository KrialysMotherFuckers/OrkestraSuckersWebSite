﻿// <auto-generated />
using System;
using Krialys.Data.EF.Mso;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Krialys.Entities.Migrations
{
    [DbContext(typeof(KrialysDbContext))]
    partial class DBCONTEXTModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRAPL_APPLICATIONS", b =>
                {
                    b.Property<int>("TRAPL_APPLICATIONID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRAPL_DESCRIPTION")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRAPL_LIB")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("TRAPL_APPLICATIONID");

                    b.ToTable("TRAPL_APPLICATIONS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRAP_ATTENDUS_PLANIFS", b =>
                {
                    b.Property<int>("TRAP_ATTENDU_PLANIFID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TRAP_DATE_MODIF")
                        .HasColumnType("TEXT");

                    b.Property<string>("TRAP_STATUT")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TRA_ATTENDUID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRP_PLANIFID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTU_MODIFICATEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TRAP_ATTENDU_PLANIFID");

                    b.HasIndex("TRP_PLANIFID");

                    b.HasIndex("TRA_ATTENDUID", "TRP_PLANIFID")
                        .IsUnique()
                        .HasDatabaseName("UK_TRAP_ATTENDUS_PLANIFS");

                    b.ToTable("TRAP_ATTENDUS_PLANIFS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRA_ATTENDUS", b =>
                {
                    b.Property<int>("TRA_ATTENDUID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRAPL_APPLICATIONID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRA_AUTEUR")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRA_CODE")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRA_COMMENTAIRE")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TRA_DEBUT_VALIDITE")
                        .HasColumnType("TEXT");

                    b.Property<string>("TRA_DESCRIPTION")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRA_DESTINATION")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TRA_DUREE_TRAITEMENT_MAX")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_DUREE_TRAITEMENT_MIN")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_FICHIER_AGE_MAX")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_FICHIER_AGE_MIN")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("TRA_FIN_VALIDITE")
                        .HasColumnType("TEXT");

                    b.Property<string>("TRA_MOTCLEF")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TRA_NB_LIGNES_ENTREE_MAX")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_NB_LIGNES_ENTREE_MIN")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_NB_LIGNES_SORTIE_MAX")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_NB_LIGNES_SORTIE_MIN")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRA_SOURCE")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRA_STATUT")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TRA_TAILLE_FICHIER_ENTREE_MAX")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_TAILLE_FICHIER_ENTREE_MIN")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_TAILLE_FICHIER_SORTIE_MAX")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_TAILLE_FICHIER_SORTIE_MIN")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRC_CONTRATID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRC_CRITICITEID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRNF_NATURE_DESTINATIONID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRNF_NATURE_ORIGINEID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRNT_NATURE_TRAITEMENTID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TRR_RESULTATID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRTT_TECHNO_TRAITEMENTID")
                        .HasColumnType("INTEGER");

                    b.HasKey("TRA_ATTENDUID");

                    b.HasIndex("TRAPL_APPLICATIONID");

                    b.HasIndex("TRC_CONTRATID");

                    b.HasIndex("TRC_CRITICITEID");

                    b.HasIndex("TRNF_NATURE_DESTINATIONID");

                    b.HasIndex("TRNF_NATURE_ORIGINEID");

                    b.HasIndex("TRNT_NATURE_TRAITEMENTID");

                    b.HasIndex("TRR_RESULTATID");

                    b.HasIndex("TRTT_TECHNO_TRAITEMENTID");

                    b.HasIndex("TRA_CODE", "TRA_DEBUT_VALIDITE", "TRA_FIN_VALIDITE")
                        .IsUnique()
                        .HasDatabaseName("UK_TRA_ATTENDUS");

                    b.ToTable("TRA_ATTENDUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRC_CONTRATS", b =>
                {
                    b.Property<int>("TRC_CONTRATID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRC_CONTRAT_CODE")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TRC_DATE_CREATION")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TRC_DATE_MODIF")
                        .HasColumnType("TEXT");

                    b.Property<string>("TRC_DATUM")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRC_LIB")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRC_LIBC")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRC_STATUT")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTU_CREATEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTU_MODIFICATEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TRC_CONTRATID");

                    b.HasIndex("TRC_CONTRAT_CODE")
                        .IsUnique()
                        .HasDatabaseName("UK_TRC_CONTRAT_CODE");

                    b.ToTable("TRC_CONTRATS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRC_CRITICITES", b =>
                {
                    b.Property<int>("TRC_CRITICITEID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRC_DESCRIPTION")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRC_LIB")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("TRC_CRITICITEID");

                    b.ToTable("TRC_CRITICITES");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRNF_NATURES_FLUX", b =>
                {
                    b.Property<int>("TRNF_NATURE_FLUXID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRNF_DESCRIPTION")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRNF_LIB")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("TRNF_NATURE_FLUXID");

                    b.ToTable("TRNF_NATURES_FLUX");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRNT_NATURES_TRAITEMENTS", b =>
                {
                    b.Property<int>("TRNT_NATURE_TRAITEMENTID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRNT_DESCRIPTION")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRNT_LIB")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("TRNT_NATURE_TRAITEMENTID");

                    b.ToTable("TRNT_NATURES_TRAITEMENTS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRP_PLANIFS", b =>
                {
                    b.Property<int>("TRP_PLANIFID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRP_CRON")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TRP_DATE_DEBUT_PLANIF")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("TRP_DATE_FIN_PLANIF")
                        .HasColumnType("TEXT");

                    b.Property<string>("TRP_DESCRIPTION")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRP_STATUT")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRP_TIMEZONE_INFOID")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTU_CREATEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTU_MODIFICATEURID")
                        .HasMaxLength(36)
                        .HasColumnType("TEXT");

                    b.HasKey("TRP_PLANIFID");

                    b.ToTable("TRP_PLANIFS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRR_RESULTATS", b =>
                {
                    b.Property<int>("TRR_RESULTATID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRR_CODE")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRR_DESCRIPTION")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("TRR_RESULTATID");

                    b.ToTable("TRR_RESULTATS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRTT_TECHNOS_TRAITEMENTS", b =>
                {
                    b.Property<int>("TRTT_TECHNO_TRAITEMENTID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRTT_DESCRIPTION")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRTT_LIB")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("TRTT_TECHNO_TRAITEMENTID");

                    b.ToTable("TRTT_TECHNOS_TRAITEMENTS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TTL_LOGS", b =>
                {
                    b.Property<int>("TTL_LOGID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TRA_ATTENDUID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TRA_CODE")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("TRC_CONTRAT_CODE")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TTED_DEMANDEID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TTL_CODE_ANOMALIE")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("TTL_DATE_DEBUT")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("TTL_DATE_FIN")
                        .HasColumnType("TEXT");

                    b.Property<string>("TTL_DYNAMIC_OBJECT")
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<string>("TTL_FICHIER_ACTEUR_MODIF")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("TTL_FICHIER_DATE_MODIF")
                        .HasColumnType("TEXT");

                    b.Property<string>("TTL_FICHIER_SOURCE")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TTL_GROUPEID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTL_INFO")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TTL_NB_LIGNES_ENTREE")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TTL_NB_LIGNES_SORTIE")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TTL_RESULTAT")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("('NA')");

                    b.Property<int?>("TTL_TAILLE_FICHIER_ENTREE")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TTL_TAILLE_FICHIER_SORTIE")
                        .HasColumnType("INTEGER");

                    b.HasKey("TTL_LOGID");

                    b.ToTable("TTL_LOGS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRAP_ATTENDUS_PLANIFS", b =>
                {
                    b.HasOne("Krialys.Data.EF.Mso.TRA_ATTENDUS", "TRA_ATTENDU")
                        .WithMany("TRAP_ATTENDUS_PLANIFS")
                        .HasForeignKey("TRA_ATTENDUID")
                        .HasConstraintName("FK_TRA_ATTENDUS")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Krialys.Data.EF.Mso.TRP_PLANIFS", "TRP_PLANIF")
                        .WithMany("TRAP_ATTENDUS_PLANIFS")
                        .HasForeignKey("TRP_PLANIFID")
                        .HasConstraintName("FK_TRP_PLANIFS")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("TRA_ATTENDU");

                    b.Navigation("TRP_PLANIF");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRA_ATTENDUS", b =>
                {
                    b.HasOne("Krialys.Data.EF.Mso.TRAPL_APPLICATIONS", "TRAPL_APPLICATION")
                        .WithMany("TRA_ATTENDUS")
                        .HasForeignKey("TRAPL_APPLICATIONID")
                        .HasConstraintName("FK_TRAPL_APPLICATIONS")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Mso.TRC_CONTRATS", "TRC_CONTRAT")
                        .WithMany("TRA_ATTENDUS")
                        .HasForeignKey("TRC_CONTRATID")
                        .HasConstraintName("FK_TRC_CONTRATS")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Krialys.Data.EF.Mso.TRC_CRITICITES", "TRC_CRITICITE")
                        .WithMany("TRA_ATTENDUS")
                        .HasForeignKey("TRC_CRITICITEID")
                        .HasConstraintName("FK_TRC_CRITICITES")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Krialys.Data.EF.Mso.TRNF_NATURES_FLUX", "TRNF_NATURE_DESTINATION")
                        .WithMany("TRA_ATTENDUSTRNF_NATURE_DESTINATION")
                        .HasForeignKey("TRNF_NATURE_DESTINATIONID")
                        .HasConstraintName("FK_TRNF_NATURES_FLUX_DEST")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Krialys.Data.EF.Mso.TRNF_NATURES_FLUX", "TRNF_NATURE_ORIGINE")
                        .WithMany("TRA_ATTENDUSTRNF_NATURE_ORIGINE")
                        .HasForeignKey("TRNF_NATURE_ORIGINEID")
                        .HasConstraintName("FK_TRNF_NATURES_FLUX_ORI")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Krialys.Data.EF.Mso.TRNT_NATURES_TRAITEMENTS", "TRNT_NATURE_TRAITEMENT")
                        .WithMany("TRA_ATTENDUS")
                        .HasForeignKey("TRNT_NATURE_TRAITEMENTID")
                        .HasConstraintName("FK_TRNT_NATURES_TRAITEMENTS")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Krialys.Data.EF.Mso.TRR_RESULTATS", "TRR_RESULTAT")
                        .WithMany("TRA_ATTENDUS")
                        .HasForeignKey("TRR_RESULTATID")
                        .HasConstraintName("FK_TRR_RESULTATS")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Krialys.Data.EF.Mso.TRTT_TECHNOS_TRAITEMENTS", "TRTT_TECHNO_TRAITEMENT")
                        .WithMany("TRA_ATTENDUS")
                        .HasForeignKey("TRTT_TECHNO_TRAITEMENTID")
                        .HasConstraintName("FK_TRTT_TECHNOS_TRAITEMENTS")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("TRAPL_APPLICATION");

                    b.Navigation("TRC_CONTRAT");

                    b.Navigation("TRC_CRITICITE");

                    b.Navigation("TRNF_NATURE_DESTINATION");

                    b.Navigation("TRNF_NATURE_ORIGINE");

                    b.Navigation("TRNT_NATURE_TRAITEMENT");

                    b.Navigation("TRR_RESULTAT");

                    b.Navigation("TRTT_TECHNO_TRAITEMENT");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRAPL_APPLICATIONS", b =>
                {
                    b.Navigation("TRA_ATTENDUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRA_ATTENDUS", b =>
                {
                    b.Navigation("TRAP_ATTENDUS_PLANIFS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRC_CONTRATS", b =>
                {
                    b.Navigation("TRA_ATTENDUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRC_CRITICITES", b =>
                {
                    b.Navigation("TRA_ATTENDUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRNF_NATURES_FLUX", b =>
                {
                    b.Navigation("TRA_ATTENDUSTRNF_NATURE_DESTINATION");

                    b.Navigation("TRA_ATTENDUSTRNF_NATURE_ORIGINE");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRNT_NATURES_TRAITEMENTS", b =>
                {
                    b.Navigation("TRA_ATTENDUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRP_PLANIFS", b =>
                {
                    b.Navigation("TRAP_ATTENDUS_PLANIFS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRR_RESULTATS", b =>
                {
                    b.Navigation("TRA_ATTENDUS");
                });

            modelBuilder.Entity("Krialys.Data.EF.Mso.TRTT_TECHNOS_TRAITEMENTS", b =>
                {
                    b.Navigation("TRA_ATTENDUS");
                });
#pragma warning restore 612, 618
        }
    }
}