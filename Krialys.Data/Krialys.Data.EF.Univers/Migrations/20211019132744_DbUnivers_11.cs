using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_11 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TEL_ETAT_LOGICIELID",
            table: "TEL_ETAT_LOGICIELS",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "UQ_TEL_ETAT_LOGICIELS",
            table: "TEL_ETAT_LOGICIELS",
            columns: new[] { "TE_ETATID", "TL_LOGICIELID" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UQ_TEL_ETAT_LOGICIELS",
            table: "TEL_ETAT_LOGICIELS");

        migrationBuilder.DropColumn(
            name: "TEL_ETAT_LOGICIELID",
            table: "TEL_ETAT_LOGICIELS");
    }
}
