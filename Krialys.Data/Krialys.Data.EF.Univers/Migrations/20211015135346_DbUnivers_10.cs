using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_10 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TEB_NOM_TECHNIQUE",
            table: "TEB_ETAT_BATCHS");

        migrationBuilder.AddColumn<string>(
            name: "TE_GENERE_CUBE",
            table: "TE_ETATS",
            type: "TEXT",
            maxLength: 1,
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TE_GENERE_CUBE",
            table: "TE_ETATS");

        migrationBuilder.AddColumn<string>(
            name: "TEB_NOM_TECHNIQUE",
            table: "TEB_ETAT_BATCHS",
            type: "TEXT",
            maxLength: 255,
            nullable: true);
    }
}
