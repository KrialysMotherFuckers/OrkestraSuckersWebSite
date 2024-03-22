using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_15 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TRST_STATUTID",
            table: "TEB_ETAT_BATCHS",
            type: "TEXT",
            maxLength: 3,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql("update TEB_ETAT_BATCHS set TRST_STATUTID='A';");

        migrationBuilder.CreateIndex(
            name: "IX_TEB_ETAT_BATCHS_TRST_STATUTID",
            table: "TEB_ETAT_BATCHS",
            column: "TRST_STATUTID");

        migrationBuilder.AddForeignKey(
            name: "FK_TEB_ETAT_BATCHS_TRST_STATUTS_TRST_STATUTID",
            table: "TEB_ETAT_BATCHS",
            column: "TRST_STATUTID",
            principalTable: "TRST_STATUTS",
            principalColumn: "TRST_STATUTID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.Sql("UPDATE TEB_ETAT_BATCHS set TEB_DATE_CREATION = substr(TEB_DATE_CREATION, 1, 19);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TEB_ETAT_BATCHS_TRST_STATUTS_TRST_STATUTID",
            table: "TEB_ETAT_BATCHS");

        migrationBuilder.DropIndex(
            name: "IX_TEB_ETAT_BATCHS_TRST_STATUTID",
            table: "TEB_ETAT_BATCHS");

        migrationBuilder.DropColumn(
            name: "TRST_STATUTID",
            table: "TEB_ETAT_BATCHS");
    }
}
