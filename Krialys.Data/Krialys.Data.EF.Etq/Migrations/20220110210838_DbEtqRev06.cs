using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev06 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TSEQ_COMMENTAIRE",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS",
            type: "TEXT",
            maxLength: 250,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TACT_ACTIONS",
            columns: table => new
            {
                TACT_ACTIONID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TACT_CODE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                TACT_LIB = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                TACT_DESC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TACT_ACTIONS", x => x.TACT_ACTIONID);
            });

        migrationBuilder.CreateTable(
            name: "TRGL_REGLES",
            columns: table => new
            {
                TRGL_REGLEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRGL_CODE_REGLE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                TRGL_LIB_REGLE = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                TRGL_DESC_REGLE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                TRGL_LIMITE_TEMPS = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRGL_REGLES", x => x.TRGL_REGLEID);
            });

        migrationBuilder.CreateTable(
            name: "TOBJR_OBJET_REGLES",
            columns: table => new
            {
                TOBJR_OBJET_REGLEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TOBJE_OBJET_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false),
                TRGL_REGLEID = table.Column<int>(type: "INTEGER", nullable: false),
                TOBJR_VALEUR = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false),
                TOBJR_APPLICABLE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TOBJR_ECHEANCE_DUREE = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TOBJR_OBJET_REGLES", x => x.TOBJR_OBJET_REGLEID);
                table.ForeignKey(
                    name: "FK_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTES_TOBJE_OBJET_ETIQUETTEID",
                    column: x => x.TOBJE_OBJET_ETIQUETTEID,
                    principalTable: "TOBJE_OBJET_ETIQUETTES",
                    principalColumn: "TOBJE_OBJET_ETIQUETTEID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TOBJR_OBJET_REGLES_TRGL_REGLES_TRGL_REGLEID",
                    column: x => x.TRGL_REGLEID,
                    principalTable: "TRGL_REGLES",
                    principalColumn: "TRGL_REGLEID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TRGLRV_REGLES_VALEURS",
            columns: table => new
            {
                TRGLRV_REGLES_VALEURID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRGL_REGLEID = table.Column<int>(type: "INTEGER", nullable: false),
                TRGLRV_VALEUR = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                TRGLRV_ORDRE_AFFICHAGE = table.Column<int>(type: "INTEGER", nullable: false),
                TRGLRV_DEPART_LIMITE_TEMPS = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TACT_ACTIONID = table.Column<int>(type: "INTEGER", nullable: true),
                TRGLRV_VALEUR_ECHEANCE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TRGLRV_VALEUR_DEFAUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRGLRV_REGLES_VALEURS", x => x.TRGLRV_REGLES_VALEURID);
                table.ForeignKey(
                    name: "FK_TRGLRV_REGLES_VALEURS_TACT_ACTIONS_TACT_ACTIONID",
                    column: x => x.TACT_ACTIONID,
                    principalTable: "TACT_ACTIONS",
                    principalColumn: "TACT_ACTIONID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRGLRV_REGLES_VALEURS_TRGL_REGLES_TRGL_REGLEID",
                    column: x => x.TRGL_REGLEID,
                    principalTable: "TRGL_REGLES",
                    principalColumn: "TRGL_REGLEID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TETQR_ETQ_REGLES",
            columns: table => new
            {
                TOBJR_OBJET_REGLEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TETQ_ETIQUETTEID = table.Column<int>(type: "INTEGER", nullable: false),
                TRGLRV_REGLES_VALEURID = table.Column<int>(type: "INTEGER", nullable: false),
                TETQR_ECHEANCE = table.Column<DateTime>(type: "TEXT", nullable: true),
                TETQR_DATE_DEBUT = table.Column<DateTime>(type: "TEXT", nullable: true),
                TETQR_DATE_FIN = table.Column<DateTime>(type: "TEXT", nullable: true),
                TETQR_LIMITE_ATTEINTE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TETQR_ETQ_REGLES_ACTION = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                TETQR_REGLE_LIEE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TETQR_ETQ_REGLES", x => x.TOBJR_OBJET_REGLEID);
                table.ForeignKey(
                    name: "FK_TETQR_ETQ_REGLES_TETQ_ETIQUETTE",
                    column: x => x.TETQ_ETIQUETTEID,
                    principalTable: "TETQ_ETIQUETTES",
                    principalColumn: "TETQ_ETIQUETTEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEUR",
                    column: x => x.TRGLRV_REGLES_VALEURID,
                    principalTable: "TRGLRV_REGLES_VALEURS",
                    principalColumn: "TRGLRV_REGLES_VALEURID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TRGLI_REGLES_LIEES",
            columns: table => new
            {
                TRGL_REGLELIEEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRGLRV_REGLES_VALEURID = table.Column<int>(type: "INTEGER", nullable: false),
                TRGLRV_REGLES_VALEURLIEEID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRGLI_REGLES_LIEES", x => x.TRGL_REGLELIEEID);
                table.ForeignKey(
                    name: "FK_TRGLI_REGLES_LIEES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURID",
                    column: x => x.TRGLRV_REGLES_VALEURID,
                    principalTable: "TRGLRV_REGLES_VALEURS",
                    principalColumn: "TRGLRV_REGLES_VALEURID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TRGLI_REGLES_LIEES_TRGLRV_REGLES_VALEURS_TRGLRV_REGLES_VALEURLIEEID",
                    column: x => x.TRGLRV_REGLES_VALEURLIEEID,
                    principalTable: "TRGLRV_REGLES_VALEURS",
                    principalColumn: "TRGLRV_REGLES_VALEURID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "UK_TACT_CODE",
            table: "TACT_ACTIONS",
            column: "TACT_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TETQR_ETQ_REGLES_TRGLRV_REGLES_VALEURID",
            table: "TETQR_ETQ_REGLES",
            column: "TRGLRV_REGLES_VALEURID");

        migrationBuilder.CreateIndex(
            name: "UK_TETQR_ETQ_REGLES",
            table: "TETQR_ETQ_REGLES",
            columns: new[] { "TETQ_ETIQUETTEID", "TRGLRV_REGLES_VALEURID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TOBJR_OBJET_REGLES_TRGL_REGLEID",
            table: "TOBJR_OBJET_REGLES",
            column: "TRGL_REGLEID");

        migrationBuilder.CreateIndex(
            name: "UK_TOBJR_OBJET_REGLES",
            table: "TOBJR_OBJET_REGLES",
            columns: new[] { "TOBJE_OBJET_ETIQUETTEID", "TRGL_REGLEID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TRGL_REGLES",
            table: "TRGL_REGLES",
            column: "TRGL_CODE_REGLE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TRGLI_REGLES_LIEES_TRGLRV_REGLES_VALEURLIEEID",
            table: "TRGLI_REGLES_LIEES",
            column: "TRGLRV_REGLES_VALEURLIEEID");

        migrationBuilder.CreateIndex(
            name: "UK_TRGLI_REGLES_LIEES",
            table: "TRGLI_REGLES_LIEES",
            columns: new[] { "TRGLRV_REGLES_VALEURID", "TRGLRV_REGLES_VALEURLIEEID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TRGLRV_REGLES_VALEURS_TACT_ACTIONID",
            table: "TRGLRV_REGLES_VALEURS",
            column: "TACT_ACTIONID");

        migrationBuilder.CreateIndex(
            name: "UK_TRGLRV_REGLES_VALEURS",
            table: "TRGLRV_REGLES_VALEURS",
            columns: new[] { "TRGL_REGLEID", "TRGLRV_VALEUR" },
            unique: true);


        migrationBuilder.Sql("INSERT INTO TTE_TYPE_EVENEMENTS(TTE_CODE, TTE_LIB) Values('ETQ_CREATION','Création étiquette');");
        migrationBuilder.Sql("INSERT INTO TTE_TYPE_EVENEMENTS(TTE_CODE, TTE_LIB) Values('ETQ_DATA_MAJ','MAJ données étiquette');");
        migrationBuilder.Sql("INSERT INTO TTE_TYPE_EVENEMENTS(TTE_CODE, TTE_LIB) Values('ETQ_REGLE_MAJ','MAJ qualification étiquette');");
        migrationBuilder.Sql("INSERT INTO TTE_TYPE_EVENEMENTS(TTE_CODE, TTE_LIB) Values('ETQ_STOCKAGE','Stockage étiquette');");
        migrationBuilder.Sql("INSERT INTO TTE_TYPE_EVENEMENTS(TTE_CODE, TTE_LIB) Values('ETQ_APPRO_MAJ','Maj du statut d aprobation');");

        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('ARCH','Archivage','La donnée doit être archivée à partir de l échéance indiquée','ECHEANCE');");
        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('SUPP','Suppression','La donnée doit être supprimée à partir de l échéance indiquée','ECHEANCE');");
        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('CERTIF','Certification','La donnée doit être certifiée','DUREE');");
        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('APPRO','Approbation','La donnée doit être approuvée','DUREE');");
        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('CONFID','Confidentialité','Définition d un niveau de confidentialité','AUCUNE');");
        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('DIFF_ETAT','Etat diffusion','Définition de l état de diffusion','PERIODE');");
        migrationBuilder.Sql("INSERT INTO TRGL_REGLES(TRGL_CODE_REGLE, TRGL_LIB_REGLE,TRGL_DESC_REGLE,TRGL_LIMITE_TEMPS) Values('DIFF_PERIM','Périmètre de diffusion','Définition du périmètre de diffusion','AUCUNE');");

        migrationBuilder.Sql("INSERT INTO TACT_ACTIONS(TACT_CODE, TACT_LIB,TACT_DESC) Values('DDE_QUALIF','Demande de qualification','Lorsqu une étiquette nécessite d être qualifiée (exemple approuvée, rejetée) elle est associée à une demande de qualification.');");
        migrationBuilder.Sql("INSERT INTO TACT_ACTIONS(TACT_CODE, TACT_LIB,TACT_DESC) Values('SUIVI_QUALIF','Suivi de qualification','Lorsqu une étiquette vient d être qualifiée elle est signalée dans le suivi des qualifications.');");
        migrationBuilder.Sql("INSERT INTO TACT_ACTIONS(TACT_CODE, TACT_LIB,TACT_DESC) Values('SUIVI_ECHEANCE','Suivi des écheances','Lorsqu une étiquette arrive à échéance, elle est signalée dans le suivi des échéances.');");
        migrationBuilder.Sql("INSERT INTO TACT_ACTIONS(TACT_CODE, TACT_LIB,TACT_DESC) Values('PERIODE_EXPIREE','Suivi des périodes expirées','Lorsqu une période définie sur une étiquette est dépassée, elle est signalée comme étant expirée.');");


    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TETQR_ETQ_REGLES");

        migrationBuilder.DropTable(
            name: "TOBJR_OBJET_REGLES");

        migrationBuilder.DropTable(
            name: "TRGLI_REGLES_LIEES");

        migrationBuilder.DropTable(
            name: "TRGLRV_REGLES_VALEURS");

        migrationBuilder.DropTable(
            name: "TACT_ACTIONS");

        migrationBuilder.DropTable(
            name: "TRGL_REGLES");

        migrationBuilder.DropColumn(
            name: "TSEQ_COMMENTAIRE",
            table: "TSEQ_SUIVI_EVENEMENT_ETQS");
    }
}
