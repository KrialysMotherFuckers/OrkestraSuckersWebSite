using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_03 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONID",
            table: "TRUCL_USERS_CLAIMS",
            column: "TRCLI_CLIENTAPPLICATIONID");

        migrationBuilder.AddForeignKey(
            name: "FK_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONS_TRCLI_CLIENTAPPLICATIONID",
            table: "TRUCL_USERS_CLAIMS",
            column: "TRCLI_CLIENTAPPLICATIONID",
            principalTable: "TRCLI_CLIENTAPPLICATIONS",
            principalColumn: "TRCLI_CLIENTAPPLICATIONID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.Sql("update TRUCL_USERS_CLAIMS set TRCLI_CLIENTAPPLICATIONID=1;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONS_TRCLI_CLIENTAPPLICATIONID",
            table: "TRUCL_USERS_CLAIMS");

        migrationBuilder.DropIndex(
            name: "IX_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONID",
            table: "TRUCL_USERS_CLAIMS");
    }
}
