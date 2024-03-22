using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev10 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.RenameColumn(
            name: "TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            newName: "TRGL_REGLEID");

        migrationBuilder.RenameColumn(
            name: "TOBJR_OBJET_REGLEID",
            table: "TETQR_ETQ_REGLES",
            newName: "TETQR_ETQ_REGLEID");

        migrationBuilder.RenameIndex(
            name: "IX_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            newName: "IX_TETQR_ETQ_REGLES_TRGL_REGLEID");

        migrationBuilder.AddColumn<string>(
            name: "TETQR_VALEUR",
            table: "TETQR_ETQ_REGLES",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGL_REGLES_TRGL_REGLEID",
            table: "TETQR_ETQ_REGLES",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGL_REGLES_TRGL_REGLEID",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropColumn(
            name: "TETQR_VALEUR",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.RenameColumn(
            name: "TRGL_REGLEID",
            table: "TETQR_ETQ_REGLES",
            newName: "TRGLRV_REGLES_VALEURID");

        migrationBuilder.RenameColumn(
            name: "TETQR_ETQ_REGLEID",
            table: "TETQR_ETQ_REGLES",
            newName: "TOBJR_OBJET_REGLEID");

        migrationBuilder.RenameIndex(
            name: "IX_TETQR_ETQ_REGLES_TRGL_REGLEID",
            table: "TETQR_ETQ_REGLES",
            newName: "IX_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURID");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR",
            table: "TETQR_ETQ_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID",
            onDelete: ReferentialAction.Restrict);
    }
}
