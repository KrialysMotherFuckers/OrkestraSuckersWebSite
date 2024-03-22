using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbMso_001 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TRAPL_APPLICATIONS",
            columns: table => new
            {
                TRAPL_APPLICATIONID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRAPL_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRAPL_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRAPL_APPLICATIONS", x => x.TRAPL_APPLICATIONID);
            });

        migrationBuilder.CreateTable(
            name: "TRC_CONTRATS",
            columns: table => new
            {
                TRC_CONTRATID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRC_CONTRAT_CODE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRC_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TRC_LIBC = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRC_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRC_DATUM = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                TRC_DATE_CREATION = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRC_DATE_MODIF = table.Column<DateTime>(type: "TEXT", nullable: false),
                TTU_CREATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TTU_MODIFICATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRC_CONTRATS", x => x.TRC_CONTRATID);
            });

        migrationBuilder.CreateTable(
            name: "TRC_CRITICITES",
            columns: table => new
            {
                TRC_CRITICITEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRC_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRC_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRC_CRITICITES", x => x.TRC_CRITICITEID);
            });

        migrationBuilder.CreateTable(
            name: "TRNF_NATURES_FLUX",
            columns: table => new
            {
                TRNF_NATURE_FLUXID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRNF_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRNF_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRNF_NATURES_FLUX", x => x.TRNF_NATURE_FLUXID);
            });

        migrationBuilder.CreateTable(
            name: "TRNT_NATURES_TRAITEMENTS",
            columns: table => new
            {
                TRNT_NATURE_TRAITEMENTID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRNT_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRNT_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRNT_NATURES_TRAITEMENTS", x => x.TRNT_NATURE_TRAITEMENTID);
            });

        migrationBuilder.CreateTable(
            name: "TRP_PLANIFS",
            columns: table => new
            {
                TRP_PLANIFID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRP_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TRP_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRP_CRON = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TTU_CREATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TRP_DATE_DEBUT_PLANIF = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRP_DATE_FIN_PLANIF = table.Column<DateTime>(type: "TEXT", nullable: true),
                TRP_TIMEZONE_INFOID = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TTU_MODIFICATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRP_PLANIFS", x => x.TRP_PLANIFID);
            });

        migrationBuilder.CreateTable(
            name: "TRR_RESULTATS",
            columns: table => new
            {
                TRR_RESULTATID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRR_CODE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRR_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRR_RESULTATS", x => x.TRR_RESULTATID);
            });

        migrationBuilder.CreateTable(
            name: "TRTT_TECHNOS_TRAITEMENTS",
            columns: table => new
            {
                TRTT_TECHNO_TRAITEMENTID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRTT_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRTT_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRTT_TECHNOS_TRAITEMENTS", x => x.TRTT_TECHNO_TRAITEMENTID);
            });

        migrationBuilder.CreateTable(
            name: "TTL_LOGS",
            columns: table => new
            {
                TTL_LOGID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRA_ATTENDUID = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_CODE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TTED_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_DATE_DEBUT = table.Column<DateTime>(type: "TEXT", nullable: true),
                TTL_DATE_FIN = table.Column<DateTime>(type: "TEXT", nullable: true),
                TTL_RESULTAT = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true, defaultValueSql: "('NA')"),
                TTL_INFO = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TTL_CODE_ANOMALIE = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_GROUPEID = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_FICHIER_SOURCE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TTL_NB_LIGNES_ENTREE = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_NB_LIGNES_SORTIE = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_TAILLE_FICHIER_ENTREE = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_TAILLE_FICHIER_SORTIE = table.Column<int>(type: "INTEGER", nullable: true),
                TTL_FICHIER_DATE_MODIF = table.Column<DateTime>(type: "TEXT", nullable: true),
                TTL_FICHIER_ACTEUR_MODIF = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRC_CONTRAT_CODE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TTL_DYNAMIC_OBJECT = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTL_LOGS", x => x.TTL_LOGID);
            });

        migrationBuilder.CreateTable(
            name: "TRA_ATTENDUS",
            columns: table => new
            {
                TRA_ATTENDUID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRA_CODE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRAPL_APPLICATIONID = table.Column<int>(type: "INTEGER", nullable: false),
                TRA_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRA_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRR_RESULTATID = table.Column<int>(type: "INTEGER", nullable: false),
                TRC_CRITICITEID = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_SOURCE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRA_DESTINATION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRNF_NATURE_ORIGINEID = table.Column<int>(type: "INTEGER", nullable: true),
                TRNF_NATURE_DESTINATIONID = table.Column<int>(type: "INTEGER", nullable: true),
                TRNT_NATURE_TRAITEMENTID = table.Column<int>(type: "INTEGER", nullable: true),
                TRTT_TECHNO_TRAITEMENTID = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_NB_LIGNES_ENTREE_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_NB_LIGNES_ENTREE_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_NB_LIGNES_SORTIE_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_NB_LIGNES_SORTIE_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_TAILLE_FICHIER_ENTREE_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_TAILLE_FICHIER_ENTREE_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_TAILLE_FICHIER_SORTIE_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_TAILLE_FICHIER_SORTIE_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_DUREE_TRAITEMENT_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_DUREE_TRAITEMENT_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_FICHIER_AGE_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_FICHIER_AGE_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_AUTEUR = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TRA_DEBUT_VALIDITE = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRA_FIN_VALIDITE = table.Column<DateTime>(type: "TEXT", nullable: true),
                TRA_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TRC_CONTRATID = table.Column<int>(type: "INTEGER", nullable: true),
                TRA_MOTCLEF = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRA_ATTENDUS", x => x.TRA_ATTENDUID);
                table.ForeignKey(
                    name: "FK_TRAPL_APPLICATIONS",
                    column: x => x.TRAPL_APPLICATIONID,
                    principalTable: "TRAPL_APPLICATIONS",
                    principalColumn: "TRAPL_APPLICATIONID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRC_CONTRATS",
                    column: x => x.TRC_CONTRATID,
                    principalTable: "TRC_CONTRATS",
                    principalColumn: "TRC_CONTRATID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRC_CRITICITES",
                    column: x => x.TRC_CRITICITEID,
                    principalTable: "TRC_CRITICITES",
                    principalColumn: "TRC_CRITICITEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRNF_NATURES_FLUX_DEST",
                    column: x => x.TRNF_NATURE_DESTINATIONID,
                    principalTable: "TRNF_NATURES_FLUX",
                    principalColumn: "TRNF_NATURE_FLUXID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRNF_NATURES_FLUX_ORI",
                    column: x => x.TRNF_NATURE_ORIGINEID,
                    principalTable: "TRNF_NATURES_FLUX",
                    principalColumn: "TRNF_NATURE_FLUXID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRNT_NATURES_TRAITEMENTS",
                    column: x => x.TRNT_NATURE_TRAITEMENTID,
                    principalTable: "TRNT_NATURES_TRAITEMENTS",
                    principalColumn: "TRNT_NATURE_TRAITEMENTID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRR_RESULTATS",
                    column: x => x.TRR_RESULTATID,
                    principalTable: "TRR_RESULTATS",
                    principalColumn: "TRR_RESULTATID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRTT_TECHNOS_TRAITEMENTS",
                    column: x => x.TRTT_TECHNO_TRAITEMENTID,
                    principalTable: "TRTT_TECHNOS_TRAITEMENTS",
                    principalColumn: "TRTT_TECHNO_TRAITEMENTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TRAP_ATTENDUS_PLANIFS",
            columns: table => new
            {
                TRAP_ATTENDU_PLANIFID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRA_ATTENDUID = table.Column<int>(type: "INTEGER", nullable: true),
                TRP_PLANIFID = table.Column<int>(type: "INTEGER", nullable: true),
                TRAP_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TRAP_DATE_MODIF = table.Column<DateTime>(type: "TEXT", nullable: false),
                TTU_MODIFICATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRAP_ATTENDUS_PLANIFS", x => x.TRAP_ATTENDU_PLANIFID);
                table.ForeignKey(
                    name: "FK_TRA_ATTENDUS",
                    column: x => x.TRA_ATTENDUID,
                    principalTable: "TRA_ATTENDUS",
                    principalColumn: "TRA_ATTENDUID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRP_PLANIFS",
                    column: x => x.TRP_PLANIFID,
                    principalTable: "TRP_PLANIFS",
                    principalColumn: "TRP_PLANIFID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRAPL_APPLICATIONID",
            table: "TRA_ATTENDUS",
            column: "TRAPL_APPLICATIONID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRC_CONTRATID",
            table: "TRA_ATTENDUS",
            column: "TRC_CONTRATID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRC_CRITICITEID",
            table: "TRA_ATTENDUS",
            column: "TRC_CRITICITEID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRNF_NATURE_DESTINATIONID",
            table: "TRA_ATTENDUS",
            column: "TRNF_NATURE_DESTINATIONID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRNF_NATURE_ORIGINEID",
            table: "TRA_ATTENDUS",
            column: "TRNF_NATURE_ORIGINEID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRNT_NATURE_TRAITEMENTID",
            table: "TRA_ATTENDUS",
            column: "TRNT_NATURE_TRAITEMENTID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRR_RESULTATID",
            table: "TRA_ATTENDUS",
            column: "TRR_RESULTATID");

        migrationBuilder.CreateIndex(
            name: "IX_TRA_ATTENDUS_TRTT_TECHNO_TRAITEMENTID",
            table: "TRA_ATTENDUS",
            column: "TRTT_TECHNO_TRAITEMENTID");

        migrationBuilder.CreateIndex(
            name: "UK_TRA_ATTENDUS",
            table: "TRA_ATTENDUS",
            columns: new[] { "TRA_CODE", "TRA_DEBUT_VALIDITE", "TRA_FIN_VALIDITE" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TRAP_ATTENDUS_PLANIFS_TRP_PLANIFID",
            table: "TRAP_ATTENDUS_PLANIFS",
            column: "TRP_PLANIFID");

        migrationBuilder.CreateIndex(
            name: "UK_TRAP_ATTENDUS_PLANIFS",
            table: "TRAP_ATTENDUS_PLANIFS",
            columns: new[] { "TRA_ATTENDUID", "TRP_PLANIFID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TRC_CONTRAT_CODE",
            table: "TRC_CONTRATS",
            column: "TRC_CONTRAT_CODE",
            unique: true);


        #region Insert Default values

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRC_CRITICITES
                    (TRC_LIB, TRC_DESCRIPTION)
                VALUES
                    ('Non critique',    'Pas d avertissement des administrateurs.'),
                    ('Critique',        'Avertissement des administrateurs.'),
                    ('Très critique',   'Avertissement prioritaire des administrateurs.');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRNF_NATURES_FLUX
                    (TRNF_LIB, TRNF_DESCRIPTION)
                VALUES
                    ('FICHIER', 'Fichiers'),
                    ('BDD', 'Base de données'),
                    ('API', 'Web API / OData'),
                    ('SHAREPOINT', 'Répertoire Sharepoint');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRNT_NATURES_TRAITEMENTS
                    (TRNT_LIB, TRNT_DESCRIPTION)
                VALUES
                    ('Acquisition', 'Traitement d Acquisition'),
                    ('Transformation', 'Traitement de transformation'),
                    ('Transfert', 'Traitement de transfert'),
                    ('Contrôle', 'Traitement de contrôle'),
                    ('Archivage', 'Traitement d archivage');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRR_RESULTATS
                    (TRR_CODE, TRR_DESCRIPTION)
                VALUES
                    ('OK', 'Le traitement a réussi.'),
                    ('KO', 'Le traitement a échoué.'),
                    ('EC', 'Le traitement est en cours.'),
                    ('NA', 'Le traitement n a pas de résultat attendu.');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRTT_TECHNOS_TRAITEMENTS
                    (TRTT_LIB, TRTT_DESCRIPTION)
                VALUES
                    ('BATCH', 'Fichier .BAT'),
                    ('EXE', 'Fichier Exécutable'),
                    ('ETL', 'ETL'),
                    ('Alteryx', 'Outil ETL'),
                    ('WASM', 'Web Assembly');
            ");

        #endregion
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TRAP_ATTENDUS_PLANIFS");

        migrationBuilder.DropTable(
            name: "TTL_LOGS");

        migrationBuilder.DropTable(
            name: "TRA_ATTENDUS");

        migrationBuilder.DropTable(
            name: "TRP_PLANIFS");

        migrationBuilder.DropTable(
            name: "TRAPL_APPLICATIONS");

        migrationBuilder.DropTable(
            name: "TRC_CONTRATS");

        migrationBuilder.DropTable(
            name: "TRC_CRITICITES");

        migrationBuilder.DropTable(
            name: "TRNF_NATURES_FLUX");

        migrationBuilder.DropTable(
            name: "TRNT_NATURES_TRAITEMENTS");

        migrationBuilder.DropTable(
            name: "TRR_RESULTATS");

        migrationBuilder.DropTable(
            name: "TRTT_TECHNOS_TRAITEMENTS");
    }
}
