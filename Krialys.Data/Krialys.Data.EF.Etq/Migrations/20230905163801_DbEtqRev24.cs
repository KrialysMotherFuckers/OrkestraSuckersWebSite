using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbEtqRev24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ETQ_TR_RMC_Ref_Manager_Connections",
                columns: table => new
                {
                    rcs_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rms_server = table.Column<string>(type: "TEXT", nullable: false),
                    rms_database_type = table.Column<int>(type: "int", nullable: false),
                    rms_database_name = table.Column<string>(type: "TEXT", nullable: false),
                    rms_database_login = table.Column<string>(type: "TEXT", nullable: true),
                    rms_database_password = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ETQ_TR_RMC_Ref_Manager_Connections", x => x.rcs_id);
                });

            migrationBuilder.CreateTable(
                name: "ETQ_TR_RMS_Ref_Manager_Settings",
                columns: table => new
                {
                    rms_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rms_table_name = table.Column<string>(type: "TEXT", nullable: false),
                    rcs_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rms_table_sql_request = table.Column<string>(type: "TEXT", nullable: false),
                    rms_table_data = table.Column<string>(type: "TEXT", nullable: true),
                    rms_table_metadata = table.Column<string>(type: "TEXT", nullable: true),
                    rms_description = table.Column<string>(type: "TEXT", nullable: true),
                    rms_table_data_updated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rms_table_data_update_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rms_table_data_updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    rms_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rms_creation_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValue: DateTime.UtcNow),
                    rms_created_by = table.Column<string>(type: "TEXT", nullable: true),
                    rms_update_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rms_update_by = table.Column<string>(type: "TEXT", nullable: true),
                    rms_last_refresh_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rms_update_to_send_to_gdb = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rms_send_date_to_gdb = table.Column<DateTime>(type: "datetime", nullable: true),
                    rms_error_message = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ETQ_TR_RMS_Ref_Manager_Settings", x => x.rms_id);
                    table.ForeignKey(
                        name: "FK_ETQ_TR_RMS_Ref_Manager_Settings_ETQ_TR_RMC_Ref_Manager_Connections_rcs_id",
                        column: x => x.rcs_id,
                        principalTable: "ETQ_TR_RMC_Ref_Manager_Connections",
                        principalColumn: "rcs_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ETQ_TR_RMS_Ref_Manager_Settings_rcs_id",
                table: "ETQ_TR_RMS_Ref_Manager_Settings",
                column: "rcs_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ETQ_TR_RMS_Ref_Manager_Settings");

            migrationBuilder.DropTable(
                name: "ETQ_TR_RMC_Ref_Manager_Connections");
        }
    }
}
