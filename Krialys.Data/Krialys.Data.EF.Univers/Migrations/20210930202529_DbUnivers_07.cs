using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_07 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropColumn(
            name: "TE_LEVEL_IMPORTANCE",
            table: "TE_ETATS");

        migrationBuilder.RenameColumn(
            name: "TE_NOM_TECHNIQUE",
            table: "TE_ETATS",
            newName: "TE_NOM_SERVEUR_CUBE");

        migrationBuilder.AlterColumn<int>(
            name: "TC_CATEGORIEID",
            table: "TEM_ETAT_MASTERS",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TRU_RESPONSABLE_TECHNIQUEID",
            table: "TEM_ETAT_MASTERS",
            type: "TEXT",
            maxLength: 36,
            nullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "TE_ENV_VIERGE_TAILLE",
            table: "TE_ETATS",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<string>(
            name: "TE_COMMENTAIRE",
            table: "TE_ETATS",
            type: "TEXT",
            maxLength: 255,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 255,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_TS_SCENARIOS_TRST_STATUTID",
            table: "TS_SCENARIOS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS_TRST_STATUTID",
            table: "TEM_ETAT_MASTERS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS_TRU_RESPONSABLE_FONCTIONNELID",
            table: "TEM_ETAT_MASTERS",
            column: "TRU_RESPONSABLE_FONCTIONNELID");

        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS_TRU_RESPONSABLE_TECHNIQUEID",
            table: "TEM_ETAT_MASTERS",
            column: "TRU_RESPONSABLE_TECHNIQUEID");

        migrationBuilder.CreateIndex(
            name: "IX_TE_ETATS_TRST_STATUTID",
            table: "TE_ETATS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TE_ETATS_TRU_DECLARANTID",
            table: "TE_ETATS",
            column: "TRU_DECLARANTID");

        migrationBuilder.CreateIndex(
            name: "IX_TE_ETATS_TRU_ENV_VIERGE_AUTEURID",
            table: "TE_ETATS",
            column: "TRU_ENV_VIERGE_AUTEURID");

        migrationBuilder.AddForeignKey(
            name: "FK_TE_ETATS_TRST_STATUTS_TRST_STATUTID",
            table: "TE_ETATS",
            column: "TRST_STATUTID",
            principalTable: "TRST_STATUTS",
            principalColumn: "TRST_STATUTID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TE_ETATS_TRU_USERS_TRU_DECLARANTID",
            table: "TE_ETATS",
            column: "TRU_DECLARANTID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TE_ETATS_TRU_USERS_TRU_ENV_VIERGE_AUTEURID",
            table: "TE_ETATS",
            column: "TRU_ENV_VIERGE_AUTEURID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS_TRST_STATUTS_TRST_STATUTID",
            table: "TEM_ETAT_MASTERS",
            column: "TRST_STATUTID",
            principalTable: "TRST_STATUTS",
            principalColumn: "TRST_STATUTID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS_TRU_USERS_TRU_RESPONSABLE_FONCTIONNELID",
            table: "TEM_ETAT_MASTERS",
            column: "TRU_RESPONSABLE_FONCTIONNELID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS_TRU_USERS_TRU_RESPONSABLE_TECHNIQUEID",
            table: "TEM_ETAT_MASTERS",
            column: "TRU_RESPONSABLE_TECHNIQUEID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS",
            columns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
            principalTable: "TC_CATEGORIES",
            principalColumns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TS_SCENARIOS_TRST_STATUTS_TRST_STATUTID",
            table: "TS_SCENARIOS",
            column: "TRST_STATUTID",
            principalTable: "TRST_STATUTS",
            principalColumn: "TRST_STATUTID",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TE_ETATS_TRST_STATUTS_TRST_STATUTID",
            table: "TE_ETATS");

        migrationBuilder.DropForeignKey(
            name: "FK_TE_ETATS_TRU_USERS_TRU_DECLARANTID",
            table: "TE_ETATS");

        migrationBuilder.DropForeignKey(
            name: "FK_TE_ETATS_TRU_USERS_TRU_ENV_VIERGE_AUTEURID",
            table: "TE_ETATS");

        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS_TRST_STATUTS_TRST_STATUTID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS_TRU_USERS_TRU_RESPONSABLE_FONCTIONNELID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS_TRU_USERS_TRU_RESPONSABLE_TECHNIQUEID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropForeignKey(
            name: "FK_TS_SCENARIOS_TRST_STATUTS_TRST_STATUTID",
            table: "TS_SCENARIOS");

        migrationBuilder.DropIndex(
            name: "IX_TS_SCENARIOS_TRST_STATUTID",
            table: "TS_SCENARIOS");

        migrationBuilder.DropIndex(
            name: "IX_TEM_ETAT_MASTERS_TRST_STATUTID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropIndex(
            name: "IX_TEM_ETAT_MASTERS_TRU_RESPONSABLE_FONCTIONNELID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropIndex(
            name: "IX_TEM_ETAT_MASTERS_TRU_RESPONSABLE_TECHNIQUEID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.DropIndex(
            name: "IX_TE_ETATS_TRST_STATUTID",
            table: "TE_ETATS");

        migrationBuilder.DropIndex(
            name: "IX_TE_ETATS_TRU_DECLARANTID",
            table: "TE_ETATS");

        migrationBuilder.DropIndex(
            name: "IX_TE_ETATS_TRU_ENV_VIERGE_AUTEURID",
            table: "TE_ETATS");

        migrationBuilder.DropColumn(
            name: "TRU_RESPONSABLE_TECHNIQUEID",
            table: "TEM_ETAT_MASTERS");

        migrationBuilder.RenameColumn(
            name: "TE_NOM_SERVEUR_CUBE",
            table: "TE_ETATS",
            newName: "TE_NOM_TECHNIQUE");

        migrationBuilder.AlterColumn<int>(
            name: "TC_CATEGORIEID",
            table: "TEM_ETAT_MASTERS",
            type: "INTEGER",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "INTEGER");

        migrationBuilder.AlterColumn<int>(
            name: "TE_ENV_VIERGE_TAILLE",
            table: "TE_ETATS",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "INTEGER",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TE_COMMENTAIRE",
            table: "TE_ETATS",
            type: "TEXT",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 255);

        migrationBuilder.AddColumn<int>(
            name: "TE_LEVEL_IMPORTANCE",
            table: "TE_ETATS",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
            table: "TEM_ETAT_MASTERS",
            columns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
            principalTable: "TC_CATEGORIES",
            principalColumns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
            onDelete: ReferentialAction.Restrict);
    }
}
