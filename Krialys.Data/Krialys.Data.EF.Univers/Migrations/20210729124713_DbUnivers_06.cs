using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_06 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TRCCL_VALUE_LABEL",
            table: "TRCCL_CATALOG_CLAIMS",
            type: "TEXT",
            maxLength: 32,
            nullable: true);
        migrationBuilder.Sql("update TRCCL_CATALOG_CLAIMS set TRCCL_VALUE_LABEL=TRCCL_VALUE;");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TRCCL_VALUE_LABEL",
            table: "TRCCL_CATALOG_CLAIMS");
    }
}
