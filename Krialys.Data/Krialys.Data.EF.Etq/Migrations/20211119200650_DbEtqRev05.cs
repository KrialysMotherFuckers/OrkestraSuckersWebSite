using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev05 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
            table: "SR_SUIVI_RESSOURCES");

        migrationBuilder.DropForeignKey(
            name: "FK_SR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "SR_SUIVI_RESSOURCES");

        migrationBuilder.DropPrimaryKey(
            name: "PK_SR_SUIVI_RESSOURCES",
            table: "SR_SUIVI_RESSOURCES");

        migrationBuilder.RenameTable(
            name: "SR_SUIVI_RESSOURCES",
            newName: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.RenameIndex(
            name: "IX_SR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "TSR_SUIVI_RESSOURCES",
            newName: "IX_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCEID");

        migrationBuilder.AddPrimaryKey(
            name: "PK_TSR_SUIVI_RESSOURCES",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TSR_SUIVI_RESSOURCEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TTR_TYPE_RESSOURCEID",
            principalTable: "TTR_TYPE_RESSOURCES",
            principalColumn: "TTR_TYPE_RESSOURCEID",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.DropForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.DropPrimaryKey(
            name: "PK_TSR_SUIVI_RESSOURCES",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.RenameTable(
            name: "TSR_SUIVI_RESSOURCES",
            newName: "SR_SUIVI_RESSOURCES");

        migrationBuilder.RenameIndex(
            name: "IX_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "SR_SUIVI_RESSOURCES",
            newName: "IX_SR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCEID");

        migrationBuilder.AddPrimaryKey(
            name: "PK_SR_SUIVI_RESSOURCES",
            table: "SR_SUIVI_RESSOURCES",
            column: "TSR_SUIVI_RESSOURCEID");

        migrationBuilder.AddForeignKey(
            name: "FK_SR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
            table: "SR_SUIVI_RESSOURCES",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_SR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "SR_SUIVI_RESSOURCES",
            column: "TTR_TYPE_RESSOURCEID",
            principalTable: "TTR_TYPE_RESSOURCES",
            principalColumn: "TTR_TYPE_RESSOURCEID",
            onDelete: ReferentialAction.Cascade);
    }
}
