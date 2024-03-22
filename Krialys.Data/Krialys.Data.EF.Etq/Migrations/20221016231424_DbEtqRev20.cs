using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbEtqRev20 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"CREATE VIEW IF NOT EXISTS VACCGET_ACCUEIL_GRAPHE_ETQS AS
                select 
                strftime('%Y-%m', TETQ_DATE_CREATION) as PERIODE, 
                count(*) as NB_ETQ
                from 
                TETQ_ETIQUETTES
                where TETQ_DATE_CREATION < DATETIME()
                and JULIANDAY(DATETIME())- JULIANDAY(TETQ_DATE_CREATION) <366
                group by strftime('%Y-%m', TETQ_DATE_CREATION) 
                order by 1 asc;");

        #region Insert Default values

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TACT_ACTIONS
                    (TACT_CODE, TACT_LIB, TACT_DESC)
                VALUES
                    ('DDE_QUALIF',      'Demande de qualification',     'Lorsqu une étiquette nécessite d être qualifiée(exemple approuvée, rejetée) elle est associée à une demande de qualification.'),
                    ('SUIVI_QUALIF',    'Suivi de qualification',       'Lorsqu une étiquette vient d être qualifiée elle est signalée dans le suivi des qualifications.'),
                    ('SUIVI_ECHEANCE',  'Suivi des écheances',          'Lorsqu une étiquette arrive à échéance, elle est signalée dans le suivi des échéances.'),
                    ('PERIODE_EXPIREE', 'Suivi des périodes expirées',  'Lorsqu une période définie sur une étiquette est dépassée, elle est signalée comme étant expirée.');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TEQC_ETQ_CODIFS
                    (TEQC_CODE, TEQC_CODE_PRC_ORDRE, TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE, TEQC_CODE_PRM_ORDRE, TEQC_INCREMENT_ORDRE, TEQC_INCREMENT_TAILLE, TEQC_INCREMENT_VAL_INIT, TEQC_SEPARATEUR)
                VALUES
                    ('PRM_PRC_OBJ_XX', 2, 3, 1, 4, 2, 1, '_');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TOBF_OBJ_FORMATS
                    (TOBF_CODE, TOBF_LIB, TOBF_DESC)
                VALUES
                    ('DATA', 'Données brutes',  'L objet est sous forme de données brutes.'),
                    ('REST', 'Restitution',     'L objet est sous forme de restitution (tableaux, graphiques).'),
                    ('CUBE', 'Cube',            'L objet est sous la forme d un cube');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TOBN_OBJ_NATURES
                    (TOBN_CODE, TOBN_LIB, TOBN_DESC)
                VALUES
                    ('PARAM',   'Paramétrage',  'L objet correspond à un paramétrage'),
                    ('ENTITE',  'Entité',       'L objet est une entité de référence'),
                    ('GEST',    'Gestion',      'L objet est une entité de gestion'),
                    ('PRODUCT', 'Production',   'L objet est une production'),
                    ('RAPPORT', 'Rapport',      'L objet est un rapport');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRGL_REGLES
                    (TRGL_CODE_REGLE, TRGL_LIB_REGLE, TRGL_DESC_REGLE, TRGL_LIMITE_TEMPS)
                VALUES
                    ('ARCH',        'Archivage',                'La donnée doit être archivée à partir de l échéance indiquée',     'ECHEANCE'),
                    ('SUPP',        'Suppression',              'La donnée doit être supprimée à partir de l échéance indiquée',    'ECHEANCE'),
                    ('CERTIF',      'Certification',            'La donnée doit être certifiée',                                    'DUREE'),
                    ('APPRO',       'Approbation',              'La donnée doit être approuvée',                                    'DUREE'),
                    ('CONFID',      'Confidentialité',          'Définition d un niveau de confidentialité',                        'AUCUNE'),
                    ('DIFF_ETAT',   'Etat diffusio',            'Définition de l état de diffusion',                                'PERIODE'),
                    ('DIFF_PERIM',  'Périmètre de diffusion',   'Définition du périmètre de diffusion',                             'AUCUNE');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TTE_TYPE_EVENEMENTS
                    (TTE_CODE, TTE_LIB, TTE_DESC)
                VALUES
                    ('ETQ_CREATION',    'Création étiquette',           null),
                    ('ETQ_DATA_MAJ',    'MAJ données étiquette',        null),
                    ('ETQ_REGLE_MAJ',   'MAJ qualification étiquette',  null),
                    ('ETQ_STOCKAGE',    'Stockage étiquette',           null),
                    ('ETQ_APPRO_MAJ',   'Maj du statut d approbation',  null);
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TTR_TYPE_RESSOURCES
                    (TTR_TYPE_ENTREE, TTR_TYPE_ENTREE_LIB, TTR_TYPE_ENTREE_DESC)
                VALUES
                    ('ETIQUETTE', 'ETIQUETTE', 'ETIQUETTE');
            ");

        migrationBuilder.Sql(@"
                -- e-TAQ>Jeux de données
                update TETQ_ETIQUETTES
	                set TETQ_DATE_CREATION = SUBSTRING(tempo.TETQ_DATE_CREATION, 1, 17) || '00'
	                from (select TETQ_ETIQUETTEID, TETQ_DATE_CREATION from TETQ_ETIQUETTES WHERE TETQ_DATE_CREATION is not null) as tempo
	                where TETQ_ETIQUETTES.TETQ_ETIQUETTEID = tempo.TETQ_ETIQUETTEID;

                -- e-TAQ-MANAGER>Gérer les objets étiquettés>périmètres
                update TPRCP_PRC_PERIMETRES
	                set TPRCP_DATE_CREATION = SUBSTRING(tempo.TPRCP_DATE_CREATION, 1, 17) || '00'
	                from (select TPRCP_PRC_PERIMETREID, TPRCP_DATE_CREATION from TPRCP_PRC_PERIMETRES WHERE TPRCP_DATE_CREATION is not null) as tempo
	                where TPRCP_PRC_PERIMETRES.TPRCP_PRC_PERIMETREID = tempo.TPRCP_PRC_PERIMETREID;
            ");

        #endregion

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
