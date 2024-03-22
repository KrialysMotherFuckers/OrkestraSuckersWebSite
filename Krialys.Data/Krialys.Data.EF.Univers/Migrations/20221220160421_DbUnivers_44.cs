using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers44 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        /* FIX car non pris en compte dans une migration précédente */
        migrationBuilder.Sql(@"UPDATE TEM_ETAT_MASTERS set TEM_NOM_ETAT_MASTER=upper(substr( trim(TEM_NOM_ETAT_MASTER), 1, 1)) || lower(substr(trim(TEM_NOM_ETAT_MASTER), 2));");

        migrationBuilder.Sql(@"UPDATE TE_ETATS set TE_NOM_ETAT = upper(substr(trim(TE_NOM_ETAT), 1, 1)) || lower(substr(trim(TE_NOM_ETAT), 2));");

        migrationBuilder.Sql(@"UPDATE TS_SCENARIOS set TS_NOM_SCENARIO = upper(substr(trim(TS_NOM_SCENARIO), 1, 1)) || lower(substr(trim(TS_NOM_SCENARIO), 2));");

        migrationBuilder.Sql(@"UPDATE TC_CATEGORIES set TC_NOM = upper(substr(trim(TC_NOM), 1, 1)) || lower(substr(trim(TC_NOM), 2));");

        migrationBuilder.Sql(@"DELETE FROM TMT_MAIL_TEMPLATES;");



        migrationBuilder.Sql(@"UPDATE TRLG_LNGS SET TRLG_LNGID='fr-FR' where  TRLG_LNGID='FR';");
        migrationBuilder.Sql(@"UPDATE TRLG_LNGS SET TRLG_LNGID='en-US' where  TRLG_LNGID='EN';");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRST_STATUTS
                    (TRST_STATUTID, TRST_INFO, TRST_INFO_EN, TRST_REGLE01, TRST_REGLE02, TRST_REGLE03, TRST_REGLE04, TRST_REGLE05, TRST_REGLE06, TRST_REGLE07, TRST_REGLE08, TRST_REGLE09, TRST_REGLE10, TRST_DESCR)
                VALUES
                    ('A', 'Actif', 'Available', 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, '');
            ");


        migrationBuilder.Sql(@"INSERT OR REPLACE INTO TMT_MAIL_TEMPLATES (TMT_CODE,TMT_CODEN2,TMT_LIB,TRST_STATUTID,TRLG_LNGID,TMT_OBJET,TMT_CORPS,TMT_PIED,TMT_COMMENTAIRE,TMT_IMPORTANCE,TMT_DATE_CREATION) VALUES (
'FIN_PROD',NULL,'Fin de production','A','fr-FR',
'Production de la demande n°[TD_DEMANDEID] - [STATUT_DEMANDE_FR]',
'<p>Bonjour,<br /><br />La production de l&apos;UTD [TE_NOM_ETAT_VERSION], module [TS_NOM_SCENARIO] est termin&eacute;e.<br /><br /><b>Statut</b> : [STATUT_DEMANDE_FR]<br /><br /><b>Dur&eacute;e du traitement</b> : [TD_DUREE_PRODUCTION_REEL]<br /><br />[TABLEAU_FEUX_QUALIF]</p>',
'<p>Ce message est envoy&eacute; par un traitement automatique, merci de ne pas r&eacute;pondre &agrave; ce mail.</p><br />[LOGO_ORKESTRA]',
'Mail pour notifier au producteur la fin de sa production','N','2022-12-20 09:30:00');");


        migrationBuilder.Sql(@"INSERT OR REPLACE INTO TMT_MAIL_TEMPLATES (TMT_CODE,TMT_CODEN2,TMT_LIB,TRST_STATUTID,TRLG_LNGID,TMT_OBJET,TMT_CORPS,TMT_PIED,TMT_COMMENTAIRE,TMT_IMPORTANCE,TMT_DATE_CREATION) VALUES (
'FIN_PROD',NULL,'End of production','A','en-US',
'Demand request n°[TD_DEMANDEID] -  [STATUT_DEMANDE_EN]',
'<p>Hello,<br /><br />The production of the DPU [TE_NOM_ETAT_VERSION], module [TS_NOM_SCENARIO] has ended.<br /><br /><b>Status</b>: [STATUT_DEMANDE_EN]<br /><br /><b>Processing duration</b>: [TD_DUREE_PRODUCTION_REEL]<br /><br />[TABLEAU_FEUX_QUALIF]</p>',
'<p>This message is sent by an automatic processing, please do not reply to this email.</p><br />[LOGO_ORKESTRA]',
'Email to notify the produceur about the end of his production','N','2022-12-20 09:30:00');");


        migrationBuilder.Sql(@"INSERT OR REPLACE INTO TMT_MAIL_TEMPLATES (TMT_CODE,TMT_CODEN2,TMT_LIB,TRST_STATUTID,TRLG_LNGID,TMT_OBJET,TMT_CORPS,TMT_PIED,TMT_COMMENTAIRE,TMT_IMPORTANCE,TMT_DATE_CREATION) VALUES (
'ANNUL_COM',NULL,'Annulation d''une commande','A','fr-FR','Annulation de la commande n°[TCMD_COMMANDEID]',
'<p>Bonjour,<br /><br />La commande n&deg;[TCMD_COMMANDEID] qui vous est assign&eacute;e, a &eacute;t&eacute; annul&eacute;e le [TMCD_SP_DATE_MODIF] &agrave; [TMCD_SP_DATE_MODIF] par [TRU_AUTEUR_MODIFID].<br /><br />Motif : [TMCD_SP_COMMENTAIRE].<br /><br />La commande a &eacute;t&eacute; pass&eacute;e automatiquement au statut &quot;Annul&eacute;e&quot;.</p>',
'<p>Ce message est envoy&eacute; par un traitement automatique, merci de ne pas r&eacute;pondre &agrave; ce mail.</p><br />[LOGO_ORKESTRA]',
'Mail pour notifier l''exploitant que la commanditaire a annulé la commande','H','2022-12-20 09:30:00');");


        migrationBuilder.Sql(@"INSERT OR REPLACE INTO TMT_MAIL_TEMPLATES (TMT_CODE,TMT_CODEN2,TMT_LIB,TRST_STATUTID,TRLG_LNGID,TMT_OBJET,TMT_CORPS,TMT_PIED,TMT_COMMENTAIRE,TMT_IMPORTANCE,TMT_DATE_CREATION) VALUES (
'ANNUL_COM',NULL,'Cancellation of an order','A','en-US','Cancellation of the order n°[TCMD_COMMANDEID]',
'<p>Hello,<br />The order n&deg;[TCMD_COMMANDEID] which had been assigned to you, has been cancelled on [TMCD_SP_DATE_MODIF] at [TMCD_SP_DATE_MODIF] by [TRU_AUTEUR_MODIFID].<br />Reason: [TMCD_RP_LIB].<br />Comment: [TMCD_SP_COMMENTAIRE].<br /><br />The order has been automatically qualified as &quot;Cancelled&quot;.</p>',
'<p>This message is sent by an automatic processing, please do not reply to this email.</p><br />[LOGO_ORKESTRA]',
'Email to notify the operator the applicant has canceled the order','H','2022-12-20 09:30:00');");

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
	                group by TD_QUALIF_BILAN;
            ");

        migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS VDE_DEMANDES_ETENDUES AS
                select
                    tc.TC_NOM CATEGORIE,
                    tc.TC_CATEGORIEID,
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
                   /*tdd.TD_SEND_MAIL_CLIENT,
                    tdd.TD_SEND_MAIL_GESTIONNAIRE,*/
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
                    where ts.TRST_STATUTID NOT in ('DB', 'MO', 'PA');
            ");


        migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS VDE_DEMANDES_RESSOURCES AS
                 select 
                     TD_DEMANDES.TE_ETATID, TD_DEMANDES.TD_DEMANDEID, TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID, TER_ETAT_RESSOURCES.TER_NOM_FICHIER, 
                      ifnull(TRD_RESSOURCE_DEMANDES.TRD_NOM_FICHIER_ORIGINAL,'_') as TRD_NOM_FICHIER_ORIGINAL,  
                     TER_ETAT_RESSOURCES.TER_COMMENTAIRE, TER_ETAT_RESSOURCES.TER_MODELE_DOC, 
                     TER_ETAT_RESSOURCES.TER_NOM_MODELE, TRS_RESSOURCE_SCENARIOS.TRS_FICHIER_OBLIGATOIRE, 
                     TER_ETAT_RESSOURCES.TER_IS_PATTERN ,
                        TRD_TAILLE_FICHIER
                 from  
                      TD_DEMANDES  
                      join TRS_RESSOURCE_SCENARIOS       on TRS_RESSOURCE_SCENARIOS.TS_SCENARIOID = TD_DEMANDES.TS_SCENARIOID  
                      join TER_ETAT_RESSOURCES           on TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID = TRS_RESSOURCE_SCENARIOS.TER_ETAT_RESSOURCEID  
                      left join TRD_RESSOURCE_DEMANDES   on TRD_RESSOURCE_DEMANDES.TER_ETAT_RESSOURCEID=TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID And TRD_RESSOURCE_DEMANDES.TD_DEMANDEID = TD_DEMANDES.TD_DEMANDEID;
            ");

        migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS VDTFH_HABILITATIONS as 
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
                    iif(SUB_GPE_SCENARIO.TSG_SCENARIO_GPEID is not null, SUB_GPE_SCENARIO.TS_SCENARIOID, TH_HABILITATIONS.TS_SCENARIOID);
            ");


        migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS VPD_PLANIF_DETAILS as 
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
                    inner join TS_SCENARIOS scenario on  scenario.TS_SCENARIOID = tdd.TS_SCENARIOID;
            ");

        migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS VPE_PLANIF_ENTETES as 
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
                        and EXISTS  (select 1 from TD_DEMANDES tdd  where  Tdd.TS_SCENARIOID = scenario.TS_SCENARIOID and tdd.TRST_STATUTID =='MO');
            ");


        migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS VSCU_CTRL_STRUCTURE_UPLOADS AS
                    SELECT TEL_ETAT_LOGICIEL.TE_ETATID, 
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_ACTION,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILE_TYPE,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_PATH_NAME,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILENAME_PATTERN
                    FROM  
                        TEL_ETAT_LOGICIELS as TEL_ETAT_LOGICIEL
                        join TL_LOGICIELS as TL_LOGICIEL on TEL_ETAT_LOGICIEL.TL_LOGICIELID = TL_LOGICIEL.TL_LOGICIELID
                        join TLE_LOGICIEL_EDITEURS as TLE_LOGICIEL_EDITEUR  on TL_LOGICIEL.TLE_LOGICIEL_EDITEURID = TLE_LOGICIEL_EDITEUR.TLE_LOGICIEL_EDITEURID
                        join TLEM_LOGICIEL_EDITEUR_MODELES as TLEM_LOGICIEL_EDITEUR_MODELE  on TLE_LOGICIEL_EDITEUR.TLE_LOGICIEL_EDITEURID = TLEM_LOGICIEL_EDITEUR_MODELE.TLE_LOGICIEL_EDITEURID
                    group by TEL_ETAT_LOGICIEL.TE_ETATID ,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_ACTION,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILE_TYPE,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_PATH_NAME,
                    TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILENAME_PATTERN;
            ");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
