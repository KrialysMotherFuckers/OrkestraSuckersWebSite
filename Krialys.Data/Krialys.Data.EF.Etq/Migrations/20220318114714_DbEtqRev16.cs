using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev16 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES_TTDOM_TYPE_DOMAINEID",
            table: "TDOM_DOMAINES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGL_REGLES_TRGL_REGLEID",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES_TOBJE_OBJET_ETIQUETTEID",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGL_REGLES_TRGL_REGLEID",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.DropForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.AddForeignKey(
            name: "FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES",
            table: "TDOM_DOMAINES",
            column: "TTDOM_TYPE_DOMAINEID",
            principalTable: "TTDOM_TYPE_DOMAINES",
            principalColumn: "TTDOM_TYPE_DOMAINEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGL_REGLE",
            table: "TETQR_ETQ_REGLES",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR",
            table: "TETQR_ETQ_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJR_OBJET_REGLES",
            column: "TOBJE_OBJET_ETIQUETTEID",
            principalTable: "TOBJE_OBJET_ETIQUETTES",
            principalColumn: "TOBJE_OBJET_ETIQUETTEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGL_REGLES",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEUR",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID");

        migrationBuilder.AddForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTE",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID");

        migrationBuilder.AddForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCE",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TTR_TYPE_RESSOURCEID",
            principalTable: "TTR_TYPE_RESSOURCES",
            principalColumn: "TTR_TYPE_RESSOURCEID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES",
            table: "TDOM_DOMAINES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGL_REGLE",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR",
            table: "TETQR_ETQ_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGL_REGLES",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEUR",
            table: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTE",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.DropForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCE",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.AddForeignKey(
            name: "FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES_TTDOM_TYPE_DOMAINEID",
            table: "TDOM_DOMAINES",
            column: "TTDOM_TYPE_DOMAINEID",
            principalTable: "TTDOM_TYPE_DOMAINES",
            principalColumn: "TTDOM_TYPE_DOMAINEID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGL_REGLES_TRGL_REGLEID",
            table: "TETQR_ETQ_REGLES",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES_TOBJE_OBJET_ETIQUETTEID",
            table: "TOBJR_OBJET_REGLES",
            column: "TOBJE_OBJET_ETIQUETTEID",
            principalTable: "TOBJE_OBJET_ETIQUETTES",
            principalColumn: "TOBJE_OBJET_ETIQUETTEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGL_REGLES_TRGL_REGLEID",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGL_REGLEID",
            principalTable: "TRGL_REGLES",
            principalColumn: "TRGL_REGLEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGLRV_REGLES_VALEURID",
            principalTable: "TRGLRV_REGLES_VALEURS",
            principalColumn: "TRGLRV_REGLES_VALEURID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TETQ_ETIQUETTES_TETQ_ETIQUETTEID",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TETQ_ETIQUETTEID",
            principalTable: "TETQ_ETIQUETTES",
            principalColumn: "TETQ_ETIQUETTEID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TSR_SUIVI_RESSOURCES_TTR_TYPE_RESSOURCES_TTR_TYPE_RESSOURCEID",
            table: "TSR_SUIVI_RESSOURCES",
            column: "TTR_TYPE_RESSOURCEID",
            principalTable: "TTR_TYPE_RESSOURCES",
            principalColumn: "TTR_TYPE_RESSOURCEID",
            onDelete: ReferentialAction.Cascade);
    }
}
