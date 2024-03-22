using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbEtqRev21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "etq_is_public_access",
                table: "TETQ_ETIQUETTES",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TM_AET_Authorization_Etq",
                columns: table => new
                {
                    aet_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    aet_etiquette_id = table.Column<int>(type: "int", nullable: false),
                    aet_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    aet_status_id = table.Column<string>(type: "TEXT", nullable: false),
                    aet_initializing_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    aet_initializing_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    aet_comments = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    aet_update_by = table.Column<Guid>(type: "TEXT", nullable: false),
                    aet_update_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_AET_Authorization_Etq", x => x.aet_id);
                    table.ForeignKey(
                        name: "FK_TM_AET_Authorization_Etq_TETQ_ETIQUETTE",
                        column: x => x.aet_etiquette_id,
                        principalTable: "TETQ_ETIQUETTES",
                        principalColumn: "TETQ_ETIQUETTEID");
                });

            migrationBuilder.CreateIndex(
                name: "IDX_ETQ_TM_AET_Authorization",
                table: "TM_AET_Authorization_Etq",
                columns: new[] { "aet_etiquette_id", "aet_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM_AET_Authorization_Etq");

            migrationBuilder.DropColumn(
                name: "etq_is_public_access",
                table: "TETQ_ETIQUETTES");
        }
    }
}
