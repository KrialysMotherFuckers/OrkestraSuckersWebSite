using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class DbRef_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rfh_treatment_id",
                table: "TX_RFH_ReferentialHistorical",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
