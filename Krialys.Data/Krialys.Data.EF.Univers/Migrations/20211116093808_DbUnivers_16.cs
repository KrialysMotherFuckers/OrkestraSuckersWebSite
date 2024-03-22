using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_16 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("update TBD_BATCH_DEMANDES set TBD_DATE_DEBUT_EXECUTION=substr(TBD_DATE_DEBUT_EXECUTION ,1,19) where  TBD_DATE_DEBUT_EXECUTION is not null;");
        migrationBuilder.Sql("update TBD_BATCH_DEMANDES  set TBD_DATE_FIN_EXECUTION=substr(TBD_DATE_FIN_EXECUTION,1,19) where  TBD_DATE_FIN_EXECUTION is not null;");
        migrationBuilder.Sql("update TC_CATEGORIES  set TC_DATE_CREATION=substr(TC_DATE_CREATION,1,19) where TC_DATE_CREATION  is not null;");
        migrationBuilder.Sql("update TDQ_DEMANDE_QUALIFS set TDQ_DATE_PROD =substr(TDQ_DATE_PROD,1,19) where TDQ_DATE_PROD   is not null;");

        migrationBuilder.Sql("update TD_DEMANDES  set TD_DATE_DEMANDE =substr(TD_DATE_DEMANDE,1,19) where TD_DATE_DEMANDE   is not null;");
        migrationBuilder.Sql("update TD_DEMANDES  set TD_DATE_EXECUTION_SOUHAITEE =substr(TD_DATE_EXECUTION_SOUHAITEE,1,19) where TD_DATE_EXECUTION_SOUHAITEE   is not null;");
        migrationBuilder.Sql("update TD_DEMANDES  set TD_DATE_PRISE_EN_CHARGE =substr(TD_DATE_PRISE_EN_CHARGE,1,19) where TD_DATE_PRISE_EN_CHARGE   is not null;");
        migrationBuilder.Sql("update TD_DEMANDES  set TD_DATE_LIVRAISON =substr(TD_DATE_LIVRAISON,1,19) where TD_DATE_LIVRAISON   is not null;");
        migrationBuilder.Sql("update TD_DEMANDES  set TD_DATE_AVIS_GESTIONNAIRE =substr(TD_DATE_AVIS_GESTIONNAIRE,1,19) where TD_DATE_AVIS_GESTIONNAIRE    is not null;");
        migrationBuilder.Sql("update TD_DEMANDES  set TD_DATE_DERNIER_DOWNLOAD =substr(TD_DATE_DERNIER_DOWNLOAD,1,19) where TD_DATE_DERNIER_DOWNLOAD   is not null;");

        migrationBuilder.Sql("update TEB_ETAT_BATCHS  set TEB_DATE_CREATION =substr(TEB_DATE_CREATION,1,19) where  TEB_DATE_CREATION  is not null;");

        migrationBuilder.Sql("update TEMF_ETAT_MASTER_FERMES set TEMF_DATE_AJOUT =substr(TEMF_DATE_AJOUT,1,19) where TEMF_DATE_AJOUT   is not null;");
        migrationBuilder.Sql("update TEMF_ETAT_MASTER_FERMES  set  TEMF_DATE_SUPPRESSION =substr(TEMF_DATE_SUPPRESSION,1,19) where TEMF_DATE_SUPPRESSION   is not null;");

        migrationBuilder.Sql("update TEM_ETAT_MASTERS  set TEM_DATE_CREATION =substr(TEM_DATE_CREATION,1,19) where TEM_DATE_CREATION   is not null;");

        migrationBuilder.Sql("update TEP_ETAT_PREREQUISS  set TEP_DATE_MAJ =substr(TEP_DATE_MAJ,1,19) where TEP_DATE_MAJ   is not null;");
        migrationBuilder.Sql("update TER_ETAT_RESSOURCES  set TER_MODELE_DATE =substr(TER_MODELE_DATE,1,19) where TER_MODELE_DATE   is not null;");


        migrationBuilder.Sql("update TE_ETATS set  TE_DATE_REVISION =substr(TE_DATE_REVISION,1,19) where TE_DATE_REVISION  is not null;");
        migrationBuilder.Sql("update TE_ETATS set  TE_DATE_DERNIERE_PRODUCTION =substr(TE_DATE_DERNIERE_PRODUCTION,1,19) where TE_DATE_DERNIERE_PRODUCTION  is not null;");

        migrationBuilder.Sql("update TL_LOGICIELS  set TL_DATE_LOGICIEL =substr(TL_DATE_LOGICIEL,1,19) where  TL_DATE_LOGICIEL  is not null;");

        migrationBuilder.Sql("update TPD_PREREQUIS_DEMANDES  set TPD_DATE_VALIDATION =substr(TPD_DATE_VALIDATION,1,19) where  TPD_DATE_VALIDATION  is not null;");
        migrationBuilder.Sql("update TPD_PREREQUIS_DEMANDES  set TPD_DATE_LAST_CHECK =substr(TPD_DATE_LAST_CHECK,1,19) where  TPD_DATE_LAST_CHECK  is not null;");

        migrationBuilder.Sql("update TPF_PLANIFS  set TPF_DATE_DEBUT =substr(TPF_DATE_DEBUT,1,19) where  TPF_DATE_DEBUT  is not null;");
        migrationBuilder.Sql("update TPF_PLANIFS  set TPF_DATE_FIN =substr(TPF_DATE_FIN,1,19) where TPF_DATE_FIN   is not null;");

        migrationBuilder.Sql("update TPUF_PARALLELEU_FERMES  set TPUF_DATE_MODIFICATION =substr(TPUF_DATE_MODIFICATION,1,19) where TPUF_DATE_MODIFICATION   is not null;");

        migrationBuilder.Sql("update TPU_PARALLELEUS  set TPU_DATE_PREMIERE_ACTIVITE =substr(TPU_DATE_PREMIERE_ACTIVITE,1,19) where TPU_DATE_PREMIERE_ACTIVITE   is not null;");
        migrationBuilder.Sql("update TPU_PARALLELEUS  set TPU_DATE_DERNIERE_ACTIVITE =substr(TPU_DATE_DERNIERE_ACTIVITE,1,19) where TPU_DATE_DERNIERE_ACTIVITE   is not null;");

        migrationBuilder.Sql("update TP_PERIMETRES  set TP_DATE_CREATION =substr(TP_DATE_CREATION,1,19) where TP_DATE_CREATION  is not null;");

        migrationBuilder.Sql("update TSRV_SERVEURS  set TSRV_DATE_ACTIVATION =substr(TSRV_DATE_ACTIVATION,1,19) where TSRV_DATE_ACTIVATION   is not null;");
        migrationBuilder.Sql("update TSRV_SERVEURS  set TSRV_DATE_DERNIERE_ACTIVITE =substr(TSRV_DATE_DERNIERE_ACTIVITE,1,19) where  TSRV_DATE_DERNIERE_ACTIVITE  is not null;");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
