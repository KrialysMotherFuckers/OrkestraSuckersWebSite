using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev13 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TRU_ACTEURID",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            type: "TEXT",
            maxLength: 36,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 36);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TRU_ACTEURID",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            type: "TEXT",
            maxLength: 36,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 36,
            oldNullable: true);
    }
}
