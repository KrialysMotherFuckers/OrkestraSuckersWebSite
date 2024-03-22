using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev15 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES",
            table: "TETQ_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.DropTable(
            name: "TPRS_PROCESSUS");

        migrationBuilder.DropTable(
            name: "TTPR_TYPE_PROCESSUS");

        migrationBuilder.AddForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES",
            table: "TETQ_ETIQUETTES",
            column: "TPRCP_PRC_PERIMETREID",
            principalTable: "TPRCP_PRC_PERIMETRES",
            principalColumn: "TPRCP_PRC_PERIMETREID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TDOM_DOMAINEID",
            principalTable: "TDOM_DOMAINES",
            principalColumn: "TDOM_DOMAINEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES",
            table: "TPRCP_PRC_PERIMETRES",
            column: "TDOM_DOMAINEID",
            principalTable: "TDOM_DOMAINES",
            principalColumn: "TDOM_DOMAINEID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES",
            table: "TETQ_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES",
            table: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.CreateTable(
            name: "TTPR_TYPE_PROCESSUS",
            columns: table => new
            {
                TTPR_TYPE_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TTPR_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TTPR_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TTPR_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
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
                TPRS_DATE_CREATION = table.Column<DateTime>(type: "TEXT", nullable: false),
                TPRS_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TPRS_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TRU_ACTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TTPR_TYPE_PROCESSUSID = table.Column<int>(type: "INTEGER", nullable: true)
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
            name: "UK_TTPR_TYPE_PROCESSUS",
            table: "TTPR_TYPE_PROCESSUS",
            column: "TTPR_CODE",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES",
            table: "TETQ_ETIQUETTES",
            column: "TPRCP_PRC_PERIMETREID",
            principalTable: "TPRCP_PRC_PERIMETRES",
            principalColumn: "TPRCP_PRC_PERIMETREID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TDOM_DOMAINEID",
            principalTable: "TDOM_DOMAINES",
            principalColumn: "TDOM_DOMAINEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TPRCP_PRC_PERIMETRES",
            column: "TDOM_DOMAINEID",
            principalTable: "TDOM_DOMAINES",
            principalColumn: "TDOM_DOMAINEID",
            onDelete: ReferentialAction.Cascade);
    }
}
