using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers_50 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"PRAGMA legacy_alter_table=1;");

        migrationBuilder.AlterColumn<string>(
            name: "TLE_EDITEUR",
            table: "TLE_LOGICIEL_EDITEURS",
            type: "TEXT",
            maxLength: 30,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TL_NOM_LOGICIEL",
            table: "TL_LOGICIELS",
            type: "TEXT",
            maxLength: 30,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TF_DESCR",
            table: "TF_FERMES",
            type: "TEXT",
            maxLength: 255,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 255,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TLE_EDITEUR",
            table: "TLE_LOGICIEL_EDITEURS",
            type: "TEXT",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 30);

        migrationBuilder.AlterColumn<string>(
            name: "TL_NOM_LOGICIEL",
            table: "TL_LOGICIELS",
            type: "TEXT",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 30);

        migrationBuilder.AlterColumn<string>(
            name: "TF_DESCR",
            table: "TF_FERMES",
            type: "TEXT",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 255);
    }
}
