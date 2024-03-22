using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_04 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TRUCL_DESCRIPTION",
            table: "TRUCL_USERS_CLAIMS",
            type: "TEXT",
            maxLength: 255,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 255,
            oldNullable: true);
        migrationBuilder.Sql("update TRUCL_USERS_CLAIMS set TRUCL_DESCRIPTION='sample' where TRUCL_DESCRIPTION is null;");

        migrationBuilder.AddColumn<string>(
            name: "TRUCL_STATUT",
            table: "TRUCL_USERS_CLAIMS",
            type: "TEXT",
            maxLength: 1,
            nullable: false,
            defaultValue: "");
        migrationBuilder.Sql("update TRUCL_USERS_CLAIMS set TRUCL_STATUT='A'");

        migrationBuilder.AddColumn<string>(
            name: "TRCLICL_STATUT",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            type: "TEXT",
            maxLength: 1,
            nullable: false,
            defaultValue: "");
        migrationBuilder.Sql("update TRCLICL_CLIENTAPPLICATIONS_CLAIMS set TRCLICL_STATUT='A';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TRUCL_STATUT",
            table: "TRUCL_USERS_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRCLICL_STATUT",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS");

        migrationBuilder.AlterColumn<string>(
            name: "TRUCL_DESCRIPTION",
            table: "TRUCL_USERS_CLAIMS",
            type: "TEXT",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 255);
    }
}
