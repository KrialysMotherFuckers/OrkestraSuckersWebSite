using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LogUnivers",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                Level = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                Exception = table.Column<string>(type: "TEXT", nullable: true),
                RenderedMessage = table.Column<string>(type: "TEXT", nullable: true),
                Properties = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("Id", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LogUnivers");
    }
}
