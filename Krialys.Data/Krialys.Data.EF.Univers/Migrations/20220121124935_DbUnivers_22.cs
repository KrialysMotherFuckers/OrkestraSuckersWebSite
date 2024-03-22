using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_22 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDE_DEMANDES_ETENDUES;");

        migrationBuilder.Sql(@"CREATE VIEW IF NOT EXISTS VDE_DEMANDES_ETENDUES AS
            select 
                tc.TC_NOM CATEGORIE,
                te.TE_NOM_ETAT,
             te.TE_NOM_ETAT || ' (' ||  cast(te.TE_INDICE_REVISION_L1 as text) || '.' || cast(te.TE_INDICE_REVISION_L2 as text) || '.' || cast(te.TE_INDICE_REVISION_L3 as text) || ')'  AS TE_NOM_ETAT_VERSION,
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
                tdd.TD_DEMANDE_ORIGINEID,
                tdd.TD_DUREE_PRODUCTION_REEL,
                tdd.TD_INFO_RETOUR_TRAITEMENT,
                tdd.TD_QUALIF_BILAN,
                tdd.TD_QUALIF_EXIST_FILE,
                tdd.TD_QUALIF_FILE_SIZE,
                tdd.TD_RESULT_EXIST_FILE,
                tdd.TD_RESULT_FILE_SIZE,
                tdd.TD_RESULT_NB_DOWNLOAD,
                tdd.TSRV_SERVEURID,
                serveur.TSRV_NOM,
                tuReferent.TRU_NAME || ' ' || tuReferent.TRU_FIRST_NAME as REFERENT,
          tuReferentTech.TRU_NAME || ' ' || tuReferentTech.TRU_FIRST_NAME as REFERENT_TECH,
                tdd.TS_SCENARIOID,
                tdd.TE_ETATID,
                tdd.TPF_PLANIF_ORIGINEID,
                tdd.TRU_DEMANDEURID,
                tu.TRU_NAME || ' ' || tu.TRU_FIRST_NAME as DEMANDEUR,
                tdd.TRST_STATUTID CODE_STATUT_DEMANDE,
                ts.TRST_INFO AS STATUT_DEMANDE_FR,
                ts.TRST_INFO_EN AS STATUT_DEMANDE_EN,
                (select count(*) from TRD_RESSOURCE_DEMANDES trd where trd.TD_DEMANDEID = tdd.TD_DEMANDEID AND trd.TRD_FICHIER_PRESENT ='O') +
                (select count(*) from TRD_RESSOURCE_DEMANDES trdorigine where trdorigine.TD_DEMANDEID = tdd.TD_DEMANDE_ORIGINEID  AND trdorigine.TRD_FICHIER_PRESENT ='O' ) as NB_RESSOURCES,
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
                inner join TRST_STATUTS tsEtat on  tsEtat.TRST_STATUTID = te.TRST_STATUTID 
                inner join TEM_ETAT_MASTERS tem on te.TEM_ETAT_MASTERID  = tem.TEM_ETAT_MASTERID 
                inner join TRU_USERS tuReferent on tuReferent.TRU_USERID = tem.TRU_RESPONSABLE_FONCTIONNELID 
                inner join TC_CATEGORIES tc on tc.TC_CATEGORIEID =tem.TC_CATEGORIEID  
                inner join TS_SCENARIOS scenario on  scenario.TS_SCENARIOID = tdd.TS_SCENARIOID 
                inner join TRU_USERS tu on tu.TRU_USERID = tdd.TRU_DEMANDEURID 
                inner join TRU_USERS tuReferentTech on tuReferentTech.TRU_USERID = tem.TRU_RESPONSABLE_TECHNIQUEID 
                left join TSRV_SERVEURS serveur on serveur.TSRV_SERVEURID = tdd.TSRV_SERVEURID 
                left join TPF_PLANIFS tp on tp.TPF_PLANIFID = tdd.TPF_PLANIF_ORIGINEID
                left join TBD_BATCH_DEMANDES ON TBD_BATCH_DEMANDES.TD_DEMANDEID = tdd.TD_DEMANDEID AND TBD_CODE_RETOUR <>0 
				where ts.TRST_STATUTID  NOT in ('DB', 'MO')
                ;"
            );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDE_DEMANDES_ETENDUES;");
    }
}
