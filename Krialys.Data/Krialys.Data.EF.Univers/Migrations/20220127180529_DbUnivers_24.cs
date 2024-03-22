using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_24 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDE_DEMANDES_RESSOURCES;");

        migrationBuilder.Sql(@"CREATE VIEW IF NOT EXISTS VDE_DEMANDES_RESSOURCES AS
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
                      left join TRD_RESSOURCE_DEMANDES   on TRD_RESSOURCE_DEMANDES.TER_ETAT_RESSOURCEID=TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID And TRD_RESSOURCE_DEMANDES.TD_DEMANDEID = TD_DEMANDES.TD_DEMANDEID 
;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VDE_DEMANDES_RESSOURCES;");
    }
}
