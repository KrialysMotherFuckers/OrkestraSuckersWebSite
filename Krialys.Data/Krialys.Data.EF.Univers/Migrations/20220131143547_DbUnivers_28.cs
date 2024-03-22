using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_28 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TPF_TIMEZONE_INFOID",
            table: "TPF_PLANIFS",
            type: "TEXT",
            maxLength: 255,
            nullable: false,
            defaultValue: "");
        migrationBuilder.Sql("update TPF_PLANIFS set TPF_TIMEZONE_INFOID='Romance Standard Time';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TPF_TIMEZONE_INFOID",
            table: "TPF_PLANIFS");
    }
}
