using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_19 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_TBS_BATCH_SCENARIOS$TBS_BATCH_SCENARIOS",
            table: "TBS_BATCH_SCENARIOS",
            column: "TEB_ETAT_BATCHID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TBS_BATCH_SCENARIOS$TBS_BATCH_SCENARIOS",
            table: "TBS_BATCH_SCENARIOS");
    }
}
