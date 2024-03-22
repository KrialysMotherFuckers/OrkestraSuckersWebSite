using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev14 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES",
            table: "TETQ_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TEQC_ETQ_CODIFS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBF_OBJ_FORMATS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBN_OBJ_NATURES",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQ_ETIQUETTES",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS");

        migrationBuilder.DropForeignKey(
            name: "FK_TTE_TYPE_EVENEMENTS",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS");

        migrationBuilder.RenameColumn(
            name: "TDOM_DOMAINESID",
            table: "TDOM_DOMAINES",
            newName: "TDOM_DOMAINEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES",
            table: "TETQ_ETIQUETTES",
            column: "TOBJE_OBJET_ETIQUETTEID",
            principalTable: "TOBJE_OBJET_ETIQUETTES",
            principalColumn: "TOBJE_OBJET_ETIQUETTEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE",
            table: "TETQR_ETQ_REGLES",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TEQC_ETQ_CODIFS",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TEQC_ETQ_CODIFID",
            principalTable: "TEQC_ETQ_CODIFS",
            principalColumn: "TEQC_ETQ_CODIFID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBF_OBJ_FORMATS",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBF_OBJ_FORMATID",
            principalTable: "TOBF_OBJ_FORMATS",
            principalColumn: "TOBF_OBJ_FORMATID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBN_OBJ_NATURES",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBN_OBJ_NATUREID",
            principalTable: "TOBN_OBJ_NATURES",
            principalColumn: "TOBN_OBJ_NATUREID");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQ_ETIQUETTES",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TTE_TYPE_EVENEMENTS",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            column: "TTE_TYPE_EVENEMENTID",
            principalTable: "TTE_TYPE_EVENEMENTS",
            principalColumn: "TTE_TYPE_EVENEMENTID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES",
            table: "TETQ_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TEQC_ETQ_CODIFS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBF_OBJ_FORMATS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBN_OBJ_NATURES",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQ_ETIQUETTES",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS");

        migrationBuilder.DropForeignKey(
            name: "FK_TTE_TYPE_EVENEMENTS",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS");

        migrationBuilder.RenameColumn(
            name: "TDOM_DOMAINEID",
            table: "TDOM_DOMAINES",
            newName: "TDOM_DOMAINESID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES",
            table: "TETQ_ETIQUETTES",
            column: "TOBJE_OBJET_ETIQUETTEID",
            principalTable: "TOBJE_OBJET_ETIQUETTES",
            principalColumn: "TOBJE_OBJET_ETIQUETTEID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE",
            table: "TETQR_ETQ_REGLES",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TEQC_ETQ_CODIFS",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TEQC_ETQ_CODIFID",
            principalTable: "TEQC_ETQ_CODIFS",
            principalColumn: "TEQC_ETQ_CODIFID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBF_OBJ_FORMATS",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBF_OBJ_FORMATID",
            principalTable: "TOBF_OBJ_FORMATS",
            principalColumn: "TOBF_OBJ_FORMATID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBN_OBJ_NATURES",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TOBN_OBJ_NATUREID",
            principalTable: "TOBN_OBJ_NATURES",
            principalColumn: "TOBN_OBJ_NATUREID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TETQ_ETIQUETTES",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TTE_TYPE_EVENEMENTS",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            column: "TTE_TYPE_EVENEMENTID",
            principalTable: "TTE_TYPE_EVENEMENTS",
            principalColumn: "TTE_TYPE_EVENEMENTID",
            onDelete: ReferentialAction.Restrict);
    }
}
