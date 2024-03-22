using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev12 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TPRS_PROCESSUS",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TPRS_PROCESSUS_TPRCP_PRC_PERIMETRES",
            table: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.RenameColumn(
            name: "TPRS_PROCESSUSID",
            table: "TPRCP_PRC_PERIMETRES",
            newName: "TDOM_DOMAINEID");

        migrationBuilder.RenameIndex(
            name: "IX_TPRCP_PRC_PERIMETRES_TPRS_PROCESSUSID",
            table: "TPRCP_PRC_PERIMETRES",
            newName: "IX_TPRCP_PRC_PERIMETRES_TDOM_DOMAINEID");

        migrationBuilder.RenameColumn(
            name: "TPRS_PROCESSUSID",
            table: "TOBJE_OBJET_ETIQUETTES",
            newName: "TDOM_DOMAINEID");

        migrationBuilder.AddColumn<int>(
            name: "TETQ_ETIQUETTE_ENTREEID",
            table: "TSR_SUIVI_RESSOURCES",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TTDOM_TYPE_DOMAINES",
            columns: table => new
            {
                TTDOM_TYPE_DOMAINEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TTDOM_CODE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TTDOM_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TTDOM_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTDOM_TYPE_DOMAINES", x => x.TTDOM_TYPE_DOMAINEID);
            });

        migrationBuilder.CreateTable(
            name: "TDOM_DOMAINES",
            columns: table => new
            {
                TDOM_DOMAINESID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TDOM_CODE = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TDOM_LIB = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                TDOM_DESC = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TTDOM_TYPE_DOMAINEID = table.Column<int>(type: "INTEGER", nullable: true),
                TDOM_DATE_CREATION = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRU_ACTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TDOM_DOMAINES", x => x.TDOM_DOMAINESID);
                table.ForeignKey(
                    name: "FK_TDOM_DOMAINES_TTDOM_TYPE_DOMAINES_TTDOM_TYPE_DOMAINEID",
                    column: x => x.TTDOM_TYPE_DOMAINEID,
                    principalTable: "TTDOM_TYPE_DOMAINES",
                    principalColumn: "TTDOM_TYPE_DOMAINEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TDOM_DOMAINES_TTDOM_TYPE_DOMAINEID",
            table: "TDOM_DOMAINES",
            column: "TTDOM_TYPE_DOMAINEID");

        migrationBuilder.CreateIndex(
            name: "UK_TDOM_DOMAINES",
            table: "TDOM_DOMAINES",
            column: "TDOM_CODE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_TTDOM_TYPE_DOMAINES",
            table: "TTDOM_TYPE_DOMAINES",
            column: "TTDOM_CODE",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TDOM_DOMAINEID",
            principalTable: "TDOM_DOMAINES",
            principalColumn: "TDOM_DOMAINESID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TPRCP_PRC_PERIMETRES",
            column: "TDOM_DOMAINEID",
            principalTable: "TDOM_DOMAINES",
            principalColumn: "TDOM_DOMAINESID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.Sql("INSERT INTO TTDOM_TYPE_DOMAINES(TTDOM_TYPE_DOMAINEID,TTDOM_CODE,TTDOM_LIB,TTDOM_DESC) Values(1,'GOUV','Gouvernance','Processus de gouvernance de la donnée');");
        migrationBuilder.Sql("INSERT INTO TTDOM_TYPE_DOMAINES(TTDOM_TYPE_DOMAINEID,TTDOM_CODE,TTDOM_LIB,TTDOM_DESC) Values(2,'PROD','Production','Processus de production de données');");
        migrationBuilder.Sql("INSERT INTO TTDOM_TYPE_DOMAINES(TTDOM_TYPE_DOMAINEID,TTDOM_CODE,TTDOM_LIB,TTDOM_DESC) Values(3,'IMP','Import','Processus d import de données');");


        migrationBuilder.Sql("INSERT INTO TDOM_DOMAINES(TDOM_DOMAINESID,TDOM_CODE, TDOM_LIB, TDOM_DESC, TTDOM_TYPE_DOMAINEID, TDOM_DATE_CREATION, TRU_ACTEURID) VALUES(1, 'OFS', 'Offre de service', 'Création d''une offre de service', 2, '2021-05-07 00:00:00', '7');");
        migrationBuilder.Sql("INSERT INTO TDOM_DOMAINES(TDOM_DOMAINESID,TDOM_CODE, TDOM_LIB, TDOM_DESC, TTDOM_TYPE_DOMAINEID, TDOM_DATE_CREATION, TRU_ACTEURID) VALUES(2, 'CLT', 'Clients', 'Gestion de la base clients', 1, '2021-05-07 00:00:00', '7');");
        migrationBuilder.Sql("INSERT INTO TDOM_DOMAINES(TDOM_DOMAINESID,TDOM_CODE, TDOM_LIB, TDOM_DESC, TTDOM_TYPE_DOMAINEID, TDOM_DATE_CREATION, TRU_ACTEURID)VALUES(3, 'PRS', 'Prestataires', 'Gestion de la base prestataires', 1, '2021-05-07 00:00:00', '7');");
        migrationBuilder.Sql("INSERT INTO TDOM_DOMAINES(TDOM_DOMAINESID,TDOM_CODE, TDOM_LIB, TDOM_DESC, TTDOM_TYPE_DOMAINEID, TDOM_DATE_CREATION, TRU_ACTEURID)VALUES(4, 'CPT', 'Comptabilité', 'Données comptables', 3, '2021-05-07 00:00:00', '7');");



    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TOBJE_OBJET_ETIQUETTES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TOBJE_OBJET_ETIQUETTES");

        migrationBuilder.DropForeignKey(
            name: "FK_TPRCP_PRC_PERIMETRES_TDOM_DOMAINES_TDOM_DOMAINEID",
            table: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.DropTable(
            name: "TDOM_DOMAINES");

        migrationBuilder.DropTable(
            name: "TTDOM_TYPE_DOMAINES");

        migrationBuilder.DropColumn(
            name: "TETQ_ETIQUETTE_ENTREEID",
            table: "TSR_SUIVI_RESSOURCES");

        migrationBuilder.RenameColumn(
            name: "TDOM_DOMAINEID",
            table: "TPRCP_PRC_PERIMETRES",
            newName: "TPRS_PROCESSUSID");

        migrationBuilder.RenameIndex(
            name: "IX_TPRCP_PRC_PERIMETRES_TDOM_DOMAINEID",
            table: "TPRCP_PRC_PERIMETRES",
            newName: "IX_TPRCP_PRC_PERIMETRES_TPRS_PROCESSUSID");

        migrationBuilder.RenameColumn(
            name: "TDOM_DOMAINEID",
            table: "TOBJE_OBJET_ETIQUETTES",
            newName: "TPRS_PROCESSUSID");

        migrationBuilder.AddForeignKey(
            name: "FK_TPRS_PROCESSUS",
            table: "TOBJE_OBJET_ETIQUETTES",
            column: "TPRS_PROCESSUSID",
            principalTable: "TPRS_PROCESSUS",
            principalColumn: "TPRS_PROCESSUSID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_TPRS_PROCESSUS_TPRCP_PRC_PERIMETRES",
            table: "TPRCP_PRC_PERIMETRES",
            column: "TPRS_PROCESSUSID",
            principalTable: "TPRS_PROCESSUS",
            principalColumn: "TPRS_PROCESSUSID",
            onDelete: ReferentialAction.Restrict);
    }
}
