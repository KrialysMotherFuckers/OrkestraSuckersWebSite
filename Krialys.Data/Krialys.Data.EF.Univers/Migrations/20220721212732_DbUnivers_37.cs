using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_37 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDTFH_HABILITATIONS;");

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
	                where TTE.TRST_STATUTID = 'A' and 
                    TUTE.TRST_STATUTID ='A'
                ) as SUB_GPE_TEAM on  SUB_GPE_TEAM.TTE_TEAMID = TH_HABILITATIONS.TTE_TEAMID
                where TH_HABILITATIONS.TRST_STATUTID='A'
                AND TH_HABILITATIONS.TH_EST_HABILITE=1
                group by iif(SUB_GPE_TEAM.TTE_TEAMID is not null, SUB_GPE_TEAM.TRU_USERID, TH_HABILITATIONS.TRU_USERID)  ,
                iif(SUB_GPE_SCENARIO.TSG_SCENARIO_GPEID is not null, SUB_GPE_SCENARIO.TS_SCENARIOID, TH_HABILITATIONS.TS_SCENARIOID);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
