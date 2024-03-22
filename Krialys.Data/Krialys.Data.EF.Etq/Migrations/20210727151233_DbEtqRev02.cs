using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbEtqRev02 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TETQ_INCREMENT",
            table: "TETQ_ETIQUETTES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "TETQ_PATTERN",
            table: "TETQ_ETIQUETTES",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TETQ_INCREMENT",
            table: "TETQ_ETIQUETTES");

        migrationBuilder.DropColumn(
            name: "TETQ_PATTERN",
            table: "TETQ_ETIQUETTES");
    }
}
