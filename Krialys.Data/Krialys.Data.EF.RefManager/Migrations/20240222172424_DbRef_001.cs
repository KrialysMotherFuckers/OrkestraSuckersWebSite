using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class DbRef_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rfs_error_message",
                table: "TM_RFS_ReferentialSettings");

            migrationBuilder.RenameColumn(
                name: "rfs_request_id",
                table: "TM_RFS_ReferentialSettings",
                newName: "rfs_scenario_id");

            migrationBuilder.AlterColumn<string>(
                name: "rfh_label_code",
                table: "TX_RFH_ReferentialHistorical",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rfh_error_message",
                table: "TX_RFH_ReferentialHistorical",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rfh_label_code_generated",
                table: "TX_RFH_ReferentialHistorical",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rfh_request_id",
                table: "TX_RFH_ReferentialHistorical",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rfh_error_message",
                table: "TX_RFH_ReferentialHistorical");

            migrationBuilder.DropColumn(
                name: "rfh_label_code_generated",
                table: "TX_RFH_ReferentialHistorical");

            migrationBuilder.DropColumn(
                name: "rfh_request_id",
                table: "TX_RFH_ReferentialHistorical");

            migrationBuilder.RenameColumn(
                name: "rfs_scenario_id",
                table: "TM_RFS_ReferentialSettings",
                newName: "rfs_request_id");

            migrationBuilder.AlterColumn<string>(
                name: "rfh_label_code",
                table: "TX_RFH_ReferentialHistorical",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rfs_error_message",
                table: "TM_RFS_ReferentialSettings",
                type: "TEXT",
                nullable: true);
        }
    }
}
