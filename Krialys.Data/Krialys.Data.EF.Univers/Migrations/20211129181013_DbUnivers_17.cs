using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_17 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TRS_FICHIER_OBLIGATOIRE",
            table: "TRS_RESSOURCE_SCENARIOS",
            type: "TEXT",
            maxLength: 1,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 1,
            oldNullable: true,
            oldDefaultValueSql: "('N')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TRS_FICHIER_OBLIGATOIRE",
            table: "TRS_RESSOURCE_SCENARIOS",
            type: "TEXT",
            maxLength: 1,
            nullable: true,
            defaultValueSql: "('N')",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 1);
    }
}
