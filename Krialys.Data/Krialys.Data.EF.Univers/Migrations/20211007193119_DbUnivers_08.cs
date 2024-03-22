using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_08 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TC_CATEGORIES$TP_PERIMETRES",
            table: "TC_CATEGORIES");

        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropIndex(
            name: "IX_TEM_ETAT_MASTERS_TC_CATEGORIEID_TP_PERIMETREID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropPrimaryKey(
            name: "PK_TC_CATEGORIES",
            table: "TC_CATEGORIES");

        migrationBuilder.DropIndex(
            name: "IX_TC_CATEGORIES_TP_PERIMETREID",
            table: "TC_CATEGORIES");

        migrationBuilder.DropColumn(
            name: "TP_PERIMETREID",
            table: "TC_CATEGORIES");

        migrationBuilder.AlterColumn<int>(
            name: "TC_CATEGORIEID",
            table: "TC_CATEGORIES",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_CATEGORIES",
            table: "TC_CATEGORIES",
            column: "TC_CATEGORIEID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_CATEGORIES",
            table: "TC_CATEGORIES");

        migrationBuilder.AlterColumn<int>(
            name: "TC_CATEGORIEID",
            table: "TC_CATEGORIES",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddColumn<int>(
            name: "TP_PERIMETREID",
            table: "TC_CATEGORIES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddPrimaryKey(
            name: "PK_TC_CATEGORIES",
            table: "TC_CATEGORIES",
            columns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" });

        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS_TC_CATEGORIEID_TP_PERIMETREID",
            table: "TEM_ETAT_MASTERS",
            columns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" });

        migrationBuilder.CreateIndex(
            name: "IX_TC_CATEGORIES_TP_PERIMETREID",
            table: "TC_CATEGORIES",
            column: "TP_PERIMETREID");

        migrationBuilder.AddForeignKey(
            name: "FK_TC_CATEGORIES$TP_PERIMETRES",
            table: "TC_CATEGORIES",
            column: "TP_PERIMETREID",
            principalTable: "TP_PERIMETRES",
            principalColumn: "TP_PERIMETREID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS",
            columns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
            principalTable: "TC_CATEGORIES",
            principalColumns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
            onDelete: ReferentialAction.Cascade);
    }
}
