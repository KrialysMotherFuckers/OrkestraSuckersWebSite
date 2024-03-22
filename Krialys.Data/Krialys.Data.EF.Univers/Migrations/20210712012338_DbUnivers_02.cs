using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_02 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TRCLI_CLIENTAPPLICATIONID",
            table: "TRUCL_USERS_CLAIMS",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "TRUCL_DESCRIPTION",
            table: "TRUCL_USERS_CLAIMS",
            type: "TEXT",
            maxLength: 255,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TRCLICL_DESCRIPTION",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            type: "TEXT",
            maxLength: 255,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TDP_DEMANDE_PROCESS",
            columns: table => new
            {
                TDP_DEMANDE_PROCESSID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TDP_NUM_ETAPE = table.Column<int>(type: "INTEGER", nullable: false),
                TDP_ETAPE = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                TDP_STATUT = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                TDP_EXTRA_INFO = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TDP_DEMANDE_PROCESS", x => x.TDP_DEMANDE_PROCESSID);
                table.ForeignKey(
                    name: "FK_TDP_DEMANDE_PROCESSS$TD_DEMANDES",
                    column: x => x.TD_DEMANDEID,
                    principalTable: "TD_DEMANDES",
                    principalColumn: "TD_DEMANDEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TDP_DEMANDE_PROCESS_TD_DEMANDEID",
            table: "TDP_DEMANDE_PROCESS",
            column: "TD_DEMANDEID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TDP_DEMANDE_PROCESS");

        migrationBuilder.DropColumn(
            name: "TRCLI_CLIENTAPPLICATIONID",
            table: "TRUCL_USERS_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRUCL_DESCRIPTION",
            table: "TRUCL_USERS_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRCLICL_DESCRIPTION",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS");
    }
}
