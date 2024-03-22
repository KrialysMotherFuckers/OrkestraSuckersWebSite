using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_39 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "TE_ENV_VIERGE_DATE_DIAG_VALIDE",
            table: "TE_ETATS",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP",
            table: "TE_ETATS",
            type: "TEXT",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TCMD_MC_MODE_CREATIONS",
            columns: table => new
            {
                TCMD_MC_MODE_CREATIONID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_MC_CODE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_MC_LIB_FR = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_MC_LIB_EN = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_MC_MODE_CREATIONS", x => x.TCMD_MC_MODE_CREATIONID);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_PH_PHASES",
            columns: table => new
            {
                TCMD_PH_PHASEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_PH_CODE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_PH_LIB_FR = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_PH_LIB_EN = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_PH_PHASES", x => x.TCMD_PH_PHASEID);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_TD_TYPE_DOCUMENTS",
            columns: table => new
            {
                TCMD_TD_TYPE_DOCUMENTID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_TD_TYPE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_TD_COMMENTAIRE = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_TD_TYPE_DOCUMENTS", x => x.TCMD_TD_TYPE_DOCUMENTID);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_COMMANDES",
            columns: table => new
            {
                TCMD_COMMANDEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_DATE_CREATION = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_DATE_PASSAGE_CMD = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_DATE_LIVRAISON_SOUHAITEE = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_DATE_MODIF = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_DATE_PRISE_EN_CHARGE = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_DATE_PREVISIONNELLE_LIVRAISON = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_DATE_LIVRAISON = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_DATE_CLOTURE = table.Column<string>(type: "TEXT", nullable: true),
                TRU_COMMANDITAIREID = table.Column<string>(type: "TEXT", nullable: false),
                TRU_EXPLOITANTID = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_COMMENTAIRE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_PH_PHASEID = table.Column<int>(type: "INTEGER", nullable: false),
                TCMD_ORIGINEID = table.Column<int>(type: "INTEGER", nullable: true),
                TCMD_MC_MODE_CREATIONID = table.Column<int>(type: "INTEGER", nullable: false),
                TDOM_DOMAINEID = table.Column<int>(type: "INTEGER", nullable: true),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: true),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_COMMANDES", x => x.TCMD_COMMANDEID);
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TCMD_COMMANDES_TCMD_ORIGINEID",
                    column: x => x.TCMD_ORIGINEID,
                    principalTable: "TCMD_COMMANDES",
                    principalColumn: "TCMD_COMMANDEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TCMD_MC_MODE_CREATIONS_TCMD_MC_MODE_CREATIONID",
                    column: x => x.TCMD_MC_MODE_CREATIONID,
                    principalTable: "TCMD_MC_MODE_CREATIONS",
                    principalColumn: "TCMD_MC_MODE_CREATIONID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TCMD_PH_PHASES_TCMD_PH_PHASEID",
                    column: x => x.TCMD_PH_PHASEID,
                    principalTable: "TCMD_PH_PHASES",
                    principalColumn: "TCMD_PH_PHASEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TE_ETATS_TE_ETATID",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID");
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TRU_USERS_TRU_COMMANDITAIREID",
                    column: x => x.TRU_COMMANDITAIREID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TRU_USERS_TRU_EXPLOITANTID",
                    column: x => x.TRU_EXPLOITANTID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_COMMANDES_TS_SCENARIOS_TS_SCENARIOID",
                    column: x => x.TS_SCENARIOID,
                    principalTable: "TS_SCENARIOS",
                    principalColumn: "TS_SCENARIOID");
            });

        migrationBuilder.CreateTable(
            name: "TCMD_RP_RAISON_PHASES",
            columns: table => new
            {
                TCMD_RP_RAISON_PHASEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_RP_LIB_FR = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_RP_LIB_EN = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_PH_PHASEID = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_RP_RAISON_PHASES", x => x.TCMD_RP_RAISON_PHASEID);
                table.ForeignKey(
                    name: "FK_TCMD_RP_RAISON_PHASES_TCMD_PH_PHASES_TCMD_PH_PHASEID",
                    column: x => x.TCMD_PH_PHASEID,
                    principalTable: "TCMD_PH_PHASES",
                    principalColumn: "TCMD_PH_PHASEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_DA_DEMANDES_ASSOCIEES",
            columns: table => new
            {
                TCMD_DA_DEMANDES_ASSOCIEEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_COMMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TCMD_DA_COMMENTAIRE = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_DA_VERSION_NOTABLE = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "0"),
                TCMD_DA_DATE_ASSOCIATION = table.Column<string>(type: "TEXT", nullable: false),
                TRU_AUTEURID = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_DA_DEMANDES_ASSOCIEES", x => x.TCMD_DA_DEMANDES_ASSOCIEEID);
                table.ForeignKey(
                    name: "FK_TCMD_DA_DEMANDES_ASSOCIEES_TCMD_COMMANDES_TCMD_COMMANDEID",
                    column: x => x.TCMD_COMMANDEID,
                    principalTable: "TCMD_COMMANDES",
                    principalColumn: "TCMD_COMMANDEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_DA_DEMANDES_ASSOCIEES_TD_DEMANDES_TD_DEMANDEID",
                    column: x => x.TD_DEMANDEID,
                    principalTable: "TD_DEMANDES",
                    principalColumn: "TD_DEMANDEID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TCMD_DA_DEMANDES_ASSOCIEES_TRU_USERS_TRU_AUTEURID",
                    column: x => x.TRU_AUTEURID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_DOC_DOCUMENTS",
            columns: table => new
            {
                TCMD_DOC_DOCUMENTID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_DOC_FILENAME = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_DOC_DATE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_DOC_COMMENTAIRE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_COMMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TCMD_TD_TYPE_DOCUMENTID = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_DOC_DOCUMENTS", x => x.TCMD_DOC_DOCUMENTID);
                table.ForeignKey(
                    name: "FK_TCMD_DOC_DOCUMENTS_TCMD_COMMANDES_TCMD_COMMANDEID",
                    column: x => x.TCMD_COMMANDEID,
                    principalTable: "TCMD_COMMANDES",
                    principalColumn: "TCMD_COMMANDEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_DOC_DOCUMENTS_TCMD_TD_TYPE_DOCUMENTS_TCMD_TD_TYPE_DOCUMENTID",
                    column: x => x.TCMD_TD_TYPE_DOCUMENTID,
                    principalTable: "TCMD_TD_TYPE_DOCUMENTS",
                    principalColumn: "TCMD_TD_TYPE_DOCUMENTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_SP_SUIVI_PHASES",
            columns: table => new
            {
                TCMD_SP_SUIVI_PHASEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_COMMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TCMD_SP_DATE_MODIF = table.Column<string>(type: "TEXT", nullable: true),
                TRU_AUTEUR_MODIFID = table.Column<string>(type: "TEXT", nullable: true),
                TCMD_SP_COMMENTAIRE = table.Column<string>(type: "TEXT", nullable: false),
                TCMD_PH_PHASE_AVANTID = table.Column<int>(type: "INTEGER", nullable: true),
                TCMD_PH_PHASE_APRESID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_SP_SUIVI_PHASES", x => x.TCMD_SP_SUIVI_PHASEID);
                table.ForeignKey(
                    name: "FK_TCMD_SP_SUIVI_PHASES_TCMD_COMMANDES_TCMD_COMMANDEID",
                    column: x => x.TCMD_COMMANDEID,
                    principalTable: "TCMD_COMMANDES",
                    principalColumn: "TCMD_COMMANDEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASES_TCMD_PH_PHASE_APRESID",
                    column: x => x.TCMD_PH_PHASE_APRESID,
                    principalTable: "TCMD_PH_PHASES",
                    principalColumn: "TCMD_PH_PHASEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASES_TCMD_PH_PHASE_AVANTID",
                    column: x => x.TCMD_PH_PHASE_AVANTID,
                    principalTable: "TCMD_PH_PHASES",
                    principalColumn: "TCMD_PH_PHASEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_SP_SUIVI_PHASES_TRU_USERS_TRU_AUTEUR_MODIFID",
                    column: x => x.TRU_AUTEUR_MODIFID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TDC_DEMANDES_COMMANDES",
            columns: table => new
            {
                TDC_DEMANDES_COMMANDEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_COMMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TDC_DEMANDES_COMMANDES", x => x.TDC_DEMANDES_COMMANDEID);
                table.ForeignKey(
                    name: "FK_TDC_DEMANDES_COMMANDES_TCMD_COMMANDES_TCMD_COMMANDEID",
                    column: x => x.TCMD_COMMANDEID,
                    principalTable: "TCMD_COMMANDES",
                    principalColumn: "TCMD_COMMANDEID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TDC_DEMANDES_COMMANDES_TD_DEMANDES_TD_DEMANDEID",
                    column: x => x.TD_DEMANDEID,
                    principalTable: "TD_DEMANDES",
                    principalColumn: "TD_DEMANDEID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TCMD_CR_CMD_RAISON_PHASES",
            columns: table => new
            {
                TCMD_CR_CMD_RAISON_PHASEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TCMD_SP_SUIVI_PHASEID = table.Column<int>(type: "INTEGER", nullable: false),
                TCMD_RP_RAISON_PHASEID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TCMD_CR_CMD_RAISON_PHASES", x => x.TCMD_CR_CMD_RAISON_PHASEID);
                table.ForeignKey(
                    name: "FK_TCMD_CR_CMD_RAISON_PHASES_TCMD_RP_RAISON_PHASES_TCMD_RP_RAISON_PHASEID",
                    column: x => x.TCMD_RP_RAISON_PHASEID,
                    principalTable: "TCMD_RP_RAISON_PHASES",
                    principalColumn: "TCMD_RP_RAISON_PHASEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TCMD_CR_CMD_RAISON_PHASES_TCMD_SP_SUIVI_PHASES_TCMD_SP_SUIVI_PHASEID",
                    column: x => x.TCMD_SP_SUIVI_PHASEID,
                    principalTable: "TCMD_SP_SUIVI_PHASES",
                    principalColumn: "TCMD_SP_SUIVI_PHASEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TCMD_MC_MODE_CREATIONID",
            table: "TCMD_COMMANDES",
            column: "TCMD_MC_MODE_CREATIONID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TCMD_ORIGINEID",
            table: "TCMD_COMMANDES",
            column: "TCMD_ORIGINEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TCMD_PH_PHASEID",
            table: "TCMD_COMMANDES",
            column: "TCMD_PH_PHASEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TE_ETATID",
            table: "TCMD_COMMANDES",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TRU_COMMANDITAIREID",
            table: "TCMD_COMMANDES",
            column: "TRU_COMMANDITAIREID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TRU_EXPLOITANTID",
            table: "TCMD_COMMANDES",
            column: "TRU_EXPLOITANTID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_COMMANDES_TS_SCENARIOID",
            table: "TCMD_COMMANDES",
            column: "TS_SCENARIOID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_CR_CMD_RAISON_PHASES_TCMD_RP_RAISON_PHASEID",
            table: "TCMD_CR_CMD_RAISON_PHASES",
            column: "TCMD_RP_RAISON_PHASEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_CR_CMD_RAISON_PHASES_TCMD_SP_SUIVI_PHASEID",
            table: "TCMD_CR_CMD_RAISON_PHASES",
            column: "TCMD_SP_SUIVI_PHASEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_DA_DEMANDES_ASSOCIEES_TCMD_COMMANDEID",
            table: "TCMD_DA_DEMANDES_ASSOCIEES",
            column: "TCMD_COMMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_DA_DEMANDES_ASSOCIEES_TD_DEMANDEID",
            table: "TCMD_DA_DEMANDES_ASSOCIEES",
            column: "TD_DEMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_DA_DEMANDES_ASSOCIEES_TRU_AUTEURID",
            table: "TCMD_DA_DEMANDES_ASSOCIEES",
            column: "TRU_AUTEURID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_DOC_DOCUMENTS",
            table: "TCMD_DOC_DOCUMENTS",
            columns: new[] { "TCMD_COMMANDEID", "TCMD_DOC_FILENAME" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_DOC_DOCUMENTS_TCMD_TD_TYPE_DOCUMENTID",
            table: "TCMD_DOC_DOCUMENTS",
            column: "TCMD_TD_TYPE_DOCUMENTID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_RP_RAISON_PHASES_TCMD_PH_PHASEID",
            table: "TCMD_RP_RAISON_PHASES",
            column: "TCMD_PH_PHASEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_SP_SUIVI_PHASES_TCMD_COMMANDEID",
            table: "TCMD_SP_SUIVI_PHASES",
            column: "TCMD_COMMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASE_APRESID",
            table: "TCMD_SP_SUIVI_PHASES",
            column: "TCMD_PH_PHASE_APRESID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASE_AVANTID",
            table: "TCMD_SP_SUIVI_PHASES",
            column: "TCMD_PH_PHASE_AVANTID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_SP_SUIVI_PHASES_TRU_AUTEUR_MODIFID",
            table: "TCMD_SP_SUIVI_PHASES",
            column: "TRU_AUTEUR_MODIFID");

        migrationBuilder.CreateIndex(
            name: "IX_TCMD_TD_TYPE_DOCUMENTS",
            table: "TCMD_TD_TYPE_DOCUMENTS",
            column: "TCMD_TD_TYPE",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TDC_DEMANDES_COMMANDES_TCMD_COMMANDEID",
            table: "TDC_DEMANDES_COMMANDES",
            column: "TCMD_COMMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TDC_DEMANDES_COMMANDES_TD_DEMANDEID",
            table: "TDC_DEMANDES_COMMANDES",
            column: "TD_DEMANDEID");

        /*FORCE CAMEL case */
        migrationBuilder.Sql(@"UPDATE TEM_ETAT_MASTERS set TEM_NOM_ETAT_MASTER   = upper(substr( trim(TEM_NOM_ETAT_MASTER), 1, 1)) || lower(substr(trim(TEM_NOM_ETAT_MASTER), 2));");

        migrationBuilder.Sql(@"UPDATE TE_ETATS set TE_NOM_ETAT = upper(substr( trim(TE_NOM_ETAT), 1, 1)) || lower(substr(trim(TE_NOM_ETAT), 2));");

        migrationBuilder.Sql(@"UPDATE TS_SCENARIOS set TS_NOM_SCENARIO  = upper(substr( trim(TS_NOM_SCENARIO), 1, 1)) || lower(substr(trim(TS_NOM_SCENARIO), 2));");


        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VPE_PLANIF_ENTETES;");

        migrationBuilder.Sql(@"CREATE VIEW IF NOT EXISTS 
            VPE_PLANIF_ENTETES as 
            select 
                tc.TC_NOM CATEGORIE,
                tem.TEM_NOM_ETAT_MASTER ,
                te.TE_NOM_ETAT,
                te.TE_NOM_ETAT || ' (' ||  cast(te.TE_INDICE_REVISION_L1 as text) || '.' || cast(te.TE_INDICE_REVISION_L2 as text) || '.' || cast(te.TE_INDICE_REVISION_L3 as text) || ')'  AS TE_NOM_ETAT_VERSION,
                cast(te.TE_INDICE_REVISION_L1 as text) || '.' || cast(te.TE_INDICE_REVISION_L2 as text) || '.' || cast(te.TE_INDICE_REVISION_L3 as text) AS VERSION,
                te.TE_COMMENTAIRE,
                tsEtat.TRST_INFO STATUT_ETAT_FR,
                tsEtat.TRST_INFO_EN STATUT_ETAT_EN ,
                scenario.TS_NOM_SCENARIO,
                scenario.TS_DESCR , 
	           tuReferent.TRU_NAME || ' ' || tuReferent.TRU_FIRST_NAME as REFERENT,
                tuReferentTech.TRU_NAME || ' ' || tuReferentTech.TRU_FIRST_NAME as REFERENT_TECH,
               scenario.TS_SCENARIOID,
                te.TE_ETATID 
                , case  tem.TRST_STATUTID || te.TRST_STATUTID || scenario.TRST_STATUTID
                when 'AAA' then 'O'
                else 'N'
                end as ELIGIBLE
                from  
                TE_ETATS te
                inner join TRST_STATUTS tsEtat on  tsEtat.TRST_STATUTID = te.TRST_STATUTID 
                inner join TEM_ETAT_MASTERS tem on te.TEM_ETAT_MASTERID  = tem.TEM_ETAT_MASTERID 
                inner join TRU_USERS tuReferent on tuReferent.TRU_USERID = tem.TRU_RESPONSABLE_FONCTIONNELID 
                inner join TC_CATEGORIES tc on tc.TC_CATEGORIEID =tem.TC_CATEGORIEID  
                inner join TS_SCENARIOS scenario on  scenario.TE_ETATID = te.TE_ETATID 
                inner join TRU_USERS tuReferentTech on tuReferentTech.TRU_USERID = tem.TRU_RESPONSABLE_TECHNIQUEID 
                and EXISTS  (select 1 from TD_DEMANDES tdd  where  Tdd.TS_SCENARIOID = scenario.TS_SCENARIOID and tdd.TRST_STATUTID =='MO');");

        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VPD_PLANIF_DETAILS;");

        migrationBuilder.Sql(@"CREATE VIEW IF NOT EXISTS 
                VPD_PLANIF_DETAILS as 
                select  
                tp.TPF_PLANIFID ,
                tp.TPF_CRON,
                tp.TPF_DATE_DEBUT ,
                tp.TPF_DATE_FIN,
                tp.TPF_DEMANDE_ORIGINEID ,
                tp.TPF_PREREQUIS_DELAI_CHK  ,
                tp.TPF_TIMEZONE_INFOID  ,
                tp.TRST_STATUTID PLANIF_STATUTID,
                tp.TRU_DECLARANTID,
                tu.TRU_NAME || ' ' || tu.TRU_FIRST_NAME as DECLARANT,
                tdd.TD_DEMANDEID,
                tdd.TS_SCENARIOID ,
                scenario.TRST_STATUTID SCENARIO_STATUTID,
                tdd.TD_COMMENTAIRE_UTILISATEUR ,
                (select count(*) from TRD_RESSOURCE_DEMANDES trd where trd.TD_DEMANDEID = tdd.TD_DEMANDEID AND trd.TRD_FICHIER_PRESENT ='O')   as NB_RESSOURCES 
                from TPF_PLANIFS tp 
                inner join TD_DEMANDES tdd on tdd.TD_DEMANDEID = tp.TPF_DEMANDE_ORIGINEID 
                inner join TRU_USERS tu on tu.TRU_USERID = tp.TRU_DECLARANTID 
                inner join TS_SCENARIOS scenario on  scenario.TS_SCENARIOID = tdd.TS_SCENARIOID;");

        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDE_DEMANDES_ETENDUES;");


        migrationBuilder.Sql(@"CREATE VIEW VDE_DEMANDES_ETENDUES AS
            select
                tc.TC_NOM CATEGORIE,
                te.TE_NOM_ETAT,
                te.TE_NOM_ETAT || ' (' || cast(te.TE_INDICE_REVISION_L1 as text) || '.' || cast(te.TE_INDICE_REVISION_L2 as text) || '.' || cast(te.TE_INDICE_REVISION_L3 as text) || ')'  AS TE_NOM_ETAT_VERSION,
               cast(te.TE_INDICE_REVISION_L1 as text) || '.' || cast(te.TE_INDICE_REVISION_L2 as text) || '.' || cast(te.TE_INDICE_REVISION_L3 as text) AS VERSION,
               te.TE_COMMENTAIRE,
                tsEtat.TRST_INFO STATUT_ETAT_FR,
                tsEtat.TRST_INFO_EN STATUT_ETAT_EN,
                scenario.TS_NOM_SCENARIO,
                scenario.TS_DESCR ,
                tdd.TD_DEMANDEID,
                tdd.TD_COMMENTAIRE_UTILISATEUR,
                tdd.TD_DATE_DEMANDE,
                tdd.TD_DATE_DERNIER_DOWNLOAD,
                tdd.TD_DATE_EXECUTION_SOUHAITEE,
                tdd.TD_DATE_LIVRAISON,
                tdd.TD_DATE_PRISE_EN_CHARGE,
                ifnull(tdd.TD_DATE_LIVRAISON, tdd.TD_DATE_EXECUTION_SOUHAITEE) AS TD_DATE_PIVOT,
               tdd.TD_DEMANDE_ORIGINEID,
                tdd.TD_DUREE_PRODUCTION_REEL,
                tdd.TD_INFO_RETOUR_TRAITEMENT,
                tdd.TD_QUALIF_BILAN,
                tdd.TD_QUALIF_EXIST_FILE,
                tdd.TD_QUALIF_FILE_SIZE,
                tdd.TD_RESULT_EXIST_FILE,
                tdd.TD_RESULT_FILE_SIZE,
		    tdd.TD_SUSPEND_EXECUTION ,  
		    tdd.TD_IGNORE_RESULT,
                tdd.TD_RESULT_NB_DOWNLOAD,
                tdd.TSRV_SERVEURID,
                serveur.TSRV_NOM,
                tuReferent.TRU_NAME  || ' ' || tuReferent.TRU_FIRST_NAME as REFERENT,
                tuReferentTech.TRU_NAME || ' ' || tuReferentTech.TRU_FIRST_NAME  as REFERENT_TECH,
                tdd.TS_SCENARIOID,
                tdd.TE_ETATID,
                tdd.TPF_PLANIF_ORIGINEID,
                tdd.TRU_DEMANDEURID,
                tu.TRU_NAME || ' ' || tu.TRU_FIRST_NAME as DEMANDEUR,
                tdd.TRST_STATUTID CODE_STATUT_DEMANDE,
                ts.TRST_INFO AS STATUT_DEMANDE_FR,
                ts.TRST_INFO_EN AS STATUT_DEMANDE_EN,
                (select count(*) from TRD_RESSOURCE_DEMANDES trd where trd.TD_DEMANDEID = tdd.TD_DEMANDEID AND trd.TRD_FICHIER_PRESENT = 'O') +
                 (select count(*) from TRD_RESSOURCE_DEMANDES trdorigine where trdorigine.TD_DEMANDEID = tdd.TD_DEMANDE_ORIGINEID  AND trdorigine.TRD_FICHIER_PRESENT = 'O' ) as NB_RESSOURCES,
                tdd.TD_GUID,
                tdd.TRU_GESTIONNAIRE_VALIDEURID,
                tdd.TD_SEND_MAIL_CLIENT,
                tdd.TD_SEND_MAIL_GESTIONNAIRE,
                tdd.TD_COMMENTAIRE_GESTIONNAIRE,
                tdd.TD_DATE_AVIS_GESTIONNAIRE,
                tdd.TD_PREREQUIS_DELAI_CHK,
                TBD_BATCH_DEMANDES.TBD_CODE_RETOUR
                from TD_DEMANDES tdd
                inner join TRST_STATUTS ts on tdd.TRST_STATUTID = ts.TRST_STATUTID
                inner join TE_ETATS te on te.TE_ETATID = tdd.TE_ETATID
                inner join TRST_STATUTS tsEtat on tsEtat.TRST_STATUTID = te.TRST_STATUTID
                inner join TEM_ETAT_MASTERS tem on te.TEM_ETAT_MASTERID = tem.TEM_ETAT_MASTERID
                inner join TRU_USERS tuReferent on tuReferent.TRU_USERID = tem.TRU_RESPONSABLE_FONCTIONNELID
                inner join TC_CATEGORIES tc on tc.TC_CATEGORIEID = tem.TC_CATEGORIEID
                left join TS_SCENARIOS scenario on scenario.TS_SCENARIOID = tdd.TS_SCENARIOID
                inner join TRU_USERS tu on tu.TRU_USERID = tdd.TRU_DEMANDEURID
                inner join TRU_USERS tuReferentTech on tuReferentTech.TRU_USERID = tem.TRU_RESPONSABLE_TECHNIQUEID
                left join TSRV_SERVEURS serveur on serveur.TSRV_SERVEURID = tdd.TSRV_SERVEURID
                left join TPF_PLANIFS tp on tp.TPF_PLANIFID = tdd.TPF_PLANIF_ORIGINEID
                left join TBD_BATCH_DEMANDES ON TBD_BATCH_DEMANDES.TD_DEMANDEID = tdd.TD_DEMANDEID AND TBD_CODE_RETOUR<>0
                where ts.TRST_STATUTID NOT in ('DB', 'MO', 'PA');  ");

        migrationBuilder.Sql(@"INSERT INTO TCMD_MC_MODE_CREATIONS (TCMD_MC_CODE, TCMD_MC_LIB_FR, TCMD_MC_LIB_EN) VALUES('UTD', 'UTD', 'DTU');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_MC_MODE_CREATIONS (TCMD_MC_CODE, TCMD_MC_LIB_FR, TCMD_MC_LIB_EN) VALUES('DOM', 'DOMAINE', 'DOMAINE');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_MC_MODE_CREATIONS (TCMD_MC_CODE, TCMD_MC_LIB_FR, TCMD_MC_LIB_EN) VALUES('CPY', 'DUPLICATION', 'TRAD_DUPLICATION');");

        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('VA', 'Validée', 'TRAD_Validé');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('EC', 'En cours', 'TRAD_En cours');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('RJ', 'Rejetée', 'TRAD_Rejetée');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('GL', 'Gelée', 'TRAD_Gelée');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('LI', 'Livrée', 'TRAD_Livrée');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('TE', 'Terminée', 'TRAD_Terminée');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN) VALUES('EX', 'Expirée', 'TRAD_Expirée');");

        migrationBuilder.Sql(@"INSERT INTO TCMD_RP_RAISON_PHASES (TCMD_RP_LIB_FR, TCMD_RP_LIB_EN, TCMD_PH_PHASEID) select 'Commande incomplète', 'TRAD_Commande incomplète', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE='RJ';");
        migrationBuilder.Sql(@"INSERT INTO TCMD_RP_RAISON_PHASES (TCMD_RP_LIB_FR, TCMD_RP_LIB_EN, TCMD_PH_PHASEID) select 'Autre', 'TRAD_Autre', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE='RJ';");

        migrationBuilder.Sql(@"INSERT INTO TCMD_TD_TYPE_DOCUMENTS (TCMD_TD_TYPE, TCMD_TD_COMMENTAIRE) VALUES('CDC', 'Cdc de production');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_TD_TYPE_DOCUMENTS (TCMD_TD_TYPE, TCMD_TD_COMMENTAIRE) VALUES('NOT', 'Note métier');");
        migrationBuilder.Sql(@"INSERT INTO TCMD_TD_TYPE_DOCUMENTS (TCMD_TD_TYPE, TCMD_TD_COMMENTAIRE) VALUES('PARAM', 'Données de paramètrage');");


    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TCMD_CR_CMD_RAISON_PHASES");

        migrationBuilder.DropTable(
            name: "TCMD_DA_DEMANDES_ASSOCIEES");

        migrationBuilder.DropTable(
            name: "TCMD_DOC_DOCUMENTS");

        migrationBuilder.DropTable(
            name: "TDC_DEMANDES_COMMANDES");

        migrationBuilder.DropTable(
            name: "TCMD_RP_RAISON_PHASES");

        migrationBuilder.DropTable(
            name: "TCMD_SP_SUIVI_PHASES");

        migrationBuilder.DropTable(
            name: "TCMD_TD_TYPE_DOCUMENTS");

        migrationBuilder.DropTable(
            name: "TCMD_COMMANDES");

        migrationBuilder.DropTable(
            name: "TCMD_MC_MODE_CREATIONS");

        migrationBuilder.DropTable(
            name: "TCMD_PH_PHASES");

        migrationBuilder.DropColumn(
            name: "TE_ENV_VIERGE_DATE_DIAG_VALIDE",
            table: "TE_ETATS");

        migrationBuilder.DropColumn(
            name: "TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP",
            table: "TE_ETATS");
    }
}
