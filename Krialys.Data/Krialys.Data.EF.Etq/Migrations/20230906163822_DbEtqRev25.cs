using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbEtqRev25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "rms_referencial_managed_by_labelling",
                table: "ETQ_TR_RMS_Ref_Manager_Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "rms_table_data_backup",
                table: "ETQ_TR_RMS_Ref_Manager_Settings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rms_table_metadata_backup",
                table: "ETQ_TR_RMS_Ref_Manager_Settings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rms_referencial_managed_by_labelling",
                table: "ETQ_TR_RMS_Ref_Manager_Settings");

            migrationBuilder.DropColumn(
                name: "rms_table_data_backup",
                table: "ETQ_TR_RMS_Ref_Manager_Settings");

            migrationBuilder.DropColumn(
                name: "rms_table_metadata_backup",
                table: "ETQ_TR_RMS_Ref_Manager_Settings");
        }
    }
}
