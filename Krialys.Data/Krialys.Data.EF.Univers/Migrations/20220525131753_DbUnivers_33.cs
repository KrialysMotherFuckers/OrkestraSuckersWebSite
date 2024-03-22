using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_33 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TRU_FULLNAME",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: true,
            computedColumnSql: "[TRU_NAME] || ' ' || [TRU_FIRST_NAME]");

        migrationBuilder.AddColumn<string>(
            name: "TE_VERSION",
            table: "TE_ETATS",
            type: "TEXT",
            nullable: true,
            computedColumnSql: "[TE_INDICE_REVISION_L1] || '.' || [TE_INDICE_REVISION_L2] || '.' || [TE_INDICE_REVISION_L3]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TRU_FULLNAME",
            table: "TRU_USERS");

        migrationBuilder.DropColumn(
            name: "TE_VERSION",
            table: "TE_ETATS");
    }
}
