using Krialys.Data.EF.RefManager;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TR_CNX_Connections",
                columns: table => new
                {
                    cnx_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cnx_code = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_label = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_database_type = table.Column<int>(type: "INTEGER", nullable: false),
                    cnx_server_name = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_port = table.Column<int>(type: "INTEGER", nullable: false),
                    cnx_database_name = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_login = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_password = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_creation_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    cnx_is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TR_CNX_Connections", x => x.cnx_id);
                });

            migrationBuilder.CreateTable(
                name: "TM_RFS_ReferentialSettings",
                columns: table => new
                {
                    rfs_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rfs_table_name = table.Column<string>(type: "TEXT", nullable: false),
                    rfs_table_functional_name = table.Column<string>(type: "TEXT", nullable: false),
                    cnx_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rfs_table_schema = table.Column<string>(type: "TEXT", nullable: false),
                    rfs_table_query_select = table.Column<string>(type: "TEXT", nullable: false),
                    rfs_table_query_insert = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_query_delete = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_query_update = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_query_update_columns = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_query_update_keys = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_query_criteria = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_description = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_typology = table.Column<int>(type: "int", nullable: false),
                    rfs_request_id = table.Column<int>(type: "int", nullable: true),
                    rfs_param_label_object_code = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_label_code_fieldname = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_documentation = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_manager_id = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_label_data_cloned_in_progress_list_json = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_data_to_approuve = table.Column<bool>(type: "boolean", nullable: false),
                    rfs_table_data_approved = table.Column<bool>(type: "boolean", nullable: false),
                    rfs_table_data_approval_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfs_table_data_approved_by = table.Column<string>(type: "datetime", nullable: true),
                    rfs_table_data_need_to_be_refreshed = table.Column<bool>(type: "boolean", nullable: false),
                    rfs_last_refresh_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfs_is_update_sent_to_gdb = table.Column<bool>(type: "boolean", nullable: false),
                    rfs_send_date_to_gdb = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfs_is_backup_needed = table.Column<bool>(type: "boolean", nullable: false),
                    rfs_error_message = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_table_data_max_rows_expected_to_receive = table.Column<int>(type: "INTEGER", nullable: true),
                    rfs_table_data_min_rows_expected_to_send = table.Column<int>(type: "INTEGER", nullable: true),
                    rfs_table_data_max_rows_expected_to_send = table.Column<int>(type: "INTEGER", nullable: true),
                    rfs_status_code = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_creation_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    rfs_created_by = table.Column<string>(type: "TEXT", nullable: true),
                    rfs_update_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfs_update_by = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_RFS_ReferentialSettings", x => x.rfs_id);
                    table.ForeignKey(
                        name: "FK_TM_RFS_ReferentialSettings_TR_CNX_Connections_cnx_id",
                        column: x => x.cnx_id,
                        principalTable: "TR_CNX_Connections",
                        principalColumn: "cnx_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TX_RFH_ReferentialHistorical",
                columns: table => new
                {
                    rfh_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rfs_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rfh_table_name = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_table_functional_name = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_description = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_status_code = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_update_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfh_update_by = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_process_type = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_process_status = table.Column<string>(type: "TEXT", nullable: true),
                    rfh_label_code = table.Column<string>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TX_RFH_ReferentialHistorical", x => x.rfh_id);
                    table.ForeignKey(
                        name: "FK_TX_RFH_ReferentialHistorical_TM_RFS_ReferentialSettings_rfs_id",
                        column: x => x.rfs_id,
                        principalTable: "TM_RFS_ReferentialSettings",
                        principalColumn: "rfs_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TX_RFX_ReferentialSettingsData",
                columns: table => new
                {
                    rfx_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rfs_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rfx_table_data = table.Column<string>(type: "TEXT", nullable: true),
                    rfx_table_metadata = table.Column<string>(type: "TEXT", nullable: true),
                    rfx_table_data_update_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfx_table_data_updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    rfx_table_data_backup = table.Column<string>(type: "TEXT", nullable: true),
                    rfx_table_metadata_backup = table.Column<string>(type: "TEXT", nullable: true),
                    rfx_table_data_backup_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    rfx_table_data_backup_updated_by = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TX_RFX_ReferentialSettingsData", x => x.rfx_id);
                    table.ForeignKey(
                        name: "FK_TX_RFX_ReferentialSettingsData_TM_RFS_ReferentialSettings_rfs_id",
                        column: x => x.rfs_id,
                        principalTable: "TM_RFS_ReferentialSettings",
                        principalColumn: "rfs_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TM_RFS_ReferentialSettings_cnx_id",
                table: "TM_RFS_ReferentialSettings",
                column: "cnx_id");

            migrationBuilder.CreateIndex(
                name: "IX_TX_RFH_ReferentialHistorical_rfs_id",
                table: "TX_RFH_ReferentialHistorical",
                column: "rfs_id");

            migrationBuilder.CreateIndex(
                name: "IX_TX_RFX_ReferentialSettingsData_rfs_id",
                table: "TX_RFX_ReferentialSettingsData",
                column: "rfs_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TX_RFH_ReferentialHistorical");

            migrationBuilder.DropTable(
                name: "TX_RFX_ReferentialSettingsData");

            migrationBuilder.DropTable(
                name: "TM_RFS_ReferentialSettings");

            migrationBuilder.DropTable(
                name: "TR_CNX_Connections");
        }
    }
}
