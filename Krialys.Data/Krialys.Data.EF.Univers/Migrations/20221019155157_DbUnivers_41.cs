using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_41 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        migrationBuilder.Sql(@"
					drop view IF EXISTS VACCGQ_ACCUEIL_GRAPHE_QUALITES;
					");

        migrationBuilder.Sql(@"
			CREATE VIEW IF NOT EXISTS VACCGQ_ACCUEIL_GRAPHE_QUALITES AS
                  select 
                    TD_QUALIF_BILAN QUALIFID,
                    case (TD_QUALIF_BILAN) when 1 then 'VERT' when 2 then 'ORANGE' when(3) then 'ROUGE' ELSE 'ROUGE' END QUALIF_FR,
	                case (TD_QUALIF_BILAN) when 1 then 'GREEN' when 2 then 'ORANGE' when(3) then 'RED' ELSE 'RED' END QUALIF_EN,
	                 COUNT(*) QUALIF_NB,
                    round(100 * (COUNT(*) / CAST(SUM(count(*)) over(partition by 1) as float))) RATIO
                    from
                    VDE_DEMANDES_ETENDUES
                    where TD_DATE_PIVOT < DATETIME()
	                and TD_QUALIF_BILAN is not null
	                and TD_DATE_PIVOT > date(date(DATETIME(),'start of month','+1 month','-1 day') , '-12 month')
	                group by TD_QUALIF_BILAN;");




        migrationBuilder.Sql(@"
					drop view IF EXISTS VACCGD_ACCUEIL_GRAPHE_DEMANDES;
					");

        migrationBuilder.Sql(@"
                    CREATE VIEW IF NOT EXISTS VACCGD_ACCUEIL_GRAPHE_DEMANDES AS
                        select strftime('%Y-%m', TD_DATE_PIVOT) as PERIODE,   
                        count(*) as NB_DEMANDES,
                        ifnull(sum(case (CODE_STATUT_DEMANDE ) when ('VA') then 1  when ('DR') then 1 else 0 end),0) as NB_REUSSITES
                        from 
                        VDE_DEMANDES_ETENDUES
                        where TD_DATE_PIVOT < DATETIME()
                        and TD_DATE_PIVOT > date(date(DATETIME(),'start of month','+1 month','-1 day') , '-12 month')
                        group by strftime('%Y-%m', TD_DATE_PIVOT) 
                        order by 1 asc;
            "
        );

        migrationBuilder.Sql(@"
					UPDATE TCMD_PH_PHASES set TCMD_PH_CODE = 'AA', TCMD_PH_LIB_FR = 'A accepter', TCMD_PH_LIB_EN = 'To accept'
                    where TCMD_PH_CODE = 'VA';
					");

        migrationBuilder.Sql(@"
			        INSERT INTO TCMD_PH_PHASES
                    (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN)
                    VALUES('BR', 'Brouillon', 'Draft');
                    ");

        migrationBuilder.Sql(@"update TD_DEMANDES set TRST_STATUTID ='DR' where TRST_STATUTID='VA';
            ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
