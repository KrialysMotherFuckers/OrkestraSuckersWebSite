using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_55 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TR_WST_WebSite_Settings",
                columns: table => new
                {
                    wst_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    wst_file_storage_in_database = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TR_WST_WebSite_Settings", x => x.wst_id);
                });

            migrationBuilder.Sql(@"INSERT INTO TR_WST_WebSite_Settings (wst_file_storage_in_database) VALUES (0);");

            migrationBuilder.Sql("PRAGMA journal_mode=\"DELETE\"", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TR_WST_WebSite_Settings");
        }
    }
}
