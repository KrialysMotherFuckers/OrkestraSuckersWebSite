using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers47 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TMT_MAILING_LIST",
            table: "TMT_MAIL_TEMPLATES",
            type: "TEXT",
            nullable: true);

        #region Insert Default values

        migrationBuilder.Sql(@"
                PRAGMA foreign_keys=off;
            ");

        #region TC_CATEGORIES

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TC_CATEGORIES
                    (TC_COMMENTAIRE, TC_DATE_CREATION, TC_NOM, TRST_STATUTID)
                VALUES
                    ('Commentaire catégorie par défaut', DATETIME(), 'Catégorie par defaut', 'A');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TC_CATEGORIES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TC_CATEGORIES WHERE TC_COMMENTAIRE = 'Commentaire catégorie par défaut')
                AND (SELECT COUNT(ROWID) FROM TC_CATEGORIES WHERE TC_COMMENTAIRE = 'Commentaire catégorie par défaut') > 1
                AND TC_COMMENTAIRE = 'Commentaire catégorie par défaut'
            ");

        #endregion TC_CATEGORIES

        #region TCMD_MC_MODE_CREATIONS

        migrationBuilder.Sql(@"
                INSERT INTO TCMD_MC_MODE_CREATIONS
                    (TCMD_MC_CODE, TCMD_MC_LIB_FR, TCMD_MC_LIB_EN)
                VALUES
                    ('UTD', 'UTD', 'DTU'),
                    ('DOM', 'DOMAINE', 'DOMAINE'),
                    ('CPY', 'DUPLICATION', 'TRAD_DUPLICATION');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_MC_MODE_CREATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_MC_MODE_CREATIONS WHERE TCMD_MC_CODE = 'UTD')
                AND (SELECT COUNT(ROWID) FROM TCMD_MC_MODE_CREATIONS WHERE TCMD_MC_CODE = 'UTD') > 1
                AND TCMD_MC_CODE = 'UTD'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_MC_MODE_CREATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_MC_MODE_CREATIONS WHERE TCMD_MC_CODE = 'DOM')
                AND (SELECT COUNT(ROWID) FROM TCMD_MC_MODE_CREATIONS WHERE TCMD_MC_CODE = 'DOM') > 1
                AND TCMD_MC_CODE = 'DOM'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_MC_MODE_CREATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_MC_MODE_CREATIONS WHERE TCMD_MC_CODE = 'CPY')
                AND (SELECT COUNT(ROWID) FROM TCMD_MC_MODE_CREATIONS WHERE TCMD_MC_CODE = 'CPY') > 1
                AND TCMD_MC_CODE = 'CPY'
            ");

        #endregion TCMD_MC_MODE_CREATIONS

        #region TCMD_PH_PHASES

        migrationBuilder.Sql(@"
                INSERT INTO TCMD_PH_PHASES
                    (TCMD_PH_CODE, TCMD_PH_LIB_FR, TCMD_PH_LIB_EN)
                VALUES
                    ('AA', 'A accepter', 'To accept'),
                    ('EC', 'En cours', 'TRAD_En cours'),
                    ('RJ', 'Rejetée', 'TRAD_Rejetée'),
                    ('GL', 'Gelée', 'TRAD_Gelée'),
                    ('LI', 'Livrée', 'TRAD_Livrée'),
                    ('TE', 'Terminée', 'TRAD_Terminée'),
                    ('AR', 'Archivée', 'TRAD_Archivée'),
                    ('BR', 'Brouillon', 'Draft'),
                    ('AN', 'Annulée', 'TRAD_Annulée');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AA')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AA') > 1
                AND TCMD_PH_CODE = 'AA'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'EC')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'EC') > 1
                AND TCMD_PH_CODE = 'EC'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ') > 1
                AND TCMD_PH_CODE = 'RJ'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL') > 1
                AND TCMD_PH_CODE = 'GL'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'LI')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'LI') > 1
                AND TCMD_PH_CODE = 'LI'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'TE')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'TE') > 1
                AND TCMD_PH_CODE = 'TE'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AR')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AR') > 1
                AND TCMD_PH_CODE = 'AR'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'BR')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'BR') > 1
                AND TCMD_PH_CODE = 'BR'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_PH_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN')
                AND (SELECT COUNT(ROWID) FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN') > 1
                AND TCMD_PH_CODE = 'AN'
            ");

        #endregion TCMD_PH_PHASES

        #region TCMD_RP_RAISON_PHASES

        migrationBuilder.Sql(@"
                INSERT INTO TCMD_RP_RAISON_PHASES
                    (TCMD_RP_LIB_FR, TCMD_RP_LIB_EN, TCMD_PH_PHASEID)
                VALUES
                    ('Création de la commande', 'TRAD_Création de la commande', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'BR' LIMIT 1) ),
                    ('Commande validée par le client', 'TRAD_Commande validée par le client', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AA' LIMIT 1) ),
                    ('Commande acceptée et assignée', 'TRAD_Commande acceptée et assignée',	(SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'EC' LIMIT 1)),
                    ('Commande incomplète', 'TRAD_Commande incomplète', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)),
                    ('Commande déjà traitée', 'TRAD_Commande déjà traitée', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)),
                    ('Commande non traitable', 'TRAD_Commande non traitable', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)),
                    ('Autre', 'Other', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)),
                    ('En attente de précision du client', 'TRAD_En attente de précision du client', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)),
                    ('En attente de fichier', 'TRAD_En attente de fichier', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)),
                    ('Autre', 'Other', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)),
                    ('Commande livrée', 'TRAD_Commande livrée', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'LI' LIMIT 1)),
                    ('Commande terminée', 'TRAD_Commande terminée', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'TE' LIMIT 1)),
                    ('Commande erronée', 'TRAD_Commande erronée', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)),
                    ('Commande obsolète', 'TRAD_Commande obsolète', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)),
                    ('Autre', 'Other', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)),
                    ('Phase d''origine obsolète', 'TRAD_Phase d''origine obsolète', (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AR' LIMIT 1));
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Création de la commande' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'BR' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Création de la commande' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'BR' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Création de la commande' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'BR' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande validée par le client' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AA' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande validée par le client' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AA' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande validée par le client' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AA' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande acceptée et assignée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'EC' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande acceptée et assignée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'EC' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande acceptée et assignée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'EC' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande incomplète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande incomplète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande incomplète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande déjà traitée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande déjà traitée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande déjà traitée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande non traitable' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande non traitable' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande non traitable' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'RJ' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'En attente de précision du client' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'En attente de précision du client' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'En attente de précision du client' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'En attente de fichier' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'En attente de fichier' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'En attente de fichier' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'GL' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande livrée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'LI' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande livrée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'LI' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande livrée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'LI' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande terminée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'TE' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande terminée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'TE' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande terminée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'TE' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande erronée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande erronée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande erronée' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande obsolète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Commande obsolète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Commande obsolète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Autre' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AN' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_RP_RAISON_PHASES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Phase d''origine obsolète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AR' LIMIT 1))
                AND (SELECT COUNT(ROWID) FROM TCMD_RP_RAISON_PHASES WHERE TCMD_RP_LIB_FR = 'Phase d''origine obsolète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AR' LIMIT 1)) > 1
                AND TCMD_RP_LIB_FR = 'Phase d''origine obsolète' AND TCMD_PH_PHASEID = (SELECT TCMD_PH_PHASEID FROM TCMD_PH_PHASES WHERE TCMD_PH_CODE = 'AR' LIMIT 1)
            ");

        #endregion TCMD_RP_RAISON_PHASES

        #region TCMD_TD_TYPE_DOCUMENTS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TCMD_TD_TYPE_DOCUMENTS
                    (TCMD_TD_TYPE, TCMD_TD_COMMENTAIRE)
                VALUES
                    ('CDC','Cdc de production'),
                    ('NOT','Note métier'),
                    ('PARAM', 'Données de paramètrage');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_TD_TYPE_DOCUMENTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_TD_TYPE_DOCUMENTS WHERE TCMD_TD_TYPE = 'CDC')
                AND (SELECT COUNT(ROWID) FROM TCMD_TD_TYPE_DOCUMENTS WHERE TCMD_TD_TYPE = 'CDC') > 1
                AND TCMD_TD_TYPE = 'CDC'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_TD_TYPE_DOCUMENTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_TD_TYPE_DOCUMENTS WHERE TCMD_TD_TYPE = 'NOT')
                AND (SELECT COUNT(ROWID) FROM TCMD_TD_TYPE_DOCUMENTS WHERE TCMD_TD_TYPE = 'NOT') > 1
                AND TCMD_TD_TYPE = 'NOT'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TCMD_TD_TYPE_DOCUMENTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TCMD_TD_TYPE_DOCUMENTS WHERE TCMD_TD_TYPE = 'PARAM')
                AND (SELECT COUNT(ROWID) FROM TCMD_TD_TYPE_DOCUMENTS WHERE TCMD_TD_TYPE = 'PARAM') > 1
                AND TCMD_TD_TYPE = 'PARAM'
            ");

        #endregion TCMD_TD_TYPE_DOCUMENTS

        #region TLE_LOGICIEL_EDITEURS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TLE_LOGICIEL_EDITEURS
                    (TLE_EDITEUR)
                VALUES
                    ('ALTERYX'),
                    ('TALEND'),
                    ('Microsoft Shell'),
                    ('PENTAHO'),
                    ('IMS Reportive');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLE_LOGICIEL_EDITEURS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'ALTERYX')
                AND (SELECT COUNT(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'ALTERYX') > 1
                AND TLE_EDITEUR = 'ALTERYX'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLE_LOGICIEL_EDITEURS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'TALEND')
                AND (SELECT COUNT(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'TALEND') > 1
                AND TLE_EDITEUR = 'TALEND'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLE_LOGICIEL_EDITEURS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'Microsoft Shell')
                AND (SELECT COUNT(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'Microsoft Shell') > 1
                AND TLE_EDITEUR = 'Microsoft Shell'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLE_LOGICIEL_EDITEURS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'PENTAHO')
                AND (SELECT COUNT(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'PENTAHO') > 1
                AND TLE_EDITEUR = 'PENTAHO'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLE_LOGICIEL_EDITEURS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'IMS Reportive')
                AND (SELECT COUNT(ROWID) FROM TLE_LOGICIEL_EDITEURS WHERE TLE_EDITEUR = 'IMS Reportive') > 1
                AND TLE_EDITEUR = 'IMS Reportive'
            ");

        #endregion TLE_LOGICIEL_EDITEURS

        #region TL_LOGICIELS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TL_LOGICIELS
                    (TL_NOM_LOGICIEL, TL_LOGICIEL_VERSION, TL_DATE_LOGICIEL, TLE_LOGICIEL_EDITEURID)
                VALUES
                    ('ALTERYX', 'xx', DATETIME() , 1),
                    ('TALEND', 'xx', DATETIME() , 2),
                    ('Microsoft Shell', 'xx', DATETIME() , 3),
                    ('PENTAHO', 'xx', DATETIME() , 4),
                    ('REPORTIVE', 'xx', DATETIME() , 5);
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TL_LOGICIELS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'ALTERYX')
                AND (SELECT COUNT(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'ALTERYX') > 1
                AND TL_NOM_LOGICIEL = 'ALTERYX'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TL_LOGICIELS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'TALEND')
                AND (SELECT COUNT(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'TALEND') > 1
                AND TL_NOM_LOGICIEL = 'TALEND'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TL_LOGICIELS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'Microsoft Shell')
                AND (SELECT COUNT(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'Microsoft Shell') > 1
                AND TL_NOM_LOGICIEL = 'Microsoft Shell'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TL_LOGICIELS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'PENTAHO')
                AND (SELECT COUNT(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'PENTAHO') > 1
                AND TL_NOM_LOGICIEL = 'PENTAHO'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TL_LOGICIELS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'REPORTIVE')
                AND (SELECT COUNT(ROWID) FROM TL_LOGICIELS WHERE TL_NOM_LOGICIEL = 'REPORTIVE') > 1
                AND TL_NOM_LOGICIEL = 'REPORTIVE'
            ");

        #endregion TL_LOGICIELS

        #region TLEM_LOGICIEL_EDITEUR_MODELES

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TLEM_LOGICIEL_EDITEUR_MODELES
                    (TLE_LOGICIEL_EDITEURID, TLEM_ACTION, TLEM_FILE_TYPE, TLEM_PATH_NAME, TLEM_FILENAME_PATTERN, TLEM_INFO)
                VALUES
                    (1,'CHK', 'F', '','*.yxmd','verif présence du fichier'),
                    (1, 'DEL', 'F', '','meta.txt', 'suppression de fichier'),
                    (1, 'DEL', 'F', 'qualif', 'qualif.csv', 'suppression de fichier'),
                    (1, 'CHK', 'D', 'documents', '', 'verif présence du répertoire'),
                    (1, 'CHK', 'D', 'batch', '', 'verif présence du répertoire'),
                    (1, 'CHK', 'D', 'output', '', 'verif présence du répertoire'),
                    (4, 'DEL', 'F', '', 'meta.txt', 'suppression de fichier'),
                    (4, 'DEL', 'F', 'qualif', 'qualif.csv', 'suppression de fichier'),
                    (4, 'CHK', 'D', 'documents', '', 'verif présence du répertoire'),
                    (4, 'CHK', 'D', 'batch', '', 'verif présence du répertoire'),
                    (4, 'CHK', 'D', 'output', '', 'verif présence du répertoire'),
                    (4, 'CHK', 'F', '', '*.ktr', 'verif présence de fichier ktr'),
                    (5, 'DEL', 'D', 'cache', '', 'purge contenu si présent'),
                    (5, 'CHK', 'D', 'resources', '', 'verif présence du répertoire'),
                    (5, 'DEL', 'F', '', 'meta.txt', 'suppression de fichier'),
                    (5, 'DEL', 'F', 'qualif', 'qualif.csv', 'suppression de fichier'),
                    (5, 'CHK', 'D', 'batch', '', 'verif présence du répertoire'),
                    (5, 'CHK', 'D', 'output', '', 'verif présence du répertoire'),
                    (5, 'CHK', 'F', '', 'configurations.xml', 'verif présence du fichier'),
                    (1, 'CHK', 'F', 'batch', '*.bat', 'verif présence au moins un bat'),
                    (4, 'CHK', 'F', 'batch', '*.bat', 'verif présence au moins un bat');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'documents')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'documents') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'documents'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif') > 1
                AND TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'documents')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'documents') > 1
                AND TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'documents'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch') > 1
                AND TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output') > 1
                AND TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'cache')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'cache') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'cache'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'resources')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'resources') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'resources'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'DEL' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'qualif'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'batch'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'D' AND TLEM_PATH_NAME = 'output'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = '') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = ''
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 4 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch') > 1
                AND TLE_LOGICIEL_EDITEURID = 1 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TLEM_LOGICIEL_EDITEUR_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch')
                AND (SELECT COUNT(ROWID) FROM TLEM_LOGICIEL_EDITEUR_MODELES WHERE TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch') > 1
                AND TLE_LOGICIEL_EDITEURID = 5 AND TLEM_ACTION = 'CHK' AND TLEM_FILE_TYPE = 'F' AND TLEM_PATH_NAME = 'batch'
            ");


        #endregion TLEM_LOGICIEL_EDITEUR_MODELES

        #region TPM_PARAMS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TPM_PARAMS
                    (TPM_PARAMID, TPM_VALEUR, TPM_INFO)
                VALUES
                    ('Domaine', 'krialys', 'DOMAINE RESEAU'),
                    ('InternalPwdAuthent', 'True', 'True autorise l authentication web Indépendamment de l AD'),
                    ('LdapActif', 'False', 'L authentification des utilisteurs doit elle se faire via  l AD'),
                    ('PathEnvVierge', 'C:\SolutionKdata\ENV\env_vierge\', 'Chemin de stockage des environnement vierge'),
                    ('PathModeleRessource', 'C:\SolutionKdata\ENV\MODRessource\', 'Chemin de stockage des modeles de fichier de ressource'),
                    ('PathQualif', 'C:\SolutionKdata\Qualif\', 'Chemin de stockage des fichier Qualif'),
                    ('PathRessource', 'C:\SolutionKdata\Ressource\', 'Chemin de stockage des fichiers de ressource'),
                    ('PathResult', 'C:\SolutionKdata\Resultat\', 'Chemin de stockage des fichiers résultats'),
                    ('RoleName', 'Lecteur', 'SSAS - NOM DU ROLE -ne pas modifier'),
                    ('ssasServer', 'NA', 'SSAS - NOM DU SERVEUR');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='Domaine')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='Domaine') > 1
                AND TPM_PARAMID = 'Domaine'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='InternalPwdAuthent')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='InternalPwdAuthent') > 1
                AND TPM_PARAMID = 'InternalPwdAuthent'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='LdapActif')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='LdapActif') > 1
                AND TPM_PARAMID = 'LdapActif'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathEnvVierge')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathEnvVierge') > 1
                AND TPM_PARAMID = 'PathEnvVierge'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathModeleRessource')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathModeleRessource') > 1
                AND TPM_PARAMID = 'PathModeleRessource'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathQualif')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathQualif') > 1
                AND TPM_PARAMID = 'PathQualif'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathRessource')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathRessource') > 1
                AND TPM_PARAMID = 'PathRessource'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathResult')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='PathResult') > 1
                AND TPM_PARAMID = 'PathResult'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='RoleName')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='RoleName') > 1
                AND TPM_PARAMID = 'RoleName'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='ssasServer')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='ssasServer') > 1
                AND TPM_PARAMID = 'ssasServer'
            ");

        #endregion TPM_PARAMS

        #region TRAS_AUTH_SCENARIOS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRAS_AUTH_SCENARIOS
                    (TRAS_LABEL, TRAS_DESCRIPTION)
                VALUES
                    ('authorization_code', 'L application cliente se connecte avec ses propres identifiants et les identifiants d’un utilisateur. Elle est capable de naviguer sur des pages web. Ce scénario est utilisé pour les applications web s’exécutant sur un serveur.'),
                    ('password', 'L application cliente se connecte avec ses propres identifiants et les identifiants d’un utilisateur. Ce scénario est réservé pour les applications privées et de confiance qui ne peuvent pas naviguer sur des pages web.'),
                    ('client_credentials', 'L application cliente se connecte avec ses propres identifiants (sans intervention d’un utilisateur). Ce scénario est utilisé pour la communication « machine to machine ».');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRAS_AUTH_SCENARIOS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRAS_AUTH_SCENARIOS WHERE TRAS_LABEL = 'authorization_code')
                AND (SELECT COUNT(ROWID) FROM TRAS_AUTH_SCENARIOS WHERE TRAS_LABEL ='authorization_code') > 1
                AND TRAS_LABEL = 'authorization_code'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRAS_AUTH_SCENARIOS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRAS_AUTH_SCENARIOS WHERE TRAS_LABEL = 'password')
                AND (SELECT COUNT(ROWID) FROM TRAS_AUTH_SCENARIOS WHERE TRAS_LABEL ='password') > 1
                AND TRAS_LABEL = 'password'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRAS_AUTH_SCENARIOS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRAS_AUTH_SCENARIOS WHERE TRAS_LABEL = 'client_credentials')
                AND (SELECT COUNT(ROWID) FROM TRAS_AUTH_SCENARIOS WHERE TRAS_LABEL ='client_credentials') > 1
                AND TRAS_LABEL = 'client_credentials'
            ");

        #endregion TRAS_AUTH_SCENARIOS

        #region TRCL_CLAIMS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCL_CLAIMS
                    (TRCL_CLAIM_NAME, TRCL_CLAIM_DESCRIPTION, TRCLI_MULTIVALUE, TRCL_STATUS)
                VALUES
                    ('Role', 'Rôle', 'F', 'A'),
                    ('TokenLifetime', 'Durée de vie du jeton d authentification en secondes (Remplace la valeur définie dans appsettings.json)', 'F', 'A');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCL_CLAIMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role')
                AND (SELECT COUNT(ROWID) FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME ='Role') > 1
                AND TRCL_CLAIM_NAME = 'Role'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCL_CLAIMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime')
                AND (SELECT COUNT(ROWID) FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME ='TokenLifetime') > 1
                AND TRCL_CLAIM_NAME = 'TokenLifetime'
            ");

        #endregion TRCL_CLAIMS

        #region TRUCL_USERS_CLAIMS

        migrationBuilder.Sql(@"
                CREATE TEMPORARY TABLE temp AS
                SELECT *
                FROM TRUCL_USERS_CLAIMS;

                DROP TABLE TRUCL_USERS_CLAIMS;

                CREATE TABLE ""TRUCL_USERS_CLAIMS"" (
                    ""TRUCL_USER_CLAIMID"" INTEGER NOT NULL CONSTRAINT ""PK_TRUCL_USERS_CLAIMS"" PRIMARY KEY AUTOINCREMENT,
                    ""TRCLI_CLIENTAPPLICATIONID"" INTEGER NOT NULL,
                    ""TRCL_CLAIMID"" INTEGER NOT NULL,
                    ""TRUCL_CLAIM_VALUE"" TEXT NULL,
                    ""TRUCL_DESCRIPTION"" TEXT NOT NULL,
                    ""TRUCL_STATUS"" TEXT NOT NULL,
                    ""TRU_USERID"" TEXT NULL,
                    CONSTRAINT ""FK_TRUCL_USERS_CLAIMS_TRCL_CLAIMS"" FOREIGN KEY (""TRCL_CLAIMID"") REFERENCES ""TRCL_CLAIMS"" (""TRCL_CLAIMID""),
                    CONSTRAINT ""FK_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONS_TRCLI_CLIENTAPPLICATIONID"" FOREIGN KEY (""TRCLI_CLIENTAPPLICATIONID"") REFERENCES ""TRCLI_CLIENTAPPLICATIONS"" (""TRCLI_CLIENTAPPLICATIONID""),
                    CONSTRAINT ""FK_TRUCL_USERS_CLAIMS_TRU_USERS"" FOREIGN KEY (""TRU_USERID"") REFERENCES ""TRU_USERS"" (""TRU_USERID"") ON DELETE RESTRICT
                );

                CREATE INDEX ""IX_TRUCL_USERS_CLAIMS_TRCL_CLAIMID"" ON ""TRUCL_USERS_CLAIMS"" (""TRCL_CLAIMID"");
                CREATE INDEX ""IX_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONID"" ON ""TRUCL_USERS_CLAIMS"" (""TRCLI_CLIENTAPPLICATIONID"");
                CREATE INDEX ""UQ_TRUCL_USERS_CLAIMS"" ON ""TRUCL_USERS_CLAIMS"" (""TRU_USERID"", ""TRCL_CLAIMID"");

                INSERT INTO TRUCL_USERS_CLAIMS
                SELECT *
                FROM temp;

                DROP TABLE temp;

            ", true);

        #endregion TRUCL_USERS_CLAIMS

        #region TRCCL_CATALOG_CLAIMS

        //migrationBuilder.Sql(@"
        //        UPDATE TRCCL_CATALOG_CLAIMS
        //        SET TRCCL_VALUE_LABEL = 'Data-Driven'
        //        WHERE TRCCL_VALUE_LABEL = 'Data Driven'            
        //    ");

        migrationBuilder.Sql(@"
                UPDATE TRCCL_CATALOG_CLAIMS
                SET TRCCL_DESCRIPTION = 'Droits gérés au niveau des Data métier.'
                WHERE TRCCL_DESCRIPTION = 'Droits gérés au niveau des Data métier'            
            ");

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                SELECT 'A', TRCL_CLAIMID,  '1', 'Utilisateur standard, accède en lecture seule.', 'U', 'Visualisateur' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCCL_CATALOG_CLAIMS
                WHERE
                    ROWID NOT IN(SELECT MIN(ROWID) FROM TRCCL_CATALOG_CLAIMS
                                    WHERE TRCCL_DESCRIPTION = 'Utilisateur standard, accède en lecture seule.'
                                    AND TRCCL_VALUE = '1' AND TRCCL_VALUE_LABEL = 'Visualisateur'
                                 )
                AND(SELECT COUNT(ROWID) FROM TRCCL_CATALOG_CLAIMS
                            WHERE TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1)
                            AND TRCCL_VALUE = '1' AND TRCCL_VALUE_LABEL = 'Visualisateur') > 1
                AND TRCCL_STATUS = 'A'
                AND TRCCL_DESCRIPTION = 'Utilisateur standard, accède en lecture seule.'
            ");


        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                SELECT 'A', TRCL_CLAIMID,  '2', 'Utilisateur avancé, accède en lecture et écriture.', 'U', 'Créateur' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCCL_CATALOG_CLAIMS
                WHERE
                    ROWID NOT IN(SELECT MIN(ROWID) FROM TRCCL_CATALOG_CLAIMS
                                    WHERE TRCCL_DESCRIPTION = 'Utilisateur avancé, accède en lecture et écriture.'
                                    AND TRCCL_VALUE = '2' AND TRCCL_VALUE_LABEL = 'Créateur'
                                 )
                AND(SELECT COUNT(ROWID) FROM TRCCL_CATALOG_CLAIMS
                            WHERE TRCCL_DESCRIPTION = 'Utilisateur avancé, accède en lecture et écriture.'
                                    AND TRCCL_VALUE = '2' AND TRCCL_VALUE_LABEL = 'Créateur') > 1
                AND TRCCL_STATUS = 'A'
                AND TRCCL_DESCRIPTION = 'Utilisateur avancé, accède en lecture et écriture.'
            ");

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                SELECT 'A', TRCL_CLAIMID,  '4', 'Droits gérés au niveau des Data métier.', 'U', 'Data Driven' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCCL_CATALOG_CLAIMS
                WHERE
                    ROWID NOT IN(SELECT MIN(ROWID) FROM TRCCL_CATALOG_CLAIMS
                                    WHERE TRCCL_DESCRIPTION = 'Droits gérés au niveau des Data métier.'
                                    AND TRCCL_VALUE = '4' AND TRCCL_VALUE_LABEL = 'Data Driven'
                                 )
                AND(SELECT COUNT(ROWID) FROM TRCCL_CATALOG_CLAIMS
                            WHERE TRCCL_DESCRIPTION = 'Droits gérés au niveau des Data métier.'
                                    AND TRCCL_VALUE = '4' AND TRCCL_VALUE_LABEL = 'Data Driven') > 1
                AND TRCCL_STATUS = 'A'
                AND TRCCL_DESCRIPTION = 'Droits gérés au niveau des Data métier.'
            ");

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                SELECT 'A', TRCL_CLAIMID,  '8', 'Administrateur d''exploitation, accède en lecture.', 'U', 'Admin-Exploit' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCCL_CATALOG_CLAIMS
                WHERE
                    ROWID NOT IN(SELECT MIN(ROWID) FROM TRCCL_CATALOG_CLAIMS
                                    WHERE TRCCL_DESCRIPTION = 'Administrateur d''exploitation, accède en lecture.'
                                    AND TRCCL_VALUE = '8' AND TRCCL_VALUE_LABEL = 'Admin-Exploit'
                                 )
                AND(SELECT COUNT(ROWID) FROM TRCCL_CATALOG_CLAIMS
                            WHERE TRCCL_DESCRIPTION = 'Administrateur d''exploitation, accède en lecture.'
                                    AND TRCCL_VALUE = '8' AND TRCCL_VALUE_LABEL = 'Admin-Exploit') > 1
                AND TRCCL_STATUS = 'A'
                AND TRCCL_DESCRIPTION = 'Administrateur d''exploitation, accède en lecture.'
            ");

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                SELECT 'A', TRCL_CLAIMID,  '16', 'Administrateur des référentiels, peut modifier les référentiels.', 'U', 'Admin-Référentiels' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCCL_CATALOG_CLAIMS
                WHERE
                    ROWID NOT IN(SELECT MIN(ROWID) FROM TRCCL_CATALOG_CLAIMS
                                    WHERE TRCCL_DESCRIPTION = 'Administrateur des référentiels, peut modifier les référentiels.'
                                    AND TRCCL_VALUE = '16' AND TRCCL_VALUE_LABEL = 'Admin-Référentiels'
                                 )
                AND(SELECT COUNT(ROWID) FROM TRCCL_CATALOG_CLAIMS
                            WHERE TRCCL_DESCRIPTION = 'Administrateur des référentiels, peut modifier les référentiels.'
                                    AND TRCCL_VALUE = '16' AND TRCCL_VALUE_LABEL = 'Admin-Référentiels') > 1
                AND TRCCL_STATUS = 'A'
                AND TRCCL_DESCRIPTION = 'Administrateur des référentiels, peut modifier les référentiels.'
            ");

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                SELECT 'A', TRCL_CLAIMID,  '32', 'Administrateur avancé, possède tous les droits.', 'U', 'Super-Admin' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCCL_CATALOG_CLAIMS
                WHERE
                    ROWID NOT IN(SELECT MIN(ROWID) FROM TRCCL_CATALOG_CLAIMS
                                    WHERE TRCCL_DESCRIPTION = 'Administrateur avancé, possède tous les droits.'
                                    AND TRCCL_VALUE = '32' AND TRCCL_VALUE_LABEL = 'Super-Admin'
                                 )
                AND(SELECT COUNT(ROWID) FROM TRCCL_CATALOG_CLAIMS
                            WHERE TRCCL_DESCRIPTION = 'Administrateur avancé, possède tous les droits.'
                                    AND TRCCL_VALUE = '32' AND TRCCL_VALUE_LABEL = 'Super-Admin') > 1
                AND TRCCL_STATUS = 'A'
                AND TRCCL_DESCRIPTION = 'Administrateur avancé, possède tous les droits.'
            ");

        #endregion TRCCL_CATALOG_CLAIMS

        #region TRCLI_CLIENTAPPLICATIONS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCLI_CLIENTAPPLICATIONS
                    (TRCLI_CLIENTAPPLICATIONID, TRCLI_LABEL, TRCLI_DESCRIPTION, TRCLI_STATUS, TRCLI_AUTH_PUBLIC, TRCLI_AUTH_SECRET)
                VALUES
                    (1, 'MSO',          'Suivi des logs et planifications',                 'A', 'Krialys.Mso',         'BF4C1FDF-C85E-45FA-B6A6-EE1FC7DB78A1'),
                    (3, 'PARALLELU',    'Lancement ParallelU.',                             'A', 'Krialys.Orkestra.ParallelU',   '18A87A31-486A-429D-9F72-A25CF2157FF0'),
                    (4, 'ETL',          'ETL',                                              'A', 'Krialys.Etl',         '7AEDE76C-457E-4BC5-8F80-8229C2CBFA8F'),
                    (5, 'ADM',          'Application d''administration des habilitations.', 'A', NULL,                  NULL),
                    (6, 'DTM',          'Data Manager',                                     'A', 'Krialys.DTM',         'B0FFEC43-C495-42B5-A2B6-1EF92C44B27B'),     
                    (7, 'DTF',          'Data Fabrik',                                      'A', NULL,                  NULL);
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLI_CLIENTAPPLICATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 1)
                AND (SELECT COUNT(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 1) > 1
                AND TRCLI_CLIENTAPPLICATIONID = 1
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLI_CLIENTAPPLICATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 3)
                AND (SELECT COUNT(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 3) > 1
                AND TRCLI_CLIENTAPPLICATIONID = 3
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLI_CLIENTAPPLICATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 4)
                AND (SELECT COUNT(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 4) > 1
                AND TRCLI_CLIENTAPPLICATIONID = 4
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLI_CLIENTAPPLICATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 5)
                AND (SELECT COUNT(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 5) > 1
                AND TRCLI_CLIENTAPPLICATIONID = 5
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLI_CLIENTAPPLICATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 6)
                AND (SELECT COUNT(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 6) > 1
                AND TRCLI_CLIENTAPPLICATIONID = 6
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLI_CLIENTAPPLICATIONS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 7)
                AND (SELECT COUNT(ROWID) FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_CLIENTAPPLICATIONID = 7) > 1
                AND TRCLI_CLIENTAPPLICATIONID = 7
            ");

        #endregion TRCLI_CLIENTAPPLICATIONS

        #region TRAPLAS_APPLICATIONS_AUTH_SCENARIOS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRAPLAS_APPLICATIONS_AUTH_SCENARIOS
                    (TRAS_AUTH_SCENARIOID, TRCLI_CLIENTAPPLICATIONID)
                VALUES
                    (1, 1),
                    (3, 3),
                    (3, 4);
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS WHERE TRAS_AUTH_SCENARIOID = 1 AND TRCLI_CLIENTAPPLICATIONID = 1)
                AND (SELECT MAX(ROWID) FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS WHERE TRAS_AUTH_SCENARIOID = 1 AND TRCLI_CLIENTAPPLICATIONID = 1) > 1
                AND TRAS_AUTH_SCENARIOID = 1 AND TRCLI_CLIENTAPPLICATIONID = 1
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS WHERE TRAS_AUTH_SCENARIOID = 3 AND TRCLI_CLIENTAPPLICATIONID = 3)
                AND (SELECT MAX(ROWID) FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS WHERE TRAS_AUTH_SCENARIOID = 3 AND TRCLI_CLIENTAPPLICATIONID = 3) > 1
                AND TRAS_AUTH_SCENARIOID = 3 AND TRCLI_CLIENTAPPLICATIONID = 3
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS WHERE TRAS_AUTH_SCENARIOID = 3 AND TRCLI_CLIENTAPPLICATIONID = 4)
                AND (SELECT MAX(ROWID) FROM TRAPLAS_APPLICATIONS_AUTH_SCENARIOS WHERE TRAS_AUTH_SCENARIOID = 3 AND TRCLI_CLIENTAPPLICATIONID = 4) > 1
                AND TRAS_AUTH_SCENARIOID = 3 AND TRCLI_CLIENTAPPLICATIONID = 4
            ");

        #endregion TRAPLAS_APPLICATIONS_AUTH_SCENARIOS

        #region TRST_STATUTS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRST_STATUTS
                    (TRST_STATUTID, TRST_INFO, TRST_INFO_EN, TRST_REGLE01, TRST_REGLE02, TRST_REGLE03, TRST_REGLE04, TRST_REGLE05, TRST_REGLE06, TRST_REGLE07, TRST_REGLE08, TRST_REGLE09, TRST_REGLE10, TRST_DESCR)
                VALUES
                    ('A',  'Actif', 'Available', 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, ''),	
                    ('AD', 'Annulation en cours', 'Stopping', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('AR', 'Demande archivée', 'Archived request', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('B',  'Brouillon', 'Draft', 0, 1, 0, 1,	0, 0, 0, 0, 0, 0, ''),	
                    ('DA', 'Demande annulée', 'Canceled request', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('DB', 'Mode Brouillon', 'Draft', 0,	0, 1, 0, 0,	0, 0, 0, 0, 0, 'Statut demande'),
                    ('DC', 'Demande créée et attente d exécution' ,'Created request and wait for execution' ,0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('DP', 'Demande programmée', 'Scheduled request' ,0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('DR', 'Demande Réalisée', 'Realized request', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('EC', 'En cours d exécution', 'In progress', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('ER', 'Erreur d exécution', 'In error', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('I',  'Inactif', 'Deactivated', 1, 1, 0, 0, 0, 0, 0, 0,	0, 0, ''),	
                    ('IV', 'Résultats Invalidés', 'Invalidated result', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('MO', 'Modele pour planification', 'Schedule model', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,	'Statut demande'),
                    ('NF', 'Delai d attente prérequis passé', 'Waiting trigger file Timeout', 0, 0,	1, 0, 0, 0, 0, 0, 0, 0,	'Statut demande'),
                    ('VA', 'Résultats Validés', 'Validated result', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('WF', 'Attente fichier déclencheur', 'Waiting trigger file', 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 'Statut demande'),
                    ('P',  'Prototype', 'Prototype', 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 'statut version etat'),
                    ('C',  'Annulé', 'Canceled', 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 'statut version etat'),
                    ('PA', 'Planif Annulée', 'Planning Cancelled', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 'statut ecarté de l IHM');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'A')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'A') > 1
                AND TRST_STATUTID = 'A'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'AD')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'AD') > 1
                AND TRST_STATUTID = 'AD'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'AR')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'AR') > 1
                AND TRST_STATUTID = 'AR'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'B')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'B') > 1
                AND TRST_STATUTID = 'B'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DA')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DA') > 1
                AND TRST_STATUTID = 'DA'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DB')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DB') > 1
                AND TRST_STATUTID = 'DB'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DC')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DC') > 1
                AND TRST_STATUTID = 'DC'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DP')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DP') > 1
                AND TRST_STATUTID = 'DP'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DR')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'DR') > 1
                AND TRST_STATUTID = 'DR'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'EC')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'EC') > 1
                AND TRST_STATUTID = 'EC'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'ER')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'ER') > 1
                AND TRST_STATUTID = 'ER'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'I')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'I') > 1
                AND TRST_STATUTID = 'I'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'IV')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'IV') > 1
                AND TRST_STATUTID = 'IV'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'MO')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'MO') > 1
                AND TRST_STATUTID = 'MO'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'NF')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'NF') > 1
                AND TRST_STATUTID = 'NF'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'VA')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'VA') > 1
                AND TRST_STATUTID = 'VA'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'WF')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'WF') > 1
                AND TRST_STATUTID = 'WF'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'P')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'P') > 1
                AND TRST_STATUTID = 'P'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'C')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'C') > 1
                AND TRST_STATUTID = 'C'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRST_STATUTS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'PA')
                AND (SELECT COUNT(ROWID) FROM TRST_STATUTS WHERE TRST_STATUTID = 'PA') > 1
                AND TRST_STATUTID = 'PA'
            ");

        #endregion TRST_STATUTS

        #region TRU_USERS | TRUCL_USERS_CLAIMS

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRU_USERS
                    (TRU_USERID, TRLG_LNGID, TRTZ_TZID, TRU_ALLOW_INTERNAL_AUTH, TRU_EMAIL, TRU_FIRST_NAME, TRU_LOGIN, TRU_NAME, TRU_PWD, TRU_STATUS)
                VALUES
                    ('eee2d5b5558643f496bfbc2a6eec3321', 'fr-FR', 'Europe/Paris', 1, Null, 'ADMIN', 'KADMIN', 'ADMIN', 'dn0RiddgkCj2YBA/9Az2gLLEzZcMVbkmKrIxeloyS5HenGFJAySoCcVlc75TTwYy3Ci965Ao8UkCBgAmYgxKKw==', 'A');
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRUCL_USERS_CLAIMS
                    (TRCLI_CLIENTAPPLICATIONID, TRCL_CLAIMID, TRUCL_CLAIM_VALUE, TRUCL_DESCRIPTION, TRUCL_STATUS, TRU_USERID)
                SELECT 1, TRCL_CLAIMID,  '32', 'MSO', 'A', 'eee2d5b5558643f496bfbc2a6eec3321' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;                   
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRUCL_USERS_CLAIMS
                    (TRCLI_CLIENTAPPLICATIONID, TRCL_CLAIMID, TRUCL_CLAIM_VALUE, TRUCL_DESCRIPTION, TRUCL_STATUS, TRU_USERID)
                SELECT 5, TRCL_CLAIMID,  '32', 'ADM', 'A', 'eee2d5b5558643f496bfbc2a6eec3321' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;                   
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRUCL_USERS_CLAIMS
                    (TRCLI_CLIENTAPPLICATIONID, TRCL_CLAIMID, TRUCL_CLAIM_VALUE, TRUCL_DESCRIPTION, TRUCL_STATUS, TRU_USERID)
                SELECT 6, TRCL_CLAIMID,  '18', 'DTM', 'A', 'eee2d5b5558643f496bfbc2a6eec3321' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;                   
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRUCL_USERS_CLAIMS
                    (TRCLI_CLIENTAPPLICATIONID, TRCL_CLAIMID, TRUCL_CLAIM_VALUE, TRUCL_DESCRIPTION, TRUCL_STATUS, TRU_USERID)
                SELECT 7, TRCL_CLAIMID,  '8', 'DTF', 'A', 'eee2d5b5558643f496bfbc2a6eec3321' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'Role' LIMIT 1;                   
            ");

        #endregion TRU_USERS | TRUCL_USERS_CLAIMS

        #region TRCLICL_CLIENTAPPLICATIONS_CLAIMS

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCLICL_CLIENTAPPLICATIONS_CLAIMS
                    (TRCL_CLAIMID, TRCLI_CLIENTAPPLICATIONID, TRCLICL_CLAIM_VALUE, TRCLICL_DESCRIPTION, TRCLICL_STATUS)
                SELECT TRCL_CLAIMID, (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'ETL' LIMIT 1), '600', null, 'A' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLICL_CLIENTAPPLICATIONS_CLAIMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLICL_CLIENTAPPLICATIONS_CLAIMS 
                        WHERE TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1) 
                          AND TRCLI_CLIENTAPPLICATIONID = (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'ETL' LIMIT 1))

                AND (SELECT COUNT(ROWID) FROM TP_PERIMETRES 
                        WHERE TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1) 
                          AND TRCLI_CLIENTAPPLICATIONID = (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'ETL' LIMIT 1))

                AND TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1) 
                AND TRCLI_CLIENTAPPLICATIONID = (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'ETL' LIMIT 1)
            ");

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TRCLICL_CLIENTAPPLICATIONS_CLAIMS
                    (TRCL_CLAIMID, TRCLI_CLIENTAPPLICATIONID, TRCLICL_CLAIM_VALUE, TRCLICL_DESCRIPTION, TRCLICL_STATUS)
                SELECT TRCL_CLAIMID,  (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'PARALLELU' LIMIT 1), '600', null, 'A' FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1;
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TRCLICL_CLIENTAPPLICATIONS_CLAIMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TRCLICL_CLIENTAPPLICATIONS_CLAIMS 
                        WHERE TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1) 
                          AND TRCLI_CLIENTAPPLICATIONID = (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'PARALLELU' LIMIT 1))

                AND (SELECT COUNT(ROWID) FROM TP_PERIMETRES 
                        WHERE TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1) 
                          AND TRCLI_CLIENTAPPLICATIONID = (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'PARALLELU' LIMIT 1))

                AND TRCL_CLAIMID = (SELECT TRCL_CLAIMID FROM TRCL_CLAIMS WHERE TRCL_CLAIM_NAME = 'TokenLifetime' LIMIT 1) 
                AND TRCLI_CLIENTAPPLICATIONID = (SELECT TRCLI_CLIENTAPPLICATIONID FROM TRCLI_CLIENTAPPLICATIONS WHERE TRCLI_LABEL = 'PARALLELU' LIMIT 1)
            ");

        #endregion TRCLICL_CLIENTAPPLICATIONS_CLAIMS

        #region TQM_QUALIF_MODELES

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TQM_QUALIF_MODELES
                    (TQM_LIB, TQM_VALEUR_MIN, TQM_VALEUR_MAX)
                VALUES
                    ('FEUX', 1,	3),
                    ('CIEL', 1,	3),
                    ('SMILE', 1, 3);
            ");

        migrationBuilder.Sql(@" 
                DELETE FROM TQM_QUALIF_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TQM_QUALIF_MODELES WHERE TQM_LIB = 'FEUX')
                AND (SELECT COUNT(ROWID) FROM TQM_QUALIF_MODELES WHERE TQM_LIB = 'FEUX') > 1
                AND TQM_LIB = 'FEUX'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TQM_QUALIF_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TQM_QUALIF_MODELES WHERE TQM_LIB = 'CIEL')
                AND (SELECT COUNT(ROWID) FROM TQM_QUALIF_MODELES WHERE TQM_LIB = 'CIEL') > 1
                AND TQM_LIB = 'CIEL'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TQM_QUALIF_MODELES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TQM_QUALIF_MODELES WHERE TQM_LIB = 'SMILE')
                AND (SELECT COUNT(ROWID) FROM TQM_QUALIF_MODELES WHERE TQM_LIB = 'SMILE') > 1
                AND TQM_LIB = 'SMILE'
            ");

        #endregion TQM_QUALIF_MODELES

        #region TP_PERIMETRES

        migrationBuilder.Sql(@"
                INSERT OR IGNORE INTO TP_PERIMETRES
                    (TP_PERIMETRE, TP_DATE_CREATION, TRST_STATUTID, TP_LOGO, TQM_QUALIF_MODELEID)
                VALUES
                    ('BU principale', DATETIME(), 'A', NULL, 1);
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TP_PERIMETRES 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TP_PERIMETRES WHERE TP_PERIMETRE = 'BU principale')
                AND (SELECT COUNT(ROWID) FROM TP_PERIMETRES WHERE TP_PERIMETRE = 'BU principale') > 1
                AND TP_PERIMETRE = 'BU principale'
            ");

        #endregion TP_PERIMETRES

        #region Small Sql Update

        migrationBuilder.Sql(@"
                -- DTF>production UTD
                update TD_DEMANDES
	                set TD_DATE_LIVRAISON = SUBSTRING(tempo.TD_DATE_LIVRAISON, 1, 17) || '00'
	                from (select TD_DEMANDEID, TD_DATE_LIVRAISON from TD_DEMANDES WHERE TD_DATE_LIVRAISON is not null) as tempo
	                where TD_DEMANDES.TD_DEMANDEID = tempo.TD_DEMANDEID;
            ");

        migrationBuilder.Sql(@"
                UPDATE TE_ETATS set TE_NOM_ETAT = upper(substr( trim(TE_NOM_ETAT), 1, 1)) || lower(substr(trim(TE_NOM_ETAT), 2));
                UPDATE TS_SCENARIOS set TS_NOM_SCENARIO  = upper(substr( trim(TS_NOM_SCENARIO), 1, 1)) || lower(substr(trim(TS_NOM_SCENARIO), 2));
                UPDATE TC_CATEGORIES set TC_NOM = upper(substr( trim(TC_NOM), 1, 1)) || lower(substr(trim(TC_NOM), 2));
            ");

        #endregion Small Sql Update

        migrationBuilder.Sql(@"
                PRAGMA foreign_keys=on;
            ");

        #endregion

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TMT_MAILING_LIST",
            table: "TMT_MAIL_TEMPLATES");
    }
}
