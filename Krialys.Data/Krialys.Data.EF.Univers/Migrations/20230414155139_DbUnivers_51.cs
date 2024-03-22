using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers_51 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TCMD_PH_DELAI_RECYCLAGE",
            table: "TCMD_PH_PHASES",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=30 where TCMD_PH_CODE='TE';");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=60 where TCMD_PH_CODE='LI';");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=30 where TCMD_PH_CODE='RJ';");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=90 where TCMD_PH_CODE='GL';");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=30 where TCMD_PH_CODE='AN';");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=120 where TCMD_PH_CODE='AA';");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=120 where TCMD_PH_CODE='EC';");

        migrationBuilder.Sql(@"update TCMD_PH_PHASES set TCMD_PH_DELAI_RECYCLAGE=30 where TCMD_PH_CODE='BR';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TCMD_PH_DELAI_RECYCLAGE",
            table: "TCMD_PH_PHASES");
    }
}
