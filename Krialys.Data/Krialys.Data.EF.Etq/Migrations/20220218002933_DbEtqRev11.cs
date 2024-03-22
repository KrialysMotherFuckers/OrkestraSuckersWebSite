using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev11 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TOBJR_VALEUR",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropColumn(
            name: "TETQR_VALEUR",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.AddColumn<int>(
            name: "TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGLRV_REGLES_VALEURID");

        migrationBuilder.CreateIndex(
            name: "IX_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            column: "TRGLRV_REGLES_VALEURID");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropIndex(
            name: "IX_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropIndex(
            name: "IX_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropColumn(
            name: "TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropColumn(
            name: "TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.AddColumn<string>(
            name: "TOBJR_VALEUR",
            table: "TOBJR_OBJET_REGLES",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "TETQR_VALEUR",
            table: "TETQR_ETQ_REGLES",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "");
    }
}
