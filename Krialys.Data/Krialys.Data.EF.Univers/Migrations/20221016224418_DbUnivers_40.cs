using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_40 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
					 CREATE VIEW IF NOT EXISTS VACCGQ_ACCUEIL_GRAPHE_QUALITES AS
                        select   
                        1 as ID, 
                         ifnull(sum(case (TD_QUALIF_BILAN) when (1) then 1 else 0 end),0) as VERT,
                         ifnull(sum(case (TD_QUALIF_BILAN) when (2) then 1 else 0 end),0) as ORANGE,
                         ifnull(sum(case (TD_QUALIF_BILAN) when (3) then 1 else 0 end),0) as ROUGE
                        from 
                        VDE_DEMANDES_ETENDUES
                        where TD_DATE_PIVOT < DATETIME()
                        and TD_QUALIF_BILAN is not null
                        and JULIANDAY(DATETIME())- JULIANDAY(TD_DATE_PIVOT) <366;
					");

        migrationBuilder.Sql(@"
                    CREATE VIEW IF NOT EXISTS VACCGD_ACCUEIL_GRAPHE_DEMANDES AS
                        select strftime('%Y-%m', TD_DATE_PIVOT) as PERIODE,   
                        count(*) as NB_DEMANDES,
                        ifnull(sum(case (CODE_STATUT_DEMANDE ) when ('VA') then 1  when ('DR') then 1 else 0 end),0) as NB_REUSSITES
                        from 
                        VDE_DEMANDES_ETENDUES
                        where TD_DATE_PIVOT < DATETIME()
                        and JULIANDAY(DATETIME())- JULIANDAY(TD_DATE_PIVOT) <366
                        group by strftime('%Y-%m', TD_DATE_PIVOT) 
                        order by 1 asc;
            ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
