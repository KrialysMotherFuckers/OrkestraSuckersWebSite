using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev04 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TPRCP_PRM_DYN",
            table: "TPRCP_PRC_PERIMETRES",
            type: "TEXT",
            maxLength: 1,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql("update TPRCP_PRC_PERIMETRES set TPRCP_PRM_DYN ='N';");

        migrationBuilder.AlterColumn<string>(
            name: "TRU_ACTEURID",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "TEXT",
            maxLength: 36,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 36);

        migrationBuilder.AddColumn<string>(
            name: "TETQ_PRM_VAL",
            table: "TETQ_ETIQUETTES",
            type: "TEXT",
            maxLength: 32,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TTR_TYPE_RESSOURCES",
            columns: table => new
            {
                TTR_TYPE_RESSOURCEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TTR_TYPE_ENTREE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TTR_TYPE_ENTREE_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TTR_TYPE_ENTREE_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTR_TYPE_RESSOURCES", x => x.TTR_TYPE_RESSOURCEID);
            });

        migrationBuilder.CreateTable(
            name: "SR_SUIVI_RESSOURCES",
            columns: table => new
            {
                TSR_SUIVI_RESSOURCEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TETQ_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false),
                TSR_ENTREE = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                TTR_TYPE_RESSOURCEID = table.Column<int>(type: "INTEGER", nullable: false),
                TSR_VALEUR_ENTREE = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SR_SUIVI_RESSOURCES", x => x.TSR_SUIVI_RESSOURCEID);
                table.ForeignKey(
                    name: "FK_SR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
                    column: x => x.TETQ_ETIQUETTEID,
                    principalTable: "TETQ_ETIQUETTES",
                    principalColumn: "TETQ_ETIQUETTEID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
                    column: x => x.TTR_TYPE_RESSOURCEID,
                    principalTable: "TTR_TYPE_RESSOURCES",
                    principalColumn: "TTR_TYPE_RESSOURCEID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "SR_SUIVI_RESSOURCES",
            column: "TTR_TYPE_RESSOURCEID");

        migrationBuilder.CreateIndex(
            name: "UK_TSR_SUIVI_RESSOURCES",
            table: "SR_SUIVI_RESSOURCES",
            columns: new[] { "TETQ_ETIQUETTEID", "TSR_ENTREE" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TTR_TYPE_RESSOURCES",
            table: "TTR_TYPE_RESSOURCES",
            column: "TTR_TYPE_ENTREE",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SR_SUIVI_RESSOURCES");

        migrationBuilder.DropTable(
            name: "TTR_TYPE_RESSOURCES");

        migrationBuilder.DropColumn(
            name: "TPRCP_PRM_DYN",
            table: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.DropColumn(
            name: "TETQ_PRM_VAL",
            table: "TETQ_ETIQUETTES");

        migrationBuilder.AlterColumn<string>(
            name: "TRU_ACTEURID",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "TEXT",
            maxLength: 36,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 36,
            oldNullable: true);
    }
}
