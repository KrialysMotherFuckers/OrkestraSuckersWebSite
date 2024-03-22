using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev03 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "TOBN_OBJ_NATUREID",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<int>(
            name: "TOBF_OBJ_FORMATID",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "TOBN_OBJ_NATUREID",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "TOBF_OBJ_FORMATID",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);
    }
}
