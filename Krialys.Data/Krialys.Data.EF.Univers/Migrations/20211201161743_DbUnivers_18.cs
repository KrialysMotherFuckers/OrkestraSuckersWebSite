using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_18 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_TEB_ETAT_BATCHS$TE_ETAT_BATCHS",
            table: "TEB_ETAT_BATCHS",
            column: "TEB_ETAT_BATCHID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TEB_ETAT_BATCHS$TE_ETAT_BATCHS",
            table: "TEB_ETAT_BATCHS");
    }
}
