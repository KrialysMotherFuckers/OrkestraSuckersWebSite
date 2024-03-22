using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_34 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "TRU_ALLOW_INTERNAL_AUTH",
            table: "TRU_USERS",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "TSG_SCENARIO_GPES",
            columns: table => new
            {
                TSG_SCENARIO_GPEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TSG_NOM = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                TSG_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TSG_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TSG_DATE_MAJ = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TSG_SCENARIO_GPES", x => x.TSG_SCENARIO_GPEID);
                table.ForeignKey(
                    name: "FK_TSG_SCENARIO_GPES$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID");
            });

        migrationBuilder.CreateTable(
            name: "TTE_TEAMS",
            columns: table => new
            {
                TTE_TEAMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TTE_NOM = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                TTE_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TTE_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TTE_DATE_CREATION = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTE_TEAMS", x => x.TTE_TEAMID);
                table.ForeignKey(
                    name: "FK_TTE_TEAMS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID");
            });

        migrationBuilder.CreateTable(
            name: "TSGA_SCENARIO_GPE_ASSOCIES",
            columns: table => new
            {
                TSGA_SCENARIO_GPE_ASSOCIEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TSG_SCENARIO_GPEID = table.Column<int>(type: "INTEGER", nullable: false),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false),
                TSGA_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TSGA_DATE_CREATION = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TSGA_SCENARIO_GPE_ASSOCIES", x => x.TSGA_SCENARIO_GPE_ASSOCIEID);
                table.ForeignKey(
                    name: "FK_TSGA_SCENARIO_GPE_ASSOCIES$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID");
                table.ForeignKey(
                    name: "FK_TSGA_SCENARIO_GPE_ASSOCIES$TS_SCENARIOS",
                    column: x => x.TS_SCENARIOID,
                    principalTable: "TS_SCENARIOS",
                    principalColumn: "TS_SCENARIOID");
                table.ForeignKey(
                    name: "FK_TSGA_SCENARIO_GPE_ASSOCIES$TSG_SCENARIO_GPES",
                    column: x => x.TSG_SCENARIO_GPEID,
                    principalTable: "TSG_SCENARIO_GPES",
                    principalColumn: "TSG_SCENARIO_GPEID");
            });

        migrationBuilder.CreateTable(
            name: "TH_HABILITATIONS",
            columns: table => new
            {
                TH_HABILITATIONID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRU_USERID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TTE_TEAMID = table.Column<int>(type: "INTEGER", nullable: true),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: true),
                TSG_SCENARIO_GPEID = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TH_DROIT_CONCERNE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                TH_EST_HABILITE = table.Column<int>(type: "INTEGER", nullable: false),
                TH_DATE_INITIALISATION = table.Column<string>(type: "TEXT", nullable: false),
                TRU_INITIALISATION_AUTEURID = table.Column<string>(type: "TEXT", nullable: false),
                TH_MAJ_DATE = table.Column<string>(type: "TEXT", nullable: false),
                TRU_MAJ_AUTEURID = table.Column<string>(type: "TEXT", nullable: false),
                TH_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TH_HERITE_HABILITATIONID = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TH_HABILITATIONS", x => x.TH_HABILITATIONID);
                table.ForeignKey(
                    name: "FK_TH_HABILITATIONS_TSG_SCENARIO_GPES_TSG_SCENARIO_GPEID",
                    column: x => x.TSG_SCENARIO_GPEID,
                    principalTable: "TSG_SCENARIO_GPES",
                    principalColumn: "TSG_SCENARIO_GPEID");
                table.ForeignKey(
                    name: "FK_TH_HABILITATIONS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID");
                table.ForeignKey(
                    name: "FK_TH_HABILITATIONS$TRU_USERS$MAJ_AUTEUR",
                    column: x => x.TRU_MAJ_AUTEURID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID");
                table.ForeignKey(
                    name: "FK_TH_HABILITATIONS$TS_SCENARIOS",
                    column: x => x.TS_SCENARIOID,
                    principalTable: "TS_SCENARIOS",
                    principalColumn: "TS_SCENARIOID");
                table.ForeignKey(
                    name: "FK_TH_HABILITATIONS$TTE_TEAMS",
                    column: x => x.TTE_TEAMID,
                    principalTable: "TTE_TEAMS",
                    principalColumn: "TTE_TEAMID");
            });

        migrationBuilder.CreateTable(
            name: "TUTE_USER_TEAMS",
            columns: table => new
            {
                TUTE_USER_TEAMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRU_USERID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TTE_TEAMID = table.Column<int>(type: "INTEGER", nullable: false),
                TUTE_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TUTE_DATE_MAJ = table.Column<DateTime>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TUTE_USER_TEAMS", x => x.TUTE_USER_TEAMID);
                table.ForeignKey(
                    name: "FK_TUTE_USER_TEAMSS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID");
                table.ForeignKey(
                    name: "FK_TUTE_USER_TEAMSS$TRU_USERS",
                    column: x => x.TRU_USERID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID");
                table.ForeignKey(
                    name: "FK_TUTE_USER_TEAMSS$TTE_TEAM",
                    column: x => x.TTE_TEAMID,
                    principalTable: "TTE_TEAMS",
                    principalColumn: "TTE_TEAMID");
            });

        migrationBuilder.CreateIndex(
            name: "IX_TH_HABILITATIONS_TRST_STATUTID",
            table: "TH_HABILITATIONS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TH_HABILITATIONS_TRU_MAJ_AUTEURID",
            table: "TH_HABILITATIONS",
            column: "TRU_MAJ_AUTEURID");

        migrationBuilder.CreateIndex(
            name: "IX_TH_HABILITATIONS_TS_SCENARIOID",
            table: "TH_HABILITATIONS",
            column: "TS_SCENARIOID");

        migrationBuilder.CreateIndex(
            name: "IX_TH_HABILITATIONS_TSG_SCENARIO_GPEID",
            table: "TH_HABILITATIONS",
            column: "TSG_SCENARIO_GPEID");

        migrationBuilder.CreateIndex(
            name: "IX_TH_HABILITATIONS_TTE_TEAMID",
            table: "TH_HABILITATIONS",
            column: "TTE_TEAMID");

        migrationBuilder.CreateIndex(
            name: "TH_HABILITATIONS_IDX1",
            table: "TH_HABILITATIONS",
            columns: new[] { "TRU_USERID", "TS_SCENARIOID" });

        migrationBuilder.CreateIndex(
            name: "IX_TSG_SCENARIO_GPES_TRST_STATUTID",
            table: "TSG_SCENARIO_GPES",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "UQ_TSG_SCENARIO_GPES_TSG_NOM",
            table: "TSG_SCENARIO_GPES",
            column: "TSG_NOM",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TSGA_SCENARIO_GPE_ASSOCIES_TRST_STATUTID",
            table: "TSGA_SCENARIO_GPE_ASSOCIES",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TSGA_SCENARIO_GPE_ASSOCIES_TS_SCENARIOID",
            table: "TSGA_SCENARIO_GPE_ASSOCIES",
            column: "TS_SCENARIOID");

        migrationBuilder.CreateIndex(
            name: "UQ_TSGA_SCENARIO_GPE_ASSOCIES",
            table: "TSGA_SCENARIO_GPE_ASSOCIES",
            columns: new[] { "TSG_SCENARIO_GPEID", "TS_SCENARIOID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TTE_TEAMS_TRST_STATUTID",
            table: "TTE_TEAMS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "UQ_TTE_TEAMS_TTE_NOM",
            table: "TTE_TEAMS",
            column: "TTE_NOM",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TUTE_USER_TEAMS_TRST_STATUTID",
            table: "TUTE_USER_TEAMS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TUTE_USER_TEAMS_TTE_TEAMID",
            table: "TUTE_USER_TEAMS",
            column: "TTE_TEAMID");

        migrationBuilder.CreateIndex(
            name: "UQ_TUTE_USER_TEAMS",
            table: "TUTE_USER_TEAMS",
            columns: new[] { "TRU_USERID", "TTE_TEAMID" },
            unique: true);


        migrationBuilder.Sql(@"CREATE VIEW VDTFH_HABILITATIONS as 
                select iif(SUB_GPE_TEAM.TTE_TEAMID is not null, SUB_GPE_TEAM.TRU_USERID, TH_HABILITATIONS.TRU_USERID)  as TRU_USERID,
                iif(SUB_GPE_SCENARIO.TSG_SCENARIO_GPEID is not null, SUB_GPE_SCENARIO.TS_SCENARIOID, TH_HABILITATIONS.TS_SCENARIOID) as TS_SCENARIOID ,
                max(case when TH_DROIT_CONCERNE ='PRODUCTEUR' then  TH_EST_HABILITE  else 0 end) AS PRODUCTEUR,  
                max(case when TH_DROIT_CONCERNE='CONTREMAITRE' then  TH_EST_HABILITE  else 0 end) AS CONTREMAITRE,
                max(case when TH_DROIT_CONCERNE='CONTROLEUR' then  TH_EST_HABILITE  else 0 end) AS CONTROLEUR
                from TH_HABILITATIONS
                left join (
                select TSGA.TSG_SCENARIO_GPEID , TSGA.TS_SCENARIOID from 
	                TSGA_SCENARIO_GPE_ASSOCIES TSGA
	                inner join TSG_SCENARIO_GPES TSG on TSGA.TSG_SCENARIO_GPEID  = TSG.TSG_SCENARIO_GPEID 
	                inner join TS_SCENARIOS TS on TS.TS_SCENARIOID = TSGA.TS_SCENARIOID   
	                where TSGA.TRST_STATUTID ='A'
	                AND TSG.TRST_STATUTID ='A'
                ) as SUB_GPE_SCENARIO on SUB_GPE_SCENARIO.TSG_SCENARIO_GPEID = TH_HABILITATIONS.TSG_SCENARIO_GPEID
                left join (
	                select TUTE.TTE_TEAMID , TUTE.TRU_USERID from 
	                TTE_TEAMS  TTE inner join  
	                TUTE_USER_TEAMS TUTE on TTE.TTE_TEAMID = TUTE.TTE_TEAMID 
	                where TTE.TRST_STATUTID = 'A'
                ) as SUB_GPE_TEAM on  SUB_GPE_TEAM.TTE_TEAMID = TH_HABILITATIONS.TTE_TEAMID
                where TH_HABILITATIONS.TRST_STATUTID='A'
                AND TH_HABILITATIONS.TH_EST_HABILITE=1
                group by iif(SUB_GPE_TEAM.TTE_TEAMID is not null, SUB_GPE_TEAM.TRU_USERID, TH_HABILITATIONS.TRU_USERID)  ,
                iif(SUB_GPE_SCENARIO.TSG_SCENARIO_GPEID is not null, SUB_GPE_SCENARIO.TS_SCENARIOID, TH_HABILITATIONS.TS_SCENARIOID);");

        migrationBuilder.Sql(
        "update TRU_USERS set TRU_ALLOW_INTERNAL_AUTH = 1;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TH_HABILITATIONS");

        migrationBuilder.DropTable(
            name: "TSGA_SCENARIO_GPE_ASSOCIES");

        migrationBuilder.DropTable(
            name: "TUTE_USER_TEAMS");

        migrationBuilder.DropTable(
            name: "TSG_SCENARIO_GPES");

        migrationBuilder.DropTable(
            name: "TTE_TEAMS");

        migrationBuilder.DropColumn(
            name: "TRU_ALLOW_INTERNAL_AUTH",
            table: "TRU_USERS");
    }
}
