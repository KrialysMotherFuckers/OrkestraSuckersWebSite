using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_12 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_TEL_ETAT_LOGICIELS",
            table: "TEL_ETAT_LOGICIELS");

        migrationBuilder.AlterColumn<int>(
            name: "TEL_ETAT_LOGICIELID",
            table: "TEL_ETAT_LOGICIELS",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_TEL_ETAT_LOGICIELS",
            table: "TEL_ETAT_LOGICIELS",
            column: "TEL_ETAT_LOGICIELID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_TEL_ETAT_LOGICIELS",
            table: "TEL_ETAT_LOGICIELS");

        migrationBuilder.AlterColumn<int>(
            name: "TEL_ETAT_LOGICIELID",
            table: "TEL_ETAT_LOGICIELS",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_TEL_ETAT_LOGICIELS",
            table: "TEL_ETAT_LOGICIELS",
            columns: new[] { "TE_ETATID", "TL_LOGICIELID" });
    }
}
