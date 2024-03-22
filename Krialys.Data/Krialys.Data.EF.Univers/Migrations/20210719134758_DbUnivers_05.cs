using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_05 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "TRUCL_STATUT",
            table: "TRUCL_USERS_CLAIMS",
            newName: "TRUCL_STATUS");

        migrationBuilder.RenameColumn(
            name: "TRCLICL_STATUT",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            newName: "TRCLICL_STATUS");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "TRUCL_STATUS",
            table: "TRUCL_USERS_CLAIMS",
            newName: "TRUCL_STATUT");

        migrationBuilder.RenameColumn(
            name: "TRCLICL_STATUS",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            newName: "TRCLICL_STATUT");
    }
}
