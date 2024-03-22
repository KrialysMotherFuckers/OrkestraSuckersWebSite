using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_01 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOID",
            table: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS");

        migrationBuilder.RenameColumn(
            name: "TRCLI_APPLICATIONID",
            table: "TRCLI_CLIENTAPPLICATIONS",
            newName: "TRCLI_CLIENTAPPLICATIONID");

        migrationBuilder.RenameColumn(
            name: "TRCL_CLAIM_DESCRIPION",
            table: "TRCL_CLAIMS",
            newName: "TRCL_CLAIM_DESCRIPTION");

        migrationBuilder.RenameColumn(
            name: "TRCLI_STATUS",
            table: "TRCL_CLAIMS",
            newName: "TRCL_STATUS");

        migrationBuilder.AddForeignKey(
            name: "FK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIO",
            table: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",
            column: "TRAS_AUTH_SCENARIOID",
            principalTable: "TRAS_AUTH_SCENARIOS",
            principalColumn: "TRAS_AUTH_SCENARIOID",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIO",
            table: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS");

        migrationBuilder.RenameColumn(
            name: "TRCLI_CLIENTAPPLICATIONID",
            table: "TRCLI_CLIENTAPPLICATIONS",
            newName: "TRCLI_APPLICATIONID");

        migrationBuilder.RenameColumn(
            name: "TRCL_STATUS",
            table: "TRCL_CLAIMS",
            newName: "TRCLI_STATUS");

        migrationBuilder.RenameColumn(
            name: "TRCL_CLAIM_DESCRIPTION",
            table: "TRCL_CLAIMS",
            newName: "TRCL_CLAIM_DESCRIPION");

        migrationBuilder.AddForeignKey(
            name: "FK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOID",
            table: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",
            column: "TRAS_AUTH_SCENARIOID",
            principalTable: "TRAS_AUTH_SCENARIOS",
            principalColumn: "TRAS_AUTH_SCENARIOID",
            onDelete: ReferentialAction.Cascade);
    }
}
