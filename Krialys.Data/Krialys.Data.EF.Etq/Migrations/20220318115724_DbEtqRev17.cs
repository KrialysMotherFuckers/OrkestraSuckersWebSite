using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev17 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TACT_ACTIONS_TACT_ACTIONID",
            table: "TRGLRV_REGLES_VALEURS");

        migrationBuilder.DropForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TRGL_REGLES_TRGL_REGLEID",
            table: "TRGLRV_REGLES_VALEURS");

        migrationBuilder.AddForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TACT_ACTION",
            table: "TRGLRV_REGLES_VALEURS",
            column: "TACT_ACTIONID",
            principalTable: "TACT_ACTIONS",
            principalColumn: "TACT_ACTIONID");

        migrationBuilder.AddForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TRGL_REGLE",
            table: "TRGLRV_REGLES_VALEURS",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TACT_ACTION",
            table: "TRGLRV_REGLES_VALEURS");

        migrationBuilder.DropForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TRGL_REGLE",
            table: "TRGLRV_REGLES_VALEURS");

        migrationBuilder.AddForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TACT_ACTIONS_TACT_ACTIONID",
            table: "TRGLRV_REGLES_VALEURS",
            column: "TACT_ACTIONID",
            principalTable: "TACT_ACTIONS",
            principalColumn: "TACT_ACTIONID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TRGLRV_REGLES_VALEURS_TRGL_REGLES_TRGL_REGLEID",
            table: "TRGLRV_REGLES_VALEURS",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID",
            onDelete: ReferentialAction.Cascade);
    }
}
