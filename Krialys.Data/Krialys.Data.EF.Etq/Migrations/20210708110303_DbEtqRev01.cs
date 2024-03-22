using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev01 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTESBIS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.AddColumn<string>(
            name: "TOBJE_VERSION_ETQ_DESC",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "TEXT",
            maxLength: 250,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJE_OBJET_ETIQUETTES",
            columns: new[] { "TOBJE_CODE", "TOBJE_VERSION" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTESBIS",
            table: "TOBJE_OBJET_ETIQUETTES",
            columns: new[] { "TPRS_PROCESSUSID", "TOBJE_CODE_ETIQUETTAGE", "TOBJE_VERSION" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTESBIS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropColumn(
            name: "TOBJE_VERSION_ETQ_DESC",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.CreateIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBJE_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TOBJE_OBJET_ETIQUETTESBIS",
            table: "TOBJE_OBJET_ETIQUETTES",
            columns: new[] { "TPRS_PROCESSUSID", "TOBJE_CODE_ETIQUETTAGE" },
            unique: true);
    }
}
