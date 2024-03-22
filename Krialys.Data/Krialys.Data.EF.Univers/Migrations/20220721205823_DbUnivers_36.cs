using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_36 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDTFH_HABILITATIONS;");

        migrationBuilder.Sql(@"UPDATE TH_HABILITATIONS set TRU_USERID=null where TRU_USERID='';");

        migrationBuilder.AddColumn<string>(
            name: "TE_FULLNAME",
            table: "TE_ETATS",
            type: "TEXT",
            nullable: true,
            computedColumnSql: "[TE_NOM_ETAT] || ' (' || [TE_INDICE_REVISION_L1] || '.' || [TE_INDICE_REVISION_L2] || '.' || [TE_INDICE_REVISION_L3] ||')'");

        migrationBuilder.CreateIndex(
            name: "IX_TH_HABILITATIONS_TRU_INITIALISATION_AUTEURID",
            table: "TH_HABILITATIONS",
            column: "TRU_INITIALISATION_AUTEURID");

        migrationBuilder.AddForeignKey(
            name: "FK_TH_HABILITATIONS$TRU_USERS",
            table: "TH_HABILITATIONS",
            column: "TRU_USERID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID");

        migrationBuilder.AddForeignKey(
            name: "FK_TH_HABILITATIONS$TRU_USERS$INITIALISATION_AUTEUR",
            table: "TH_HABILITATIONS",
            column: "TRU_INITIALISATION_AUTEURID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TH_HABILITATIONS$TRU_USERS",
            table: "TH_HABILITATIONS");

        migrationBuilder.DropForeignKey(
            name: "FK_TH_HABILITATIONS$TRU_USERS$INITIALISATION_AUTEUR",
            table: "TH_HABILITATIONS");

        migrationBuilder.DropIndex(
            name: "IX_TH_HABILITATIONS_TRU_INITIALISATION_AUTEURID",
            table: "TH_HABILITATIONS");

        migrationBuilder.DropColumn(
            name: "TE_FULLNAME",
            table: "TE_ETATS");
    }
}
