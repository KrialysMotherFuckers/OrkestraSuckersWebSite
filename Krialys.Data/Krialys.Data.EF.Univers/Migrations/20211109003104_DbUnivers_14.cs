using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_14 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.AddColumn<int>(
            name: "TPD_PREREQUIS_DEMANDEID",
            table: "TPD_PREREQUIS_DEMANDES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0)
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AddColumn<int>(
            name: "TE_ETATID",
            table: "TPD_PREREQUIS_DEMANDES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<int>(
            name: "TS_SCENARIOID",
            table: "TD_DEMANDES",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AddPrimaryKey(
            name: "PK_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES",
            column: "TPD_PREREQUIS_DEMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TE_ETATID" });

        migrationBuilder.CreateIndex(
            name: "UK_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TEP_ETAT_PREREQUISID" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.DropIndex(
            name: "IX_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.DropIndex(
            name: "UK_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.DropColumn(
            name: "TPD_PREREQUIS_DEMANDEID",
            table: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.DropColumn(
            name: "TE_ETATID",
            table: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.AlterColumn<int>(
            name: "TS_SCENARIOID",
            table: "TD_DEMANDES",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_TPD_PREREQUIS_DEMANDES",
            table: "TPD_PREREQUIS_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TEP_ETAT_PREREQUISID" });
    }
}
