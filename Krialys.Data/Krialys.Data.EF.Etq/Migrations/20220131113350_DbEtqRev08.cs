using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev08 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TOBJR_VALEUR",
            table: "TOBJR_OBJET_REGLES",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldMaxLength: 50);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "TOBJR_VALEUR",
            table: "TOBJR_OBJET_REGLES",
            type: "INTEGER",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50);
    }
}
