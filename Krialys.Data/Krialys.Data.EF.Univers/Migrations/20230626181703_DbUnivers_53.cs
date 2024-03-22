using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_53 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TM_LIC_Licence",
                columns: table => new
                {
                    lic_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    lic_product_name = table.Column<string>(type: "TEXT", nullable: false),
                    lic_licence_key = table.Column<string>(type: "TEXT", nullable: false),
                    lic_issued_to = table.Column<string>(type: "TEXT", nullable: false),
                    lic_issued_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    lic_expiration_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    lic_is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_LIC_Licence", x => x.lic_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM_LIC_Licence");
        }
    }
}
