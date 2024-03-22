using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbEtqRev23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                /* reprise du contenu du mail du 29/06  ( appliqué sur demo )*/
                update TTE_TYPE_EVENEMENTS set TTE_TYPE_EVENEMENTID = 1 where TTE_CODE='ETQ_CREATION'  and TTE_TYPE_EVENEMENTID <>1;
                update TTE_TYPE_EVENEMENTS set TTE_TYPE_EVENEMENTID = 2 where TTE_CODE='ETQ_DATA_MAJ'  and TTE_TYPE_EVENEMENTID <>2;
                update TTE_TYPE_EVENEMENTS set TTE_TYPE_EVENEMENTID = 3 where TTE_CODE='ETQ_REGLE_MAJ' and TTE_TYPE_EVENEMENTID <>3;
                update TTE_TYPE_EVENEMENTS set TTE_TYPE_EVENEMENTID = 4 where TTE_CODE='ETQ_STOCKAGE'  and TTE_TYPE_EVENEMENTID <>4;
                update TTE_TYPE_EVENEMENTS set TTE_TYPE_EVENEMENTID = 5 where TTE_CODE='ETQ_APPRO_MAJ' and TTE_TYPE_EVENEMENTID <>5;

                /* realignement des ID des tables actions et regles  
                pour simplifier la reprise des données de TRGLRV_REGLES_VALEURS qui a besoin de FK de ces 2 tables */
                UPDATE TACT_ACTIONS set TACT_ACTIONID=1  where TACT_CODE= 'DDE_QUALIF' and  TACT_ACTIONID<>1;
                UPDATE TACT_ACTIONS set TACT_ACTIONID=2  where TACT_CODE= 'SUIVI_QUALIF' and  TACT_ACTIONID<>2;
                UPDATE TACT_ACTIONS set TACT_ACTIONID=3  where TACT_CODE= 'SUIVI_ECHEANCE' and  TACT_ACTIONID<>3;
                UPDATE TACT_ACTIONS set TACT_ACTIONID=4  where TACT_CODE= 'PERIODE_EXPIREE' and  TACT_ACTIONID<>4;

                UPDATE TRGL_REGLES set  TRGL_REGLEID=1 where  TRGL_CODE_REGLE ='ARCH' and TRGL_REGLEID<>1;
                UPDATE TRGL_REGLES set  TRGL_REGLEID=2 where  TRGL_CODE_REGLE ='SUPP' and TRGL_REGLEID<>2;
                UPDATE TRGL_REGLES set  TRGL_REGLEID=3 where  TRGL_CODE_REGLE ='CERTIF' and TRGL_REGLEID<>3;
                UPDATE TRGL_REGLES set  TRGL_REGLEID=4 where  TRGL_CODE_REGLE ='APPRO' and TRGL_REGLEID<>4;
                UPDATE TRGL_REGLES set  TRGL_REGLEID=5 where  TRGL_CODE_REGLE ='CONFID' and TRGL_REGLEID<>5;
                UPDATE TRGL_REGLES set  TRGL_REGLEID=6 where  TRGL_CODE_REGLE ='DIFF_ETAT' and TRGL_REGLEID<>6;
                UPDATE TRGL_REGLES set  TRGL_REGLEID=7, TRGL_LIB_REGLE='Etat diffusion' where  TRGL_CODE_REGLE ='DIFF_PERIM' and TRGL_REGLEID<>7;

                INSERT OR REPLACE INTO TRGLRV_REGLES_VALEURS
                    (TRGLRV_REGLES_VALEURID, TACT_ACTIONID, TRGLRV_DEPART_LIMITE_TEMPS, TRGLRV_ORDRE_AFFICHAGE, TRGLRV_VALEUR, TRGLRV_VALEUR_DEFAUT, TRGLRV_VALEUR_ECHEANCE, TRGL_REGLEID)
                VALUES 
                    (1,null,'O',1,'Échéance non atteinte','F','N',1),
                    (2,3,'N',2,'A archiver','N','O',1),
                    (3,null,'N',3,'Archivé','N','N',1),
                    (4,null,'O',1,'Échéance non atteinte','O','N',2),
                    (5,3,'N',2,'A supprimer','N','O',2),
                    (6,null,'N',3,'Supprimé','N','N',2),
                    (7,1,'N',1,'En attente d''approbation','O','N',4),
                    (8,2,'O',2,'Approuvée','N','N',4),
                    (9,2,'N',3,'Rejeté','N','N',4),
                    (10,4,'N',4,'Périmé','N','O',4),
                    (11,1,'N',1,'En attente de certification','O','N',3),
                    (12,2,'O',2,'Certifiée','N','N',3),
                    (13,2,'N',3,'Non certifiée','N','N',3),
                    (14,4,'N',4,'Certification périmée','N','O',3),
                    (15,null,'N',1,'1','O','N',5),
                    (16,null,'N',2,'2','N','N',5),
                    (17,null,'N',3,'3','N','N',5),
                    (18,null,'N',4,'4','N','N',5),
                    (19,null,'N',1,'Non diffusable','O','N',6),
                    (20,null,'N',2,'A diffuser','N','N',6),
                    (21,2,'O',3,'Diffusé','N','N',6),
                    (22,4,'N',4,'Fin de diffusion','N','O',6),
                    (23,null,'N',1,'Interne','O','N',7),
                    (24,null,'N',2,'Externe','N','N',7),
                    (25,null,'N',3,'Mixte','N','N',7);

                INSERT OR REPLACE INTO TRGLI_REGLES_LIEES
                    (TRGL_REGLELIEEID, TRGLRV_REGLES_VALEURID, TRGLRV_REGLES_VALEURLIEEID)
                VALUES 
                    (1,9,19), 
                    (2,9,13),
                    (3,10,14);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
