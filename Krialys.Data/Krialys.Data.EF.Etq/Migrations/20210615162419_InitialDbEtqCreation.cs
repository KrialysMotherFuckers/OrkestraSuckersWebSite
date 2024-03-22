using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class InitialDbEtqCreation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TEQC_ETQ_CODIFS",
            columns: table => new
            {
                TEQC_ETQ_CODIFID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TEQC_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TEQC_CODE_PRC_ORDRE = table.Column<int>(type: "INTEGER", nullable: true),
                TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE = table.Column<int>(type: "INTEGER", nullable: true),
                TEQC_CODE_PRM_ORDRE = table.Column<int>(type: "INTEGER", nullable: true),
                TEQC_INCREMENT_ORDRE = table.Column<int>(type: "INTEGER", nullable: false),
                TEQC_INCREMENT_TAILLE = table.Column<int>(type: "INTEGER", nullable: false),
                TEQC_INCREMENT_VAL_INIT = table.Column<int>(type: "INTEGER", nullable: true),
                TEQC_SEPARATEUR = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TEQC_ETQ_CODIFS", x => x.TEQC_ETQ_CODIFID);
            });

        migrationBuilder.CreateTable(
            name: "TOBF_OBJ_FORMATS",
            columns: table => new
            {
                TOBF_OBJ_FORMATID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TOBF_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TOBF_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TOBF_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TOBF_OBJ_FORMATS", x => x.TOBF_OBJ_FORMATID);
            });

        migrationBuilder.CreateTable(
            name: "TOBN_OBJ_NATURES",
            columns: table => new
            {
                TOBN_OBJ_NATUREID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TOBN_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TOBN_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TOBN_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TOBN_OBJ_NATURES", x => x.TOBN_OBJ_NATUREID);
            });

        migrationBuilder.CreateTable(
            name: "TTE_TYPE_EVENEMENTS",
            columns: table => new
            {
                TTE_TYPE_EVENEMENTID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TTE_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TTE_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TTE_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTE_TYPE_EVENEMENTS", x => x.TTE_TYPE_EVENEMENTID);
            });

        migrationBuilder.CreateTable(
            name: "TTPR_TYPE_PROCESSUS",
            columns: table => new
            {
                TTPR_TYPE_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TTPR_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TTPR_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TTPR_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTPR_TYPE_PROCESSUS", x => x.TTPR_TYPE_PROCESSUSID);
            });

        migrationBuilder.CreateTable(
            name: "TPRS_PROCESSUS",
            columns: table => new
            {
                TPRS_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TPRS_CODE = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TPRS_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TPRS_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TTPR_TYPE_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: true),
                TPRS_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRU_ACTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPRS_PROCESSUS", x => x.TPRS_PROCESSUSID);
                table.ForeignKey(
                    name: "FK_TTPR_TYPE_PROCESSUS",
                    column: x => x.TTPR_TYPE_PROCESSUSID,
                    principalTable: "TTPR_TYPE_PROCESSUS",
                    principalColumn: "TTPR_TYPE_PROCESSUSID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TOBJE_OBJET_ETIQUETTES",
            columns: table => new
            {
                TOBJE_OBJET_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TOBJE_CODE = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                TOBJE_VERSION = table.Column<int>(type: "INTEGER", nullable: false),
                TOBJE_VERSION_ETQ_STATUT = table.Column<int>(type: "INTEGER", nullable: false),
                TPRS_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: false),
                TOBJE_CODE_ETIQUETTAGE = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                TOBJE_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TOBJE_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TOBF_OBJ_FORMATID = table.Column<int>(type: "INTEGER", nullable: false),
                TOBN_OBJ_NATUREID = table.Column<int>(type: "INTEGER", nullable: false),
                TEQC_ETQ_CODIFID = table.Column<int>(type: "INTEGER", nullable: false),
                TOBJE_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRU_ACTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TOBJE_OBJET_ETIQUETTES", x => x.TOBJE_OBJET_ETIQUETTEID);
                table.ForeignKey(
                    name: "FK_TEQC_ETQ_CODIFS",
                    column: x => x.TEQC_ETQ_CODIFID,
                    principalTable: "TEQC_ETQ_CODIFS",
                    principalColumn: "TEQC_ETQ_CODIFID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TOBF_OBJ_FORMATS",
                    column: x => x.TOBF_OBJ_FORMATID,
                    principalTable: "TOBF_OBJ_FORMATS",
                    principalColumn: "TOBF_OBJ_FORMATID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TOBN_OBJ_NATURES",
                    column: x => x.TOBN_OBJ_NATUREID,
                    principalTable: "TOBN_OBJ_NATURES",
                    principalColumn: "TOBN_OBJ_NATUREID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPRS_PROCESSUS",
                    column: x => x.TPRS_PROCESSUSID,
                    principalTable: "TPRS_PROCESSUS",
                    principalColumn: "TPRS_PROCESSUSID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPRCP_PRC_PERIMETRES",
            columns: table => new
            {
                TPRCP_PRC_PERIMETREID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TPRS_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: false),
                TPRCP_CODE = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                TPRCP_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TPRCP_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TPRCP_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRU_ACTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPRCP_PRC_PERIMETRES", x => x.TPRCP_PRC_PERIMETREID);
                table.ForeignKey(
                    name: "FK_TPRS_PROCESSUS_TPRCP_PRC_PERIMETRES",
                    column: x => x.TPRS_PROCESSUSID,
                    principalTable: "TPRS_PROCESSUS",
                    principalColumn: "TPRS_PROCESSUSID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TETQ_ETIQUETTES",
            columns: table => new
            {
                TETQ_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TETQ_CODE = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                TOBJE_OBJET_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false),
                TETQ_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TETQ_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TPRCP_PRC_PERIMETREID = table.Column<int>(type: "INTEGER", nullable: true),
                DEMANDEID = table.Column<int>(type: "INTEGER", nullable: true),
                TETQ_VERSION_ETQ = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TETQ_ETIQUETTES", x => x.TETQ_ETIQUETTEID);
                table.ForeignKey(
                    name: "FK_TOBJE_OBJET_ETIQUETTES",
                    column: x => x.TOBJE_OBJET_ETIQUETTEID,
                    principalTable: "TOBJE_OBJET_ETIQUETTES",
                    principalColumn: "TOBJE_OBJET_ETIQUETTEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPRCP_PRC_PERIMETRES",
                    column: x => x.TPRCP_PRC_PERIMETREID,
                    principalTable: "TPRCP_PRC_PERIMETRES",
                    principalColumn: "TPRCP_PRC_PERIMETREID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TSEQ_SUIVI_EVENEMENT_ETQS",
            columns: table => new
            {
                TSEQ_SUIVI_EVENEMENT_ETQID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TETQ_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false),
                TTE_TYPE_EVENEMENTID = table.Column<int>(type: "INTEGER", nullable: false),
                TSEQ_DATE_EVENEMENT = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRU_ACTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TSEQ_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TSEQ_SUIVI_EVENEMENT_ETQS", x => x.TSEQ_SUIVI_EVENEMENT_ETQID);
                table.ForeignKey(
                    name: "FK_TETQ_ETIQUETTES",
                    column: x => x.TETQ_ETIQUETTEID,
                    principalTable: "TETQ_ETIQUETTES",
                    principalColumn: "TETQ_ETIQUETTEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TTE_TYPE_EVENEMENTS",
                    column: x => x.TTE_TYPE_EVENEMENTID,
                    principalTable: "TTE_TYPE_EVENEMENTS",
                    principalColumn: "TTE_TYPE_EVENEMENTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "UK_TEQC_ETQ_CODIFS",
            table: "TEQC_ETQ_CODIFS",
            column: "TEQC_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TETQ_ETIQUETTES_TOBJE_OBJET_ETIQUETTEID",
            table: "TETQ_ETIQUETTES",
            column: "TOBJE_OBJET_ETIQUETTEID");

        migrationBuilder.CreateIndex(
            name: "IX_TETQ_ETIQUETTES_TPRCP_PRC_PERIMETREID",
            table: "TETQ_ETIQUETTES",
            column: "TPRCP_PRC_PERIMETREID");

        migrationBuilder.CreateIndex(
            name: "UK_TETQ_ETIQUETTES",
            table: "TETQ_ETIQUETTES",
            column: "TETQ_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TOBF_OBJ_FORMATS",
            table: "TOBF_OBJ_FORMATS",
            column: "TOBF_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TOBJE_OBJET_ETIQUETTES_TEQC_ETQ_CODIFID",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TEQC_ETQ_CODIFID");

        migrationBuilder.CreateIndex(
            name: "IX_TOBJE_OBJET_ETIQUETTES_TOBF_OBJ_FORMATID",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBF_OBJ_FORMATID");

        migrationBuilder.CreateIndex(
            name: "IX_TOBJE_OBJET_ETIQUETTES_TOBN_OBJ_NATUREID",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBN_OBJ_NATUREID");

        migrationBuilder.CreateIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBJE_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTESBIS",
            table: "TOBJE_OBJET_ETIQUETTES",
            columns: new[] { "TPRS_PROCESSUSID", "TOBJE_CODE_ETIQUETTAGE" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TOBN_OBJ_NATURES",
            table: "TOBN_OBJ_NATURES",
            column: "TOBN_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TPRCP_PRC_PERIMETRES_TPRS_PROCESSUSID",
            table: "TPRCP_PRC_PERIMETRES",
            column: "TPRS_PROCESSUSID");

        migrationBuilder.CreateIndex(
            name: "UK_TPRCP_PRC_PERIMETRES",
            table: "TPRCP_PRC_PERIMETRES",
            columns: new[] { "TPRCP_CODE", "TPRS_PROCESSUSID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TPRS_PROCESSUS_TTPR_TYPE_PROCESSUSID",
            table: "TPRS_PROCESSUS",
            column: "TTPR_TYPE_PROCESSUSID");

        migrationBuilder.CreateIndex(
            name: "UK_TPRS_PROCESSUS",
            table: "TPRS_PROCESSUS",
            column: "TPRS_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TSEQ_SUIVI_EVENEMENT_ETQS_TETQ_ETIQUETTEID",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            column: "TETQ_ETIQUETTEID");

        migrationBuilder.CreateIndex(
            name: "IX_TSEQ_SUIVI_EVENEMENT_ETQS_TTE_TYPE_EVENEMENTID",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            column: "TTE_TYPE_EVENEMENTID");

        migrationBuilder.CreateIndex(
            name: "UK_TTE_TYPE_EVENEMENTS",
            table: "TTE_TYPE_EVENEMENTS",
            column: "TTE_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TTPR_TYPE_PROCESSUS",
            table: "TTPR_TYPE_PROCESSUS",
            column: "TTPR_CODE",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TSEQ_SUIVI_EVENEMENT_ETQS");

        migrationBuilder.DropTable(
            name: "TETQ_ETIQUETTES");

        migrationBuilder.DropTable(
            name: "TTE_TYPE_EVENEMENTS");

        migrationBuilder.DropTable(
            name: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropTable(
            name: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.DropTable(
            name: "TEQC_ETQ_CODIFS");

        migrationBuilder.DropTable(
            name: "TOBF_OBJ_FORMATS");

        migrationBuilder.DropTable(
            name: "TOBN_OBJ_NATURES");

        migrationBuilder.DropTable(
            name: "TPRS_PROCESSUS");

        migrationBuilder.DropTable(
            name: "TTPR_TYPE_PROCESSUS");
    }
}
