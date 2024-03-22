using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_52 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TRST_STATUTID_OLD",
                table: "TS_SCENARIOS",
                type: "TEXT",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TRST_STATUTID_OLD",
                table: "TEM_ETAT_MASTERS",
                type: "TEXT",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TRST_STATUTID_OLD",
                table: "TE_ETATS",
                type: "TEXT",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TRST_STATUTID_OLD",
                table: "TS_SCENARIOS");

            migrationBuilder.DropColumn(
                name: "TRST_STATUTID_OLD",
                table: "TEM_ETAT_MASTERS");

            migrationBuilder.DropColumn(
                name: "TRST_STATUTID_OLD",
                table: "TE_ETATS");
        }
    }
}
