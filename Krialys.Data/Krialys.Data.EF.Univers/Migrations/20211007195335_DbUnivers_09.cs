using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_09 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS_TC_CATEGORIEID",
            table: "TEM_ETAT_MASTERS",
            column: "TC_CATEGORIEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS",
            column: "TC_CATEGORIEID",
            principalTable: "TC_CATEGORIES",
            principalColumn: "TC_CATEGORIEID",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropIndex(
            name: "IX_TEM_ETAT_MASTERS_TC_CATEGORIEID",
            table: "TEM_ETAT_MASTERS");
    }
}
