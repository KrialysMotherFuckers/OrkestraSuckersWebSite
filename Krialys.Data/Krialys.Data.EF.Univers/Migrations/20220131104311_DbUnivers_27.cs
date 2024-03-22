using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_27 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TIMEZONEINFOID",
            table: "TPF_PLANIFS",
            type: "TEXT",
            maxLength: 32,
            nullable: true);
        migrationBuilder.Sql("update TPF_PLANIFS set TIMEZONEINFOID='Romance Standard Time';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TIMEZONEINFOID",
            table: "TPF_PLANIFS");
    }
}
