using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbEtqRev22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TM_AET_Authorization_Etq_TETQ_ETIQUETTE",
                table: "TM_AET_Authorization_Etq");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TM_AET_Authorization_Etq",
                table: "TM_AET_Authorization_Etq");

            migrationBuilder.RenameTable(
                name: "TM_AET_Authorization_Etq",
                newName: "ETQ_TM_AET_Authorization");

            migrationBuilder.AlterColumn<string>(
                name: "aet_update_by",
                table: "ETQ_TM_AET_Authorization",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ETQ_TM_AET_Authorization",
                table: "ETQ_TM_AET_Authorization",
                column: "aet_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ETQ_TM_AET_Authorization_TETQ_ETIQUETTE",
                table: "ETQ_TM_AET_Authorization",
                column: "aet_etiquette_id",
                principalTable: "TETQ_ETIQUETTES",
                principalColumn: "TETQ_ETIQUETTEID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ETQ_TM_AET_Authorization_TETQ_ETIQUETTE",
                table: "ETQ_TM_AET_Authorization");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ETQ_TM_AET_Authorization",
                table: "ETQ_TM_AET_Authorization");

            migrationBuilder.RenameTable(
                name: "ETQ_TM_AET_Authorization",
                newName: "TM_AET_Authorization_Etq");

            migrationBuilder.AlterColumn<Guid>(
                name: "aet_update_by",
                table: "TM_AET_Authorization_Etq",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TM_AET_Authorization_Etq",
                table: "TM_AET_Authorization_Etq",
                column: "aet_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TM_AET_Authorization_Etq_TETQ_ETIQUETTE",
                table: "TM_AET_Authorization_Etq",
                column: "aet_etiquette_id",
                principalTable: "TETQ_ETIQUETTES",
                principalColumn: "TETQ_ETIQUETTEID");
        }
    }
}
