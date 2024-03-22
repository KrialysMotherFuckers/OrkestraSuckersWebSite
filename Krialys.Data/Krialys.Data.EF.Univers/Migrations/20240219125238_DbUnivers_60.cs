using Krialys.Data.EF.Univers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_60 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var ctx = new KrialysDbContext())
            {
                //var connStr = ctx.Database.GetConnectionString();
                using (var conn = new SqliteConnection($"DataSource=App_Data/Database/db-Univers.db3"))
                    try
                    {
                        conn.Open();
                        using (var cmd = new SqliteCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = @"

                                /* Migrate from TRLG_LNGS to TR_LNG_Languages */

                                PRAGMA foreign_keys = 0;

                                CREATE TABLE TR_LNG_Languages (
                                    lng_id                    TEXT    NOT NULL
                                                                DEFAULT (upper(hex(randomblob(4) ) ) || '-' || upper(hex(randomblob(2) ) ) || '-4' || substr(upper(hex(randomblob(2) ) ), 2) || '-' || substr('89ab', abs(random() ) % 4 + 1, 1) || substr(upper(hex(randomblob(2) ) ), 2) || '-' || upper(hex(randomblob(6) ) ) ),
                                    lng_label                 TEXT    NOT NULL,
                                    lng_code                  TEXT    NOT NULL
                                                                      CONSTRAINT PK_TRLG_LNGS PRIMARY KEY,
                                    lng_is_preferred_language INTEGER
                                );

                                INSERT INTO TR_LNG_Languages (
                                                                 lng_code,
                                                                 lng_label,
                                                                 lng_is_preferred_language
                                                             )
                                                             SELECT TRLG_LNGID,
                                                                    (Case TRLG_LNGID WHEN ""fr-FR"" THEN ""Français"" ELSE ""English"" END),
                                                                    TRLG_PREFERED_LNG
                                                               FROM TRLG_LNGS;

                                CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                                                          FROM TRU_USERS;

                                DROP TABLE TRU_USERS;

                                CREATE TABLE TRU_USERS (
                                    TRU_USERID              TEXT    NOT NULL
                                                                    CONSTRAINT PK_TRU_USERS PRIMARY KEY,
                                    TRLG_LNGID              TEXT    NOT NULL,
                                    TRTZ_TZID               TEXT    NOT NULL,
                                    TRU_ALLOW_INTERNAL_AUTH INTEGER NOT NULL,
                                    TRU_EMAIL               TEXT,
                                    TRU_FIRST_NAME          TEXT    NOT NULL,
                                    TRU_FULLNAME                    AS (TRU_NAME || ' ' || TRU_FIRST_NAME),
                                    TRU_LOGIN               TEXT    NOT NULL,
                                    TRU_NAME                TEXT    NOT NULL,
                                    TRU_PWD                 TEXT,
                                    TRU_STATUS              TEXT    NOT NULL,
                                    CONSTRAINT FK_TRU_USERS_TRLG_LNGS_TRLG_LNGID FOREIGN KEY (
                                        TRLG_LNGID
                                    )
                                    REFERENCES TR_LNG_Languages (lng_code) ON DELETE CASCADE,
                                    CONSTRAINT FK_TRU_USERS_TRTZ_TZS_TRTZ_TZID FOREIGN KEY (
                                        TRTZ_TZID
                                    )
                                    REFERENCES TRTZ_TZS (TRTZ_TZID) ON DELETE CASCADE
                                );

                                INSERT INTO TRU_USERS (
                                                          TRU_USERID,
                                                          TRLG_LNGID,
                                                          TRTZ_TZID,
                                                          TRU_ALLOW_INTERNAL_AUTH,
                                                          TRU_EMAIL,
                                                          TRU_FIRST_NAME,
                                                          TRU_LOGIN,
                                                          TRU_NAME,
                                                          TRU_PWD,
                                                          TRU_STATUS
                                                      )
                                                      SELECT TRU_USERID,
                                                             TRLG_LNGID,
                                                             TRTZ_TZID,
                                                             TRU_ALLOW_INTERNAL_AUTH,
                                                             TRU_EMAIL,
                                                             TRU_FIRST_NAME,
                                                             TRU_LOGIN,
                                                             TRU_NAME,
                                                             TRU_PWD,
                                                             TRU_STATUS
                                                        FROM sqlitestudio_temp_table;

                                CREATE TABLE sqlitestudio_temp_table0 AS SELECT *
                                                                           FROM TCMD_SP_SUIVI_PHASES;

                                DROP TABLE TCMD_SP_SUIVI_PHASES;

                                CREATE TABLE TCMD_SP_SUIVI_PHASES (
                                    TCMD_SP_SUIVI_PHASEID INTEGER NOT NULL
                                                                  CONSTRAINT PK_TCMD_SP_SUIVI_PHASES PRIMARY KEY AUTOINCREMENT,
                                    TCMD_COMMANDEID       INTEGER NOT NULL,
                                    TCMD_PH_PHASE_APRESID INTEGER NOT NULL,
                                    TCMD_PH_PHASE_AVANTID INTEGER,
                                    TCMD_SP_COMMENTAIRE   TEXT,
                                    TCMD_SP_DATE_MODIF    TEXT,
                                    TRU_AUTEUR_MODIFID    TEXT,
                                    CONSTRAINT FK_TCMD_SP_SUIVI_PHASES_TCMD_COMMANDES_TCMD_COMMANDEID FOREIGN KEY (
                                        TCMD_COMMANDEID
                                    )
                                    REFERENCES TCMD_COMMANDES (TCMD_COMMANDEID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASES_TCMD_PH_PHASE_APRESID FOREIGN KEY (
                                        TCMD_PH_PHASE_APRESID
                                    )
                                    REFERENCES TCMD_PH_PHASES (TCMD_PH_PHASEID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASES_TCMD_PH_PHASE_AVANTID FOREIGN KEY (
                                        TCMD_PH_PHASE_AVANTID
                                    )
                                    REFERENCES TCMD_PH_PHASES (TCMD_PH_PHASEID) ON DELETE RESTRICT
                                );

                                INSERT INTO TCMD_SP_SUIVI_PHASES (
                                                                     TCMD_SP_SUIVI_PHASEID,
                                                                     TCMD_COMMANDEID,
                                                                     TCMD_PH_PHASE_APRESID,
                                                                     TCMD_PH_PHASE_AVANTID,
                                                                     TCMD_SP_COMMENTAIRE,
                                                                     TCMD_SP_DATE_MODIF,
                                                                     TRU_AUTEUR_MODIFID
                                                                 )
                                                                 SELECT TCMD_SP_SUIVI_PHASEID,
                                                                        TCMD_COMMANDEID,
                                                                        TCMD_PH_PHASE_APRESID,
                                                                        TCMD_PH_PHASE_AVANTID,
                                                                        TCMD_SP_COMMENTAIRE,
                                                                        TCMD_SP_DATE_MODIF,
                                                                        TRU_AUTEUR_MODIFID
                                                                   FROM sqlitestudio_temp_table0;

                                CREATE TABLE sqlitestudio_temp_table1 AS SELECT *
                                                                           FROM TCMD_CR_CMD_RAISON_PHASES;

                                DROP TABLE TCMD_CR_CMD_RAISON_PHASES;

                                CREATE TABLE TCMD_CR_CMD_RAISON_PHASES (
                                    TCMD_CR_CMD_RAISON_PHASEID INTEGER NOT NULL
                                                                       CONSTRAINT PK_TCMD_CR_CMD_RAISON_PHASES PRIMARY KEY AUTOINCREMENT,
                                    TCMD_SP_SUIVI_PHASEID      INTEGER NOT NULL,
                                    TCMD_RP_RAISON_PHASEID     INTEGER NOT NULL,
                                    CONSTRAINT FK_TCMD_CR_CMD_RAISON_PHASES_TCMD_RP_RAISON_PHASES_TCMD_RP_RAISON_PHASEID FOREIGN KEY (
                                        TCMD_RP_RAISON_PHASEID
                                    )
                                    REFERENCES TCMD_RP_RAISON_PHASES (TCMD_RP_RAISON_PHASEID) ON DELETE RESTRICT
                                );

                                INSERT INTO TCMD_CR_CMD_RAISON_PHASES (
                                                                          TCMD_CR_CMD_RAISON_PHASEID,
                                                                          TCMD_SP_SUIVI_PHASEID,
                                                                          TCMD_RP_RAISON_PHASEID
                                                                      )
                                                                      SELECT TCMD_CR_CMD_RAISON_PHASEID,
                                                                             TCMD_SP_SUIVI_PHASEID,
                                                                             TCMD_RP_RAISON_PHASEID
                                                                        FROM sqlitestudio_temp_table1;

                                DROP TABLE sqlitestudio_temp_table1;

                                CREATE INDEX IX_TCMD_CR_CMD_RAISON_PHASES_TCMD_RP_RAISON_PHASEID ON TCMD_CR_CMD_RAISON_PHASES (
                                    ""TCMD_RP_RAISON_PHASEID""
                                );

                                CREATE INDEX IX_TCMD_CR_CMD_RAISON_PHASES_TCMD_SP_SUIVI_PHASEID ON TCMD_CR_CMD_RAISON_PHASES (
                                    ""TCMD_SP_SUIVI_PHASEID""
                                );

                                DROP TABLE sqlitestudio_temp_table0;

                                CREATE INDEX IX_TCMD_SP_SUIVI_PHASES_TCMD_COMMANDEID ON TCMD_SP_SUIVI_PHASES (
                                    ""TCMD_COMMANDEID""
                                );

                                CREATE INDEX IX_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASE_APRESID ON TCMD_SP_SUIVI_PHASES (
                                    ""TCMD_PH_PHASE_APRESID""
                                );

                                CREATE INDEX IX_TCMD_SP_SUIVI_PHASES_TCMD_PH_PHASE_AVANTID ON TCMD_SP_SUIVI_PHASES (
                                    ""TCMD_PH_PHASE_AVANTID""
                                );

                                CREATE INDEX IX_TCMD_SP_SUIVI_PHASES_TRU_AUTEUR_MODIFID ON TCMD_SP_SUIVI_PHASES (
                                    ""TRU_AUTEUR_MODIFID""
                                );

                                CREATE TABLE sqlitestudio_temp_table2 AS SELECT *
                                                                           FROM TCMD_DA_DEMANDES_ASSOCIEES;

                                DROP TABLE TCMD_DA_DEMANDES_ASSOCIEES;

                                CREATE TABLE TCMD_DA_DEMANDES_ASSOCIEES (
                                    TCMD_DA_DEMANDES_ASSOCIEEID INTEGER NOT NULL
                                                                        CONSTRAINT PK_TCMD_DA_DEMANDES_ASSOCIEES PRIMARY KEY AUTOINCREMENT,
                                    TCMD_COMMANDEID             INTEGER NOT NULL,
                                    TD_DEMANDEID                INTEGER NOT NULL,
                                    TCMD_DA_COMMENTAIRE         TEXT,
                                    TCMD_DA_VERSION_NOTABLE     INTEGER DEFAULT (0),
                                    TCMD_DA_DATE_ASSOCIATION    TEXT    NOT NULL,
                                    TRU_AUTEURID                TEXT,
                                    CONSTRAINT FK_TCMD_DA_DEMANDES_ASSOCIEES_TCMD_COMMANDES_TCMD_COMMANDEID FOREIGN KEY (
                                        TCMD_COMMANDEID
                                    )
                                    REFERENCES TCMD_COMMANDES (TCMD_COMMANDEID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_DA_DEMANDES_ASSOCIEES_TD_DEMANDES_TD_DEMANDEID FOREIGN KEY (
                                        TD_DEMANDEID
                                    )
                                    REFERENCES TD_DEMANDES (TD_DEMANDEID) ON DELETE CASCADE
                                );

                                INSERT INTO TCMD_DA_DEMANDES_ASSOCIEES (
                                                                           TCMD_DA_DEMANDES_ASSOCIEEID,
                                                                           TCMD_COMMANDEID,
                                                                           TD_DEMANDEID,
                                                                           TCMD_DA_COMMENTAIRE,
                                                                           TCMD_DA_VERSION_NOTABLE,
                                                                           TCMD_DA_DATE_ASSOCIATION,
                                                                           TRU_AUTEURID
                                                                       )
                                                                       SELECT TCMD_DA_DEMANDES_ASSOCIEEID,
                                                                              TCMD_COMMANDEID,
                                                                              TD_DEMANDEID,
                                                                              TCMD_DA_COMMENTAIRE,
                                                                              TCMD_DA_VERSION_NOTABLE,
                                                                              TCMD_DA_DATE_ASSOCIATION,
                                                                              TRU_AUTEURID
                                                                         FROM sqlitestudio_temp_table2;

                                DROP TABLE sqlitestudio_temp_table2;

                                CREATE INDEX IX_TCMD_DA_DEMANDES_ASSOCIEES_TCMD_COMMANDEID ON TCMD_DA_DEMANDES_ASSOCIEES (
                                    ""TCMD_COMMANDEID""
                                );

                                CREATE INDEX IX_TCMD_DA_DEMANDES_ASSOCIEES_TD_DEMANDEID ON TCMD_DA_DEMANDES_ASSOCIEES (
                                    ""TD_DEMANDEID""
                                );

                                CREATE INDEX IX_TCMD_DA_DEMANDES_ASSOCIEES_TRU_AUTEURID ON TCMD_DA_DEMANDES_ASSOCIEES (
                                    ""TRU_AUTEURID""
                                );

                                CREATE TABLE sqlitestudio_temp_table3 AS SELECT *
                                                                           FROM TRUCL_USERS_CLAIMS;

                                DROP TABLE TRUCL_USERS_CLAIMS;

                                CREATE TABLE TRUCL_USERS_CLAIMS (
                                    TRUCL_USER_CLAIMID        INTEGER NOT NULL
                                                                      CONSTRAINT PK_TRUCL_USERS_CLAIMS PRIMARY KEY AUTOINCREMENT,
                                    TRCLI_CLIENTAPPLICATIONID INTEGER NOT NULL,
                                    TRCL_CLAIMID              INTEGER NOT NULL,
                                    TRUCL_CLAIM_VALUE         TEXT,
                                    TRUCL_DESCRIPTION         TEXT    NOT NULL,
                                    TRUCL_STATUS              TEXT    NOT NULL,
                                    TRU_USERID                TEXT,
                                    CONSTRAINT FK_TRUCL_USERS_CLAIMS_TRCL_CLAIMS FOREIGN KEY (
                                        TRCL_CLAIMID
                                    )
                                    REFERENCES TRCL_CLAIMS (TRCL_CLAIMID) ON DELETE CASCADE,
                                    CONSTRAINT FK_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONS_TRCLI_CLIENTAPPLICATIONID FOREIGN KEY (
                                        TRCLI_CLIENTAPPLICATIONID
                                    )
                                    REFERENCES TRCLI_CLIENTAPPLICATIONS (TRCLI_CLIENTAPPLICATIONID) ON DELETE CASCADE
                                );

                                INSERT INTO TRUCL_USERS_CLAIMS (
                                                                   TRUCL_USER_CLAIMID,
                                                                   TRCLI_CLIENTAPPLICATIONID,
                                                                   TRCL_CLAIMID,
                                                                   TRUCL_CLAIM_VALUE,
                                                                   TRUCL_DESCRIPTION,
                                                                   TRUCL_STATUS,
                                                                   TRU_USERID
                                                               )
                                                               SELECT TRUCL_USER_CLAIMID,
                                                                      TRCLI_CLIENTAPPLICATIONID,
                                                                      TRCL_CLAIMID,
                                                                      TRUCL_CLAIM_VALUE,
                                                                      TRUCL_DESCRIPTION,
                                                                      TRUCL_STATUS,
                                                                      TRU_USERID
                                                                 FROM sqlitestudio_temp_table3;

                                DROP TABLE sqlitestudio_temp_table3;

                                CREATE INDEX IX_TRUCL_USERS_CLAIMS_TRCLI_CLIENTAPPLICATIONID ON TRUCL_USERS_CLAIMS (
                                    ""TRCLI_CLIENTAPPLICATIONID""
                                );

                                CREATE INDEX IX_TRUCL_USERS_CLAIMS_TRCL_CLAIMID ON TRUCL_USERS_CLAIMS (
                                    ""TRCL_CLAIMID""
                                );

                                CREATE INDEX UQ_TRUCL_USERS_CLAIMS ON TRUCL_USERS_CLAIMS (
                                    ""TRU_USERID"",
                                    ""TRCL_CLAIMID""
                                );

                                CREATE TABLE sqlitestudio_temp_table4 AS SELECT *
                                                                           FROM TE_ETATS;

                                DROP TABLE TE_ETATS;

                                CREATE TABLE TE_ETATS (
                                    TE_ETATID                         INTEGER NOT NULL
                                                                              CONSTRAINT PK_TE_ETATS PRIMARY KEY AUTOINCREMENT,
                                    PARENT_ETATID                     INTEGER,
                                    TEM_ETAT_MASTERID                 INTEGER NOT NULL,
                                    TE_COMMENTAIRE                    TEXT    NOT NULL,
                                    TE_DATE_DERNIERE_PRODUCTION       TEXT,
                                    TE_DATE_REVISION                  TEXT    NOT NULL,
                                    TE_DUREE_DERNIERE_PRODUCTION      INTEGER,
                                    TE_DUREE_PRODUCTION_ESTIMEE       INTEGER,
                                    TE_ENV_VIERGE_TAILLE              INTEGER,
                                    TE_ENV_VIERGE_UPLOADED            TEXT    NOT NULL
                                                                              DEFAULT ( ('N') ),
                                    TE_GUID                           TEXT,
                                    TE_INDICE_REVISION_L1             INTEGER NOT NULL
                                                                              DEFAULT ( ( (1) ) ),
                                    TE_INDICE_REVISION_L2             INTEGER NOT NULL,
                                    TE_INDICE_REVISION_L3             INTEGER NOT NULL,
                                    TE_INFO_REVISION                  TEXT,
                                    TE_NOM_DATABASE                   TEXT,
                                    TE_NOM_ETAT                       TEXT    NOT NULL,
                                    TE_NOM_SERVEUR_CUBE               TEXT,
                                    TE_SEND_MAIL_CLIENT               TEXT    DEFAULT ( ('O') ),
                                    TE_SEND_MAIL_GESTIONNAIRE         TEXT    DEFAULT ( ('O') ),
                                    TE_TYPE_SORTIE                    TEXT    NOT NULL
                                                                              DEFAULT ( ('M') ),
                                    TE_VALIDATION_IMPLICITE           TEXT    DEFAULT ( ('N') ),
                                    TRST_STATUTID                     TEXT    NOT NULL
                                                                              DEFAULT ( ('B') ),
                                    TRU_DECLARANTID                   TEXT    NOT NULL,
                                    TRU_ENV_VIERGE_AUTEURID           TEXT,
                                    TE_GENERE_CUBE                    TEXT    NOT NULL
                                                                              DEFAULT '',
                                    TE_VERSION                                AS (TE_INDICE_REVISION_L1 || '.' || TE_INDICE_REVISION_L2 || '.' || TE_INDICE_REVISION_L3),
                                    TE_FULLNAME                               AS (TE_NOM_ETAT || ' (' || TE_INDICE_REVISION_L1 || '.' || TE_INDICE_REVISION_L2 || '.' || TE_INDICE_REVISION_L3 || ')'),
                                    TE_ENV_VIERGE_DATE_DIAG_VALIDE    TEXT,
                                    TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP TEXT,
                                    TRST_STATUTID_OLD                 TEXT,
                                    CONSTRAINT FK_TE_ETATS_TRST_STATUTS_TRST_STATUTID FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) ON DELETE CASCADE,
                                    CONSTRAINT [FK_TE_ETATS$TEM_ETAT_MASTERS] FOREIGN KEY (
                                        TEM_ETAT_MASTERID
                                    )
                                    REFERENCES TEM_ETAT_MASTERS (TEM_ETAT_MASTERID) ON DELETE RESTRICT
                                );

                                INSERT INTO TE_ETATS (
                                                         TE_ETATID,
                                                         PARENT_ETATID,
                                                         TEM_ETAT_MASTERID,
                                                         TE_COMMENTAIRE,
                                                         TE_DATE_DERNIERE_PRODUCTION,
                                                         TE_DATE_REVISION,
                                                         TE_DUREE_DERNIERE_PRODUCTION,
                                                         TE_DUREE_PRODUCTION_ESTIMEE,
                                                         TE_ENV_VIERGE_TAILLE,
                                                         TE_ENV_VIERGE_UPLOADED,
                                                         TE_GUID,
                                                         TE_INDICE_REVISION_L1,
                                                         TE_INDICE_REVISION_L2,
                                                         TE_INDICE_REVISION_L3,
                                                         TE_INFO_REVISION,
                                                         TE_NOM_DATABASE,
                                                         TE_NOM_ETAT,
                                                         TE_NOM_SERVEUR_CUBE,
                                                         TE_SEND_MAIL_CLIENT,
                                                         TE_SEND_MAIL_GESTIONNAIRE,
                                                         TE_TYPE_SORTIE,
                                                         TE_VALIDATION_IMPLICITE,
                                                         TRST_STATUTID,
                                                         TRU_DECLARANTID,
                                                         TRU_ENV_VIERGE_AUTEURID,
                                                         TE_GENERE_CUBE,
                                                         TE_ENV_VIERGE_DATE_DIAG_VALIDE,
                                                         TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP,
                                                         TRST_STATUTID_OLD
                                                     )
                                                     SELECT TE_ETATID,
                                                            PARENT_ETATID,
                                                            TEM_ETAT_MASTERID,
                                                            TE_COMMENTAIRE,
                                                            TE_DATE_DERNIERE_PRODUCTION,
                                                            TE_DATE_REVISION,
                                                            TE_DUREE_DERNIERE_PRODUCTION,
                                                            TE_DUREE_PRODUCTION_ESTIMEE,
                                                            TE_ENV_VIERGE_TAILLE,
                                                            TE_ENV_VIERGE_UPLOADED,
                                                            TE_GUID,
                                                            TE_INDICE_REVISION_L1,
                                                            TE_INDICE_REVISION_L2,
                                                            TE_INDICE_REVISION_L3,
                                                            TE_INFO_REVISION,
                                                            TE_NOM_DATABASE,
                                                            TE_NOM_ETAT,
                                                            TE_NOM_SERVEUR_CUBE,
                                                            TE_SEND_MAIL_CLIENT,
                                                            TE_SEND_MAIL_GESTIONNAIRE,
                                                            TE_TYPE_SORTIE,
                                                            TE_VALIDATION_IMPLICITE,
                                                            TRST_STATUTID,
                                                            TRU_DECLARANTID,
                                                            TRU_ENV_VIERGE_AUTEURID,
                                                            TE_GENERE_CUBE,
                                                            TE_ENV_VIERGE_DATE_DIAG_VALIDE,
                                                            TE_ENV_VIERGE_DATE_MODIF_HORS_ZIP,
                                                            TRST_STATUTID_OLD
                                                       FROM sqlitestudio_temp_table4;

                                CREATE TABLE sqlitestudio_temp_table5 AS SELECT *
                                                                           FROM TD_DEMANDES;

                                DROP TABLE TD_DEMANDES;

                                CREATE TABLE TD_DEMANDES (
                                    TD_DEMANDEID                INTEGER NOT NULL
                                                                        CONSTRAINT PK_TD_DEMANDES PRIMARY KEY AUTOINCREMENT,
                                    TD_COMMENTAIRE_GESTIONNAIRE TEXT,
                                    TD_COMMENTAIRE_UTILISATEUR  TEXT,
                                    TD_DATE_AVIS_GESTIONNAIRE   TEXT,
                                    TD_DATE_DEMANDE             TEXT    NOT NULL,
                                    TD_DATE_DERNIER_DOWNLOAD    TEXT,
                                    TD_DATE_EXECUTION_SOUHAITEE TEXT,
                                    TD_DATE_LIVRAISON           TEXT,
                                    TD_DATE_PRISE_EN_CHARGE     TEXT,
                                    TD_DEMANDE_ORIGINEID        INTEGER,
                                    TD_DUREE_PRODUCTION_REEL    INTEGER DEFAULT ( ( (0) ) ),
                                    TD_GUID                     TEXT,
                                    TD_INFO_RETOUR_TRAITEMENT   TEXT,
                                    TD_PREREQUIS_DELAI_CHK      INTEGER,
                                    TD_QUALIF_BILAN             INTEGER,
                                    TD_QUALIF_EXIST_FILE        TEXT,
                                    TD_QUALIF_FILE_SIZE         INTEGER,
                                    TD_RESULT_EXIST_FILE        TEXT,
                                    TD_RESULT_FILE_SIZE         INTEGER,
                                    TD_RESULT_NB_DOWNLOAD       INTEGER DEFAULT ( ( (0) ) ),
                                    TD_SEND_MAIL_CLIENT         TEXT    DEFAULT ( ('O') ),
                                    TD_SEND_MAIL_GESTIONNAIRE   TEXT,
                                    TE_ETATID                   INTEGER NOT NULL,
                                    TPF_PLANIF_ORIGINEID        INTEGER,
                                    TRST_STATUTID               TEXT    NOT NULL
                                                                        DEFAULT ( ('DB') ),
                                    TRU_DEMANDEURID             TEXT    NOT NULL,
                                    TRU_GESTIONNAIRE_VALIDEURID TEXT,
                                    TSRV_SERVEURID              INTEGER,
                                    TS_SCENARIOID               INTEGER,
                                    TD_IGNORE_RESULT            TEXT,
                                    TD_SUSPEND_EXECUTION        TEXT,
                                    CONSTRAINT AK_TD_DEMANDES_TD_DEMANDEID_TE_ETATID UNIQUE (
                                        TD_DEMANDEID,
                                        TE_ETATID
                                    ),
                                    CONSTRAINT [FK_TD_DEMANDES$SERVEUR] FOREIGN KEY (
                                        TSRV_SERVEURID
                                    )
                                    REFERENCES TSRV_SERVEURS (TSRV_SERVEURID) ON DELETE RESTRICT
                                );

                                INSERT INTO TD_DEMANDES (
                                                            TD_DEMANDEID,
                                                            TD_COMMENTAIRE_GESTIONNAIRE,
                                                            TD_COMMENTAIRE_UTILISATEUR,
                                                            TD_DATE_AVIS_GESTIONNAIRE,
                                                            TD_DATE_DEMANDE,
                                                            TD_DATE_DERNIER_DOWNLOAD,
                                                            TD_DATE_EXECUTION_SOUHAITEE,
                                                            TD_DATE_LIVRAISON,
                                                            TD_DATE_PRISE_EN_CHARGE,
                                                            TD_DEMANDE_ORIGINEID,
                                                            TD_DUREE_PRODUCTION_REEL,
                                                            TD_GUID,
                                                            TD_INFO_RETOUR_TRAITEMENT,
                                                            TD_PREREQUIS_DELAI_CHK,
                                                            TD_QUALIF_BILAN,
                                                            TD_QUALIF_EXIST_FILE,
                                                            TD_QUALIF_FILE_SIZE,
                                                            TD_RESULT_EXIST_FILE,
                                                            TD_RESULT_FILE_SIZE,
                                                            TD_RESULT_NB_DOWNLOAD,
                                                            TD_SEND_MAIL_CLIENT,
                                                            TD_SEND_MAIL_GESTIONNAIRE,
                                                            TE_ETATID,
                                                            TPF_PLANIF_ORIGINEID,
                                                            TRST_STATUTID,
                                                            TRU_DEMANDEURID,
                                                            TRU_GESTIONNAIRE_VALIDEURID,
                                                            TSRV_SERVEURID,
                                                            TS_SCENARIOID,
                                                            TD_IGNORE_RESULT,
                                                            TD_SUSPEND_EXECUTION
                                                        )
                                                        SELECT TD_DEMANDEID,
                                                               TD_COMMENTAIRE_GESTIONNAIRE,
                                                               TD_COMMENTAIRE_UTILISATEUR,
                                                               TD_DATE_AVIS_GESTIONNAIRE,
                                                               TD_DATE_DEMANDE,
                                                               TD_DATE_DERNIER_DOWNLOAD,
                                                               TD_DATE_EXECUTION_SOUHAITEE,
                                                               TD_DATE_LIVRAISON,
                                                               TD_DATE_PRISE_EN_CHARGE,
                                                               TD_DEMANDE_ORIGINEID,
                                                               TD_DUREE_PRODUCTION_REEL,
                                                               TD_GUID,
                                                               TD_INFO_RETOUR_TRAITEMENT,
                                                               TD_PREREQUIS_DELAI_CHK,
                                                               TD_QUALIF_BILAN,
                                                               TD_QUALIF_EXIST_FILE,
                                                               TD_QUALIF_FILE_SIZE,
                                                               TD_RESULT_EXIST_FILE,
                                                               TD_RESULT_FILE_SIZE,
                                                               TD_RESULT_NB_DOWNLOAD,
                                                               TD_SEND_MAIL_CLIENT,
                                                               TD_SEND_MAIL_GESTIONNAIRE,
                                                               TE_ETATID,
                                                               TPF_PLANIF_ORIGINEID,
                                                               TRST_STATUTID,
                                                               TRU_DEMANDEURID,
                                                               TRU_GESTIONNAIRE_VALIDEURID,
                                                               TSRV_SERVEURID,
                                                               TS_SCENARIOID,
                                                               TD_IGNORE_RESULT,
                                                               TD_SUSPEND_EXECUTION
                                                          FROM sqlitestudio_temp_table5;

                                CREATE TABLE sqlitestudio_temp_table6 AS SELECT *
                                                                           FROM TPF_PLANIFS;

                                DROP TABLE TPF_PLANIFS;

                                CREATE TABLE TPF_PLANIFS (
                                    TPF_PLANIFID            INTEGER NOT NULL
                                                                    CONSTRAINT PK_TPF_PLANIFS PRIMARY KEY AUTOINCREMENT,
                                    TPF_CRON                TEXT,
                                    TPF_DATE_DEBUT          TEXT    NOT NULL,
                                    TPF_DATE_FIN            TEXT,
                                    TPF_DEMANDE_ORIGINEID   INTEGER NOT NULL,
                                    TPF_PREREQUIS_DELAI_CHK INTEGER,
                                    TPF_TIMEZONE_INFOID     TEXT    NOT NULL,
                                    TRST_STATUTID           TEXT    NOT NULL,
                                    TRU_DECLARANTID         TEXT,
                                    CONSTRAINT [FK_TPF_PLANIFS$TRST_STATUTS] FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) ON DELETE RESTRICT
                                );

                                INSERT INTO TPF_PLANIFS (
                                                            TPF_PLANIFID,
                                                            TPF_CRON,
                                                            TPF_DATE_DEBUT,
                                                            TPF_DATE_FIN,
                                                            TPF_DEMANDE_ORIGINEID,
                                                            TPF_PREREQUIS_DELAI_CHK,
                                                            TPF_TIMEZONE_INFOID,
                                                            TRST_STATUTID,
                                                            TRU_DECLARANTID
                                                        )
                                                        SELECT TPF_PLANIFID,
                                                               TPF_CRON,
                                                               TPF_DATE_DEBUT,
                                                               TPF_DATE_FIN,
                                                               TPF_DEMANDE_ORIGINEID,
                                                               TPF_PREREQUIS_DELAI_CHK,
                                                               TPF_TIMEZONE_INFOID,
                                                               TRST_STATUTID,
                                                               TRU_DECLARANTID
                                                          FROM sqlitestudio_temp_table6;

                                DROP TABLE sqlitestudio_temp_table6;

                                CREATE INDEX IX_TPF_PLANIFS_TPF_DEMANDE_ORIGINEID ON TPF_PLANIFS (
                                    ""TPF_DEMANDE_ORIGINEID""
                                );

                                CREATE INDEX IX_TPF_PLANIFS_TRST_STATUTID ON TPF_PLANIFS (
                                    ""TRST_STATUTID""
                                );

                                CREATE TABLE sqlitestudio_temp_table7 AS SELECT *
                                                                           FROM TDQ_DEMANDE_QUALIFS;

                                DROP TABLE TDQ_DEMANDE_QUALIFS;

                                CREATE TABLE TDQ_DEMANDE_QUALIFS (
                                    TDQ_DEMANDE_QUALIFID INTEGER NOT NULL
                                                                 CONSTRAINT PK_TDQ_DEMANDE_QUALIFS PRIMARY KEY AUTOINCREMENT,
                                    TD_DEMANDEID         INTEGER NOT NULL,
                                    TDQ_NUM_ORDRE        INTEGER,
                                    TDQ_CODE             TEXT,
                                    TDQ_NOM              TEXT,
                                    TDQ_VALEUR           INTEGER,
                                    TDQ_NATURE           TEXT,
                                    TDQ_DATASET          TEXT,
                                    TDQ_OBJECTIF         TEXT,
                                    TDQ_TYPOLOGIE        TEXT,
                                    TDQ_COMMENT          TEXT,
                                    TDQ_DATE_PROD        TEXT
                                );

                                INSERT INTO TDQ_DEMANDE_QUALIFS (
                                                                    TDQ_DEMANDE_QUALIFID,
                                                                    TD_DEMANDEID,
                                                                    TDQ_NUM_ORDRE,
                                                                    TDQ_CODE,
                                                                    TDQ_NOM,
                                                                    TDQ_VALEUR,
                                                                    TDQ_NATURE,
                                                                    TDQ_DATASET,
                                                                    TDQ_OBJECTIF,
                                                                    TDQ_TYPOLOGIE,
                                                                    TDQ_COMMENT,
                                                                    TDQ_DATE_PROD
                                                                )
                                                                SELECT TDQ_DEMANDE_QUALIFID,
                                                                       TD_DEMANDEID,
                                                                       TDQ_NUM_ORDRE,
                                                                       TDQ_CODE,
                                                                       TDQ_NOM,
                                                                       TDQ_VALEUR,
                                                                       TDQ_NATURE,
                                                                       TDQ_DATASET,
                                                                       TDQ_OBJECTIF,
                                                                       TDQ_TYPOLOGIE,
                                                                       TDQ_COMMENT,
                                                                       TDQ_DATE_PROD
                                                                  FROM sqlitestudio_temp_table7;

                                DROP TABLE sqlitestudio_temp_table7;

                                CREATE INDEX IX_TDQ_DEMANDE_QUALIFS_TD_DEMANDEID ON TDQ_DEMANDE_QUALIFS (
                                    ""TD_DEMANDEID""
                                );

                                CREATE TABLE sqlitestudio_temp_table8 AS SELECT *
                                                                           FROM TDP_DEMANDE_PROCESS;

                                DROP TABLE TDP_DEMANDE_PROCESS;

                                CREATE TABLE TDP_DEMANDE_PROCESS (
                                    TDP_DEMANDE_PROCESSID INTEGER NOT NULL
                                                                  CONSTRAINT PK_TDP_DEMANDE_PROCESS PRIMARY KEY AUTOINCREMENT,
                                    TD_DEMANDEID          INTEGER NOT NULL,
                                    TDP_NUM_ETAPE         INTEGER NOT NULL,
                                    TDP_ETAPE             TEXT,
                                    TDP_STATUT            TEXT,
                                    TDP_EXTRA_INFO        TEXT
                                );

                                INSERT INTO TDP_DEMANDE_PROCESS (
                                                                    TDP_DEMANDE_PROCESSID,
                                                                    TD_DEMANDEID,
                                                                    TDP_NUM_ETAPE,
                                                                    TDP_ETAPE,
                                                                    TDP_STATUT,
                                                                    TDP_EXTRA_INFO
                                                                )
                                                                SELECT TDP_DEMANDE_PROCESSID,
                                                                       TD_DEMANDEID,
                                                                       TDP_NUM_ETAPE,
                                                                       TDP_ETAPE,
                                                                       TDP_STATUT,
                                                                       TDP_EXTRA_INFO
                                                                  FROM sqlitestudio_temp_table8;

                                DROP TABLE sqlitestudio_temp_table8;

                                CREATE INDEX IX_TDP_DEMANDE_PROCESS_TD_DEMANDEID ON TDP_DEMANDE_PROCESS (
                                    ""TD_DEMANDEID""
                                );

                                CREATE TABLE sqlitestudio_temp_table9 AS SELECT *
                                                                           FROM TDC_DEMANDES_COMMANDES;

                                DROP TABLE TDC_DEMANDES_COMMANDES;

                                CREATE TABLE TDC_DEMANDES_COMMANDES (
                                    TDC_DEMANDES_COMMANDEID INTEGER NOT NULL
                                                                    CONSTRAINT PK_TDC_DEMANDES_COMMANDES PRIMARY KEY AUTOINCREMENT,
                                    TCMD_COMMANDEID         INTEGER NOT NULL,
                                    TD_DEMANDEID            INTEGER NOT NULL,
                                    CONSTRAINT FK_TDC_DEMANDES_COMMANDES_TCMD_COMMANDES_TCMD_COMMANDEID FOREIGN KEY (
                                        TCMD_COMMANDEID
                                    )
                                    REFERENCES TCMD_COMMANDES (TCMD_COMMANDEID) ON DELETE CASCADE
                                );

                                INSERT INTO TDC_DEMANDES_COMMANDES (
                                                                       TDC_DEMANDES_COMMANDEID,
                                                                       TCMD_COMMANDEID,
                                                                       TD_DEMANDEID
                                                                   )
                                                                   SELECT TDC_DEMANDES_COMMANDEID,
                                                                          TCMD_COMMANDEID,
                                                                          TD_DEMANDEID
                                                                     FROM sqlitestudio_temp_table9;

                                DROP TABLE sqlitestudio_temp_table9;

                                CREATE INDEX IX_TDC_DEMANDES_COMMANDES_TCMD_COMMANDEID ON TDC_DEMANDES_COMMANDES (
                                    ""TCMD_COMMANDEID""
                                );

                                CREATE INDEX IX_TDC_DEMANDES_COMMANDES_TD_DEMANDEID ON TDC_DEMANDES_COMMANDES (
                                    ""TD_DEMANDEID""
                                );

                                CREATE TABLE sqlitestudio_temp_table10 AS SELECT *
                                                                            FROM TPD_PREREQUIS_DEMANDES;

                                DROP TABLE TPD_PREREQUIS_DEMANDES;

                                CREATE TABLE TPD_PREREQUIS_DEMANDES (
                                    TPD_PREREQUIS_DEMANDEID INTEGER NOT NULL
                                                                    CONSTRAINT PK_TPD_PREREQUIS_DEMANDES PRIMARY KEY AUTOINCREMENT,
                                    TD_DEMANDEID            INTEGER NOT NULL,
                                    TEP_ETAT_PREREQUISID    INTEGER NOT NULL,
                                    TE_ETATID               INTEGER NOT NULL,
                                    TPD_DATE_LAST_CHECK     TEXT,
                                    TPD_DATE_VALIDATION     TEXT,
                                    TPD_MSG_LAST_CHECK      TEXT,
                                    TPD_NB_FILE_TROUVE      INTEGER,
                                    TPD_VALIDE              TEXT,
                                    CONSTRAINT [FK_TPD_PREREQUIS_DEMANDES$TEP_ETAT_PREREQUIS] FOREIGN KEY (
                                        TEP_ETAT_PREREQUISID
                                    )
                                    REFERENCES TEP_ETAT_PREREQUISS (TEP_ETAT_PREREQUISID) ON DELETE RESTRICT
                                );

                                INSERT INTO TPD_PREREQUIS_DEMANDES (
                                                                       TPD_PREREQUIS_DEMANDEID,
                                                                       TD_DEMANDEID,
                                                                       TEP_ETAT_PREREQUISID,
                                                                       TE_ETATID,
                                                                       TPD_DATE_LAST_CHECK,
                                                                       TPD_DATE_VALIDATION,
                                                                       TPD_MSG_LAST_CHECK,
                                                                       TPD_NB_FILE_TROUVE,
                                                                       TPD_VALIDE
                                                                   )
                                                                   SELECT TPD_PREREQUIS_DEMANDEID,
                                                                          TD_DEMANDEID,
                                                                          TEP_ETAT_PREREQUISID,
                                                                          TE_ETATID,
                                                                          TPD_DATE_LAST_CHECK,
                                                                          TPD_DATE_VALIDATION,
                                                                          TPD_MSG_LAST_CHECK,
                                                                          TPD_NB_FILE_TROUVE,
                                                                          TPD_VALIDE
                                                                     FROM sqlitestudio_temp_table10;

                                DROP TABLE sqlitestudio_temp_table10;

                                CREATE INDEX IX_TPD_PREREQUIS_DEMANDES ON TPD_PREREQUIS_DEMANDES (
                                    ""TD_DEMANDEID"",
                                    ""TE_ETATID""
                                );

                                CREATE INDEX IX_TPD_PREREQUIS_DEMANDES_TEP_ETAT_PREREQUISID ON TPD_PREREQUIS_DEMANDES (
                                    ""TEP_ETAT_PREREQUISID""
                                );

                                CREATE UNIQUE INDEX UK_TPD_PREREQUIS_DEMANDES ON TPD_PREREQUIS_DEMANDES (
                                    ""TD_DEMANDEID"",
                                    ""TEP_ETAT_PREREQUISID""
                                );

                                CREATE TABLE sqlitestudio_temp_table11 AS SELECT *
                                                                            FROM TBD_BATCH_DEMANDES;

                                DROP TABLE TBD_BATCH_DEMANDES;

                                CREATE TABLE TBD_BATCH_DEMANDES (
                                    TBD_BATCH_DEMANDEID      INTEGER NOT NULL
                                                                     CONSTRAINT PK_TBD_BATCH_DEMANDES PRIMARY KEY AUTOINCREMENT,
                                    TD_DEMANDEID             INTEGER NOT NULL,
                                    TE_ETATID                INTEGER NOT NULL,
                                    TBD_ORDRE_EXECUTION      INTEGER NOT NULL,
                                    TBD_EXECUTION            TEXT,
                                    TBD_CODE_RETOUR          INTEGER,
                                    TEB_ETAT_BATCHID         INTEGER NOT NULL,
                                    TBD_DATE_DEBUT_EXECUTION TEXT,
                                    TBD_DATE_FIN_EXECUTION   TEXT,
                                    CONSTRAINT [FK_TBD_BATCH_DEMANDES$TEB_ETAT_BATCHS] FOREIGN KEY (
                                        TEB_ETAT_BATCHID
                                    )
                                    REFERENCES TEB_ETAT_BATCHS (TEB_ETAT_BATCHID) ON DELETE RESTRICT
                                );

                                INSERT INTO TBD_BATCH_DEMANDES (
                                                                   TBD_BATCH_DEMANDEID,
                                                                   TD_DEMANDEID,
                                                                   TE_ETATID,
                                                                   TBD_ORDRE_EXECUTION,
                                                                   TBD_EXECUTION,
                                                                   TBD_CODE_RETOUR,
                                                                   TEB_ETAT_BATCHID,
                                                                   TBD_DATE_DEBUT_EXECUTION,
                                                                   TBD_DATE_FIN_EXECUTION
                                                               )
                                                               SELECT TBD_BATCH_DEMANDEID,
                                                                      TD_DEMANDEID,
                                                                      TE_ETATID,
                                                                      TBD_ORDRE_EXECUTION,
                                                                      TBD_EXECUTION,
                                                                      TBD_CODE_RETOUR,
                                                                      TEB_ETAT_BATCHID,
                                                                      TBD_DATE_DEBUT_EXECUTION,
                                                                      TBD_DATE_FIN_EXECUTION
                                                                 FROM sqlitestudio_temp_table11;

                                DROP TABLE sqlitestudio_temp_table11;

                                CREATE UNIQUE INDEX [IX_TBD_BATCH_DEMANDE$TD_DEMANDES] ON TBD_BATCH_DEMANDES (
                                    ""TD_DEMANDEID"",
                                    ""TE_ETATID"",
                                    ""TBD_ORDRE_EXECUTION""
                                );

                                CREATE INDEX [IX_TBD_BATCH_DEMANDES$ETAT_BATCH] ON TBD_BATCH_DEMANDES (
                                    ""TE_ETATID"",
                                    ""TBD_ORDRE_EXECUTION""
                                );

                                CREATE INDEX [IX_TBD_BATCH_DEMANDES$TD_DEMANDEID] ON TBD_BATCH_DEMANDES (
                                    ""TD_DEMANDEID""
                                );

                                CREATE INDEX IX_TBD_BATCH_DEMANDES_TEB_ETAT_BATCHID ON TBD_BATCH_DEMANDES (
                                    ""TEB_ETAT_BATCHID""
                                );

                                CREATE TABLE sqlitestudio_temp_table12 AS SELECT *
                                                                            FROM TRD_RESSOURCE_DEMANDES;

                                DROP TABLE TRD_RESSOURCE_DEMANDES;

                                CREATE TABLE TRD_RESSOURCE_DEMANDES (
                                    TRD_RESSOURCE_DEMANDEID  INTEGER NOT NULL
                                                                     CONSTRAINT PK_TRD_RESSOURCE_DEMANDES PRIMARY KEY AUTOINCREMENT,
                                    TE_ETATID                INTEGER NOT NULL,
                                    TD_DEMANDEID             INTEGER NOT NULL,
                                    TER_ETAT_RESSOURCEID     INTEGER NOT NULL,
                                    TRD_NOM_FICHIER          TEXT    NOT NULL,
                                    TRD_NOM_FICHIER_ORIGINAL TEXT,
                                    TRD_FICHIER_PRESENT      TEXT    DEFAULT ( ('N') ),
                                    TRD_COMMENTAIRE          TEXT,
                                    TRD_TAILLE_FICHIER       INTEGER,
                                    CONSTRAINT [FK_TRD_RESSOURCE_DEMANDES$TER_ETAT_RESSOURCES] FOREIGN KEY (
                                        TER_ETAT_RESSOURCEID
                                    )
                                    REFERENCES TER_ETAT_RESSOURCES (TER_ETAT_RESSOURCEID) ON DELETE RESTRICT
                                );

                                INSERT INTO TRD_RESSOURCE_DEMANDES (
                                                                       TRD_RESSOURCE_DEMANDEID,
                                                                       TE_ETATID,
                                                                       TD_DEMANDEID,
                                                                       TER_ETAT_RESSOURCEID,
                                                                       TRD_NOM_FICHIER,
                                                                       TRD_NOM_FICHIER_ORIGINAL,
                                                                       TRD_FICHIER_PRESENT,
                                                                       TRD_COMMENTAIRE,
                                                                       TRD_TAILLE_FICHIER
                                                                   )
                                                                   SELECT TRD_RESSOURCE_DEMANDEID,
                                                                          TE_ETATID,
                                                                          TD_DEMANDEID,
                                                                          TER_ETAT_RESSOURCEID,
                                                                          TRD_NOM_FICHIER,
                                                                          TRD_NOM_FICHIER_ORIGINAL,
                                                                          TRD_FICHIER_PRESENT,
                                                                          TRD_COMMENTAIRE,
                                                                          TRD_TAILLE_FICHIER
                                                                     FROM sqlitestudio_temp_table12;

                                DROP TABLE sqlitestudio_temp_table12;

                                CREATE INDEX [IX_TRD_RESSOURCE_DEMANDES$ETAT_RESSOURCE] ON TRD_RESSOURCE_DEMANDES (
                                    ""TD_DEMANDEID"",
                                    ""TE_ETATID"",
                                    ""TRD_NOM_FICHIER""
                                );

                                CREATE INDEX [IX_TRD_RESSOURCE_DEMANDES$TE_ETATID] ON TRD_RESSOURCE_DEMANDES (
                                    ""TE_ETATID""
                                );

                                CREATE INDEX IX_TRD_RESSOURCE_DEMANDES_TER_ETAT_RESSOURCEID ON TRD_RESSOURCE_DEMANDES (
                                    ""TER_ETAT_RESSOURCEID""
                                );

                                DROP TABLE sqlitestudio_temp_table5;

                                CREATE INDEX [IX_TD_DEMANDES$ETAT] ON TD_DEMANDES (
                                    ""TE_ETATID""
                                );

                                CREATE INDEX [IX_TD_DEMANDES$NUM_CP] ON TD_DEMANDES (
                                    ""TRU_DEMANDEURID""
                                );

                                CREATE INDEX [IX_TD_DEMANDES$SERVEUR] ON TD_DEMANDES (
                                    ""TSRV_SERVEURID""
                                );

                                CREATE UNIQUE INDEX UK_TD_DEMANDES_TD_DEMANDEID_TE_ETATID ON TD_DEMANDES (
                                    ""TD_DEMANDEID"",
                                    ""TE_ETATID""
                                );

                                CREATE UNIQUE INDEX UQ_TD_DEMANDES ON TD_DEMANDES (
                                    ""TD_DEMANDEID"",
                                    ""TE_ETATID""
                                );

                                CREATE TABLE sqlitestudio_temp_table13 AS SELECT *
                                                                            FROM TS_SCENARIOS;

                                DROP TABLE TS_SCENARIOS;

                                CREATE TABLE TS_SCENARIOS (
                                    TS_SCENARIOID     INTEGER NOT NULL
                                                              CONSTRAINT PK_TS_SCENARIOS PRIMARY KEY AUTOINCREMENT,
                                    TE_ETATID         INTEGER NOT NULL,
                                    TRST_STATUTID     TEXT    NOT NULL,
                                    TS_DESCR          TEXT,
                                    TS_GUID           TEXT,
                                    TS_IS_DEFAULT     TEXT,
                                    TS_NOM_SCENARIO   TEXT    NOT NULL,
                                    TRST_STATUTID_OLD TEXT,
                                    CONSTRAINT FK_TS_SCENARIOS_TRST_STATUTS_TRST_STATUTID FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) ON DELETE CASCADE
                                );

                                INSERT INTO TS_SCENARIOS (
                                                             TS_SCENARIOID,
                                                             TE_ETATID,
                                                             TRST_STATUTID,
                                                             TS_DESCR,
                                                             TS_GUID,
                                                             TS_IS_DEFAULT,
                                                             TS_NOM_SCENARIO,
                                                             TRST_STATUTID_OLD
                                                         )
                                                         SELECT TS_SCENARIOID,
                                                                TE_ETATID,
                                                                TRST_STATUTID,
                                                                TS_DESCR,
                                                                TS_GUID,
                                                                TS_IS_DEFAULT,
                                                                TS_NOM_SCENARIO,
                                                                TRST_STATUTID_OLD
                                                           FROM sqlitestudio_temp_table13;

                                CREATE TABLE sqlitestudio_temp_table14 AS SELECT *
                                                                            FROM TRS_RESSOURCE_SCENARIOS;

                                DROP TABLE TRS_RESSOURCE_SCENARIOS;

                                CREATE TABLE TRS_RESSOURCE_SCENARIOS (
                                    TER_ETAT_RESSOURCEID    INTEGER NOT NULL,
                                    TS_SCENARIOID           INTEGER NOT NULL,
                                    TRS_COMMENTAIRE         TEXT,
                                    TRS_FICHIER_OBLIGATOIRE TEXT    NOT NULL,
                                    CONSTRAINT PK_TRS_RESSOURCE_SCENARIOS PRIMARY KEY (
                                        TER_ETAT_RESSOURCEID,
                                        TS_SCENARIOID
                                    ),
                                    CONSTRAINT [FK_TRS_RESSOURCE_SCENARIOS$TER_ETAT_RESSOURCES] FOREIGN KEY (
                                        TER_ETAT_RESSOURCEID
                                    )
                                    REFERENCES TER_ETAT_RESSOURCES (TER_ETAT_RESSOURCEID) ON DELETE RESTRICT
                                );

                                INSERT INTO TRS_RESSOURCE_SCENARIOS (
                                                                        TER_ETAT_RESSOURCEID,
                                                                        TS_SCENARIOID,
                                                                        TRS_COMMENTAIRE,
                                                                        TRS_FICHIER_OBLIGATOIRE
                                                                    )
                                                                    SELECT TER_ETAT_RESSOURCEID,
                                                                           TS_SCENARIOID,
                                                                           TRS_COMMENTAIRE,
                                                                           TRS_FICHIER_OBLIGATOIRE
                                                                      FROM sqlitestudio_temp_table14;

                                DROP TABLE sqlitestudio_temp_table14;

                                CREATE INDEX [IX_TRS_RESSOURCE_SCENARIOS$ETAT_RESSOURCE] ON TRS_RESSOURCE_SCENARIOS (
                                    ""TER_ETAT_RESSOURCEID""
                                );

                                CREATE INDEX [IX_TRS_RESSOURCE_SCENARIOS$TS_SCENARIOS] ON TRS_RESSOURCE_SCENARIOS (
                                    ""TS_SCENARIOID""
                                );

                                CREATE TABLE sqlitestudio_temp_table15 AS SELECT *
                                                                            FROM TBS_BATCH_SCENARIOS;

                                DROP TABLE TBS_BATCH_SCENARIOS;

                                CREATE TABLE TBS_BATCH_SCENARIOS (
                                    TEB_ETAT_BATCHID    INTEGER NOT NULL,
                                    TS_SCENARIOID       INTEGER NOT NULL,
                                    TBS_ORDRE_EXECUTION INTEGER NOT NULL,
                                    CONSTRAINT PK_TBS_BATCH_SCENARIOS PRIMARY KEY (
                                        TEB_ETAT_BATCHID,
                                        TS_SCENARIOID
                                    ),
                                    CONSTRAINT [FK_TBS_BATCH_SCENARIOS$TEB_ETAT_BATCHS] FOREIGN KEY (
                                        TEB_ETAT_BATCHID
                                    )
                                    REFERENCES TEB_ETAT_BATCHS (TEB_ETAT_BATCHID) ON DELETE RESTRICT
                                );

                                INSERT INTO TBS_BATCH_SCENARIOS (
                                                                    TEB_ETAT_BATCHID,
                                                                    TS_SCENARIOID,
                                                                    TBS_ORDRE_EXECUTION
                                                                )
                                                                SELECT TEB_ETAT_BATCHID,
                                                                       TS_SCENARIOID,
                                                                       TBS_ORDRE_EXECUTION
                                                                  FROM sqlitestudio_temp_table15;

                                DROP TABLE sqlitestudio_temp_table15;

                                CREATE INDEX [IX_TBS_BATCH_SCENARIOS$TBS_BATCH_SCENARIOS] ON TBS_BATCH_SCENARIOS (
                                    ""TEB_ETAT_BATCHID""
                                );

                                CREATE INDEX IX_TBS_BATCH_SCENARIOS_TS_SCENARIOID ON TBS_BATCH_SCENARIOS (
                                    ""TS_SCENARIOID""
                                );

                                CREATE TABLE sqlitestudio_temp_table16 AS SELECT *
                                                                            FROM TCMD_COMMANDES;

                                DROP TABLE TCMD_COMMANDES;

                                CREATE TABLE TCMD_COMMANDES (
                                    TCMD_COMMANDEID                    INTEGER NOT NULL
                                                                               CONSTRAINT PK_TCMD_COMMANDES PRIMARY KEY AUTOINCREMENT,
                                    TCMD_COMMENTAIRE                   TEXT,
                                    TCMD_DATE_CLOTURE                  TEXT,
                                    TCMD_DATE_CREATION                 TEXT    NOT NULL,
                                    TCMD_DATE_LIVRAISON                TEXT,
                                    TCMD_DATE_LIVRAISON_SOUHAITEE      TEXT,
                                    TCMD_DATE_MODIF                    TEXT    NOT NULL,
                                    TCMD_DATE_PASSAGE_CMD              TEXT,
                                    TCMD_DATE_PREVISIONNELLE_LIVRAISON TEXT,
                                    TCMD_DATE_PRISE_EN_CHARGE          TEXT,
                                    TCMD_MC_MODE_CREATIONID            INTEGER NOT NULL,
                                    TCMD_ORIGINEID                     INTEGER,
                                    TCMD_PH_PHASEID                    INTEGER NOT NULL,
                                    TDOM_DOMAINEID                     INTEGER,
                                    TE_ETATID                          INTEGER,
                                    TRU_COMMANDITAIREID                TEXT    NOT NULL,
                                    TRU_EXPLOITANTID                   TEXT,
                                    TS_SCENARIOID                      INTEGER,
                                    CONSTRAINT FK_TCMD_COMMANDES_TCMD_COMMANDES_TCMD_ORIGINEID FOREIGN KEY (
                                        TCMD_ORIGINEID
                                    )
                                    REFERENCES TCMD_COMMANDES (TCMD_COMMANDEID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_COMMANDES_TCMD_MC_MODE_CREATIONS_TCMD_MC_MODE_CREATIONID FOREIGN KEY (
                                        TCMD_MC_MODE_CREATIONID
                                    )
                                    REFERENCES TCMD_MC_MODE_CREATIONS (TCMD_MC_MODE_CREATIONID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_COMMANDES_TCMD_PH_PHASES_TCMD_PH_PHASEID FOREIGN KEY (
                                        TCMD_PH_PHASEID
                                    )
                                    REFERENCES TCMD_PH_PHASES (TCMD_PH_PHASEID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_COMMANDES_TE_ETATS_TE_ETATID FOREIGN KEY (
                                        TE_ETATID
                                    )
                                    REFERENCES TE_ETATS (TE_ETATID),
                                    CONSTRAINT FK_TCMD_COMMANDES_TRU_USERS_TRU_COMMANDITAIREID FOREIGN KEY (
                                        TRU_COMMANDITAIREID
                                    )
                                    REFERENCES TRU_USERS (TRU_USERID) ON DELETE RESTRICT,
                                    CONSTRAINT FK_TCMD_COMMANDES_TRU_USERS_TRU_EXPLOITANTID FOREIGN KEY (
                                        TRU_EXPLOITANTID
                                    )
                                    REFERENCES TRU_USERS (TRU_USERID) ON DELETE RESTRICT
                                );

                                INSERT INTO TCMD_COMMANDES (
                                                               TCMD_COMMANDEID,
                                                               TCMD_COMMENTAIRE,
                                                               TCMD_DATE_CLOTURE,
                                                               TCMD_DATE_CREATION,
                                                               TCMD_DATE_LIVRAISON,
                                                               TCMD_DATE_LIVRAISON_SOUHAITEE,
                                                               TCMD_DATE_MODIF,
                                                               TCMD_DATE_PASSAGE_CMD,
                                                               TCMD_DATE_PREVISIONNELLE_LIVRAISON,
                                                               TCMD_DATE_PRISE_EN_CHARGE,
                                                               TCMD_MC_MODE_CREATIONID,
                                                               TCMD_ORIGINEID,
                                                               TCMD_PH_PHASEID,
                                                               TDOM_DOMAINEID,
                                                               TE_ETATID,
                                                               TRU_COMMANDITAIREID,
                                                               TRU_EXPLOITANTID,
                                                               TS_SCENARIOID
                                                           )
                                                           SELECT TCMD_COMMANDEID,
                                                                  TCMD_COMMENTAIRE,
                                                                  TCMD_DATE_CLOTURE,
                                                                  TCMD_DATE_CREATION,
                                                                  TCMD_DATE_LIVRAISON,
                                                                  TCMD_DATE_LIVRAISON_SOUHAITEE,
                                                                  TCMD_DATE_MODIF,
                                                                  TCMD_DATE_PASSAGE_CMD,
                                                                  TCMD_DATE_PREVISIONNELLE_LIVRAISON,
                                                                  TCMD_DATE_PRISE_EN_CHARGE,
                                                                  TCMD_MC_MODE_CREATIONID,
                                                                  TCMD_ORIGINEID,
                                                                  TCMD_PH_PHASEID,
                                                                  TDOM_DOMAINEID,
                                                                  TE_ETATID,
                                                                  TRU_COMMANDITAIREID,
                                                                  TRU_EXPLOITANTID,
                                                                  TS_SCENARIOID
                                                             FROM sqlitestudio_temp_table16;

                                CREATE TABLE sqlitestudio_temp_table17 AS SELECT *
                                                                            FROM TCMD_DOC_DOCUMENTS;

                                DROP TABLE TCMD_DOC_DOCUMENTS;

                                CREATE TABLE TCMD_DOC_DOCUMENTS (
                                    TCMD_DOC_DOCUMENTID     INTEGER NOT NULL
                                                                    CONSTRAINT PK_TCMD_DOC_DOCUMENTS PRIMARY KEY AUTOINCREMENT,
                                    TCMD_DOC_FILENAME       TEXT    NOT NULL,
                                    TCMD_DOC_DATE           TEXT    NOT NULL,
                                    TCMD_DOC_COMMENTAIRE    TEXT    NOT NULL,
                                    TCMD_COMMANDEID         INTEGER NOT NULL,
                                    TCMD_TD_TYPE_DOCUMENTID INTEGER,
                                    TCMD_DOC_TAILLE         INTEGER,
                                    CONSTRAINT FK_TCMD_DOC_DOCUMENTS_TCMD_TD_TYPE_DOCUMENTS_TCMD_TD_TYPE_DOCUMENTID FOREIGN KEY (
                                        TCMD_TD_TYPE_DOCUMENTID
                                    )
                                    REFERENCES TCMD_TD_TYPE_DOCUMENTS (TCMD_TD_TYPE_DOCUMENTID) ON DELETE RESTRICT
                                );

                                INSERT INTO TCMD_DOC_DOCUMENTS (
                                                                   TCMD_DOC_DOCUMENTID,
                                                                   TCMD_DOC_FILENAME,
                                                                   TCMD_DOC_DATE,
                                                                   TCMD_DOC_COMMENTAIRE,
                                                                   TCMD_COMMANDEID,
                                                                   TCMD_TD_TYPE_DOCUMENTID,
                                                                   TCMD_DOC_TAILLE
                                                               )
                                                               SELECT TCMD_DOC_DOCUMENTID,
                                                                      TCMD_DOC_FILENAME,
                                                                      TCMD_DOC_DATE,
                                                                      TCMD_DOC_COMMENTAIRE,
                                                                      TCMD_COMMANDEID,
                                                                      TCMD_TD_TYPE_DOCUMENTID,
                                                                      TCMD_DOC_TAILLE
                                                                 FROM sqlitestudio_temp_table17;

                                DROP TABLE sqlitestudio_temp_table17;

                                CREATE UNIQUE INDEX IX_TCMD_DOC_DOCUMENTS ON TCMD_DOC_DOCUMENTS (
                                    ""TCMD_COMMANDEID"",
                                    ""TCMD_DOC_FILENAME""
                                );

                                CREATE INDEX IX_TCMD_DOC_DOCUMENTS_TCMD_TD_TYPE_DOCUMENTID ON TCMD_DOC_DOCUMENTS (
                                    ""TCMD_TD_TYPE_DOCUMENTID""
                                );

                                DROP TABLE sqlitestudio_temp_table16;

                                CREATE INDEX IX_TCMD_COMMANDES_TCMD_MC_MODE_CREATIONID ON TCMD_COMMANDES (
                                    ""TCMD_MC_MODE_CREATIONID""
                                );

                                CREATE INDEX IX_TCMD_COMMANDES_TCMD_ORIGINEID ON TCMD_COMMANDES (
                                    ""TCMD_ORIGINEID""
                                );

                                CREATE INDEX IX_TCMD_COMMANDES_TCMD_PH_PHASEID ON TCMD_COMMANDES (
                                    ""TCMD_PH_PHASEID""
                                );

                                CREATE INDEX IX_TCMD_COMMANDES_TE_ETATID ON TCMD_COMMANDES (
                                    ""TE_ETATID""
                                );

                                CREATE INDEX IX_TCMD_COMMANDES_TRU_COMMANDITAIREID ON TCMD_COMMANDES (
                                    ""TRU_COMMANDITAIREID""
                                );

                                CREATE INDEX IX_TCMD_COMMANDES_TRU_EXPLOITANTID ON TCMD_COMMANDES (
                                    ""TRU_EXPLOITANTID""
                                );

                                CREATE INDEX IX_TCMD_COMMANDES_TS_SCENARIOID ON TCMD_COMMANDES (
                                    ""TS_SCENARIOID""
                                );

                                CREATE TABLE sqlitestudio_temp_table18 AS SELECT *
                                                                            FROM TPS_PREREQUIS_SCENARIOS;

                                DROP TABLE TPS_PREREQUIS_SCENARIOS;

                                CREATE TABLE TPS_PREREQUIS_SCENARIOS (
                                    TEP_ETAT_PREREQUISID INTEGER NOT NULL,
                                    TS_SCENARIOID        INTEGER NOT NULL,
                                    TPS_NB_FILE_MIN      INTEGER,
                                    TPS_NB_FILE_MAX      INTEGER,
                                    TPS_COMMENTAIRE      TEXT,
                                    CONSTRAINT PK_TPS_PREREQUIS_SCENARIOS PRIMARY KEY (
                                        TEP_ETAT_PREREQUISID,
                                        TS_SCENARIOID
                                    ),
                                    CONSTRAINT [FK_TPS_PREREQUIS_SCENARIOS$TEP_ETAT_PREREQUIS] FOREIGN KEY (
                                        TEP_ETAT_PREREQUISID
                                    )
                                    REFERENCES TEP_ETAT_PREREQUISS (TEP_ETAT_PREREQUISID) ON DELETE RESTRICT
                                );

                                INSERT INTO TPS_PREREQUIS_SCENARIOS (
                                                                        TEP_ETAT_PREREQUISID,
                                                                        TS_SCENARIOID,
                                                                        TPS_NB_FILE_MIN,
                                                                        TPS_NB_FILE_MAX,
                                                                        TPS_COMMENTAIRE
                                                                    )
                                                                    SELECT TEP_ETAT_PREREQUISID,
                                                                           TS_SCENARIOID,
                                                                           TPS_NB_FILE_MIN,
                                                                           TPS_NB_FILE_MAX,
                                                                           TPS_COMMENTAIRE
                                                                      FROM sqlitestudio_temp_table18;

                                DROP TABLE sqlitestudio_temp_table18;

                                CREATE INDEX IX_TPS_PREREQUIS_SCENARIOS_TS_SCENARIOID ON TPS_PREREQUIS_SCENARIOS (
                                    ""TS_SCENARIOID""
                                );

                                CREATE TABLE sqlitestudio_temp_table19 AS SELECT *
                                                                            FROM TH_HABILITATIONS;

                                DROP TABLE TH_HABILITATIONS;

                                CREATE TABLE TH_HABILITATIONS (
                                    TH_HABILITATIONID           INTEGER NOT NULL
                                                                        CONSTRAINT PK_TH_HABILITATIONS PRIMARY KEY AUTOINCREMENT,
                                    TH_COMMENTAIRE              TEXT,
                                    TH_DATE_INITIALISATION      TEXT    NOT NULL,
                                    TH_DROIT_CONCERNE           TEXT    NOT NULL,
                                    TH_EST_HABILITE             INTEGER NOT NULL,
                                    TH_HERITE_HABILITATIONID    INTEGER,
                                    TH_MAJ_DATE                 TEXT    NOT NULL,
                                    TRST_STATUTID               TEXT    NOT NULL,
                                    TRU_INITIALISATION_AUTEURID TEXT    NOT NULL,
                                    TRU_MAJ_AUTEURID            TEXT    NOT NULL,
                                    TRU_USERID                  TEXT,
                                    TSG_SCENARIO_GPEID          INTEGER,
                                    TS_SCENARIOID               INTEGER,
                                    TTE_TEAMID                  INTEGER,
                                    CONSTRAINT FK_TH_HABILITATIONS_TSG_SCENARIO_GPES_TSG_SCENARIO_GPEID FOREIGN KEY (
                                        TSG_SCENARIO_GPEID
                                    )
                                    REFERENCES TSG_SCENARIO_GPES (TSG_SCENARIO_GPEID),
                                    CONSTRAINT [FK_TH_HABILITATIONS$TRST_STATUTS] FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID),
                                    CONSTRAINT [FK_TH_HABILITATIONS$TRU_USERS] FOREIGN KEY (
                                        TRU_USERID
                                    )
                                    REFERENCES TRU_USERS (TRU_USERID),
                                    CONSTRAINT [FK_TH_HABILITATIONS$TRU_USERS$INITIALISATION_AUTEUR] FOREIGN KEY (
                                        TRU_INITIALISATION_AUTEURID
                                    )
                                    REFERENCES TRU_USERS (TRU_USERID),
                                    CONSTRAINT [FK_TH_HABILITATIONS$TRU_USERS$MAJ_AUTEUR] FOREIGN KEY (
                                        TRU_MAJ_AUTEURID
                                    )
                                    REFERENCES TRU_USERS (TRU_USERID),
                                    CONSTRAINT [FK_TH_HABILITATIONS$TTE_TEAMS] FOREIGN KEY (
                                        TTE_TEAMID
                                    )
                                    REFERENCES TTE_TEAMS (TTE_TEAMID) 
                                );

                                INSERT INTO TH_HABILITATIONS (
                                                                 TH_HABILITATIONID,
                                                                 TH_COMMENTAIRE,
                                                                 TH_DATE_INITIALISATION,
                                                                 TH_DROIT_CONCERNE,
                                                                 TH_EST_HABILITE,
                                                                 TH_HERITE_HABILITATIONID,
                                                                 TH_MAJ_DATE,
                                                                 TRST_STATUTID,
                                                                 TRU_INITIALISATION_AUTEURID,
                                                                 TRU_MAJ_AUTEURID,
                                                                 TRU_USERID,
                                                                 TSG_SCENARIO_GPEID,
                                                                 TS_SCENARIOID,
                                                                 TTE_TEAMID
                                                             )
                                                             SELECT TH_HABILITATIONID,
                                                                    TH_COMMENTAIRE,
                                                                    TH_DATE_INITIALISATION,
                                                                    TH_DROIT_CONCERNE,
                                                                    TH_EST_HABILITE,
                                                                    TH_HERITE_HABILITATIONID,
                                                                    TH_MAJ_DATE,
                                                                    TRST_STATUTID,
                                                                    TRU_INITIALISATION_AUTEURID,
                                                                    TRU_MAJ_AUTEURID,
                                                                    TRU_USERID,
                                                                    TSG_SCENARIO_GPEID,
                                                                    TS_SCENARIOID,
                                                                    TTE_TEAMID
                                                               FROM sqlitestudio_temp_table19;

                                DROP TABLE sqlitestudio_temp_table19;

                                CREATE INDEX IX_TH_HABILITATIONS_TRST_STATUTID ON TH_HABILITATIONS (
                                    ""TRST_STATUTID""
                                );

                                CREATE INDEX IX_TH_HABILITATIONS_TRU_INITIALISATION_AUTEURID ON TH_HABILITATIONS (
                                    ""TRU_INITIALISATION_AUTEURID""
                                );

                                CREATE INDEX IX_TH_HABILITATIONS_TRU_MAJ_AUTEURID ON TH_HABILITATIONS (
                                    ""TRU_MAJ_AUTEURID""
                                );

                                CREATE INDEX IX_TH_HABILITATIONS_TSG_SCENARIO_GPEID ON TH_HABILITATIONS (
                                    ""TSG_SCENARIO_GPEID""
                                );

                                CREATE INDEX IX_TH_HABILITATIONS_TS_SCENARIOID ON TH_HABILITATIONS (
                                    ""TS_SCENARIOID""
                                );

                                CREATE INDEX IX_TH_HABILITATIONS_TTE_TEAMID ON TH_HABILITATIONS (
                                    ""TTE_TEAMID""
                                );

                                CREATE INDEX TH_HABILITATIONS_IDX1 ON TH_HABILITATIONS (
                                    ""TRU_USERID"",
                                    ""TS_SCENARIOID""
                                );

                                CREATE TABLE sqlitestudio_temp_table20 AS SELECT *
                                                                            FROM TSGA_SCENARIO_GPE_ASSOCIES;

                                DROP TABLE TSGA_SCENARIO_GPE_ASSOCIES;

                                CREATE TABLE TSGA_SCENARIO_GPE_ASSOCIES (
                                    TSGA_SCENARIO_GPE_ASSOCIEID INTEGER NOT NULL
                                                                        CONSTRAINT PK_TSGA_SCENARIO_GPE_ASSOCIES PRIMARY KEY AUTOINCREMENT,
                                    TSG_SCENARIO_GPEID          INTEGER NOT NULL,
                                    TS_SCENARIOID               INTEGER NOT NULL,
                                    TSGA_COMMENTAIRE            TEXT,
                                    TSGA_DATE_CREATION          TEXT    NOT NULL,
                                    TRST_STATUTID               TEXT    NOT NULL,
                                    CONSTRAINT [FK_TSGA_SCENARIO_GPE_ASSOCIES$TRST_STATUTS] FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID),
                                    CONSTRAINT [FK_TSGA_SCENARIO_GPE_ASSOCIES$TSG_SCENARIO_GPES] FOREIGN KEY (
                                        TSG_SCENARIO_GPEID
                                    )
                                    REFERENCES TSG_SCENARIO_GPES (TSG_SCENARIO_GPEID) 
                                );

                                INSERT INTO TSGA_SCENARIO_GPE_ASSOCIES (
                                                                           TSGA_SCENARIO_GPE_ASSOCIEID,
                                                                           TSG_SCENARIO_GPEID,
                                                                           TS_SCENARIOID,
                                                                           TSGA_COMMENTAIRE,
                                                                           TSGA_DATE_CREATION,
                                                                           TRST_STATUTID
                                                                       )
                                                                       SELECT TSGA_SCENARIO_GPE_ASSOCIEID,
                                                                              TSG_SCENARIO_GPEID,
                                                                              TS_SCENARIOID,
                                                                              TSGA_COMMENTAIRE,
                                                                              TSGA_DATE_CREATION,
                                                                              TRST_STATUTID
                                                                         FROM sqlitestudio_temp_table20;

                                DROP TABLE sqlitestudio_temp_table20;

                                CREATE INDEX IX_TSGA_SCENARIO_GPE_ASSOCIES_TRST_STATUTID ON TSGA_SCENARIO_GPE_ASSOCIES (
                                    ""TRST_STATUTID""
                                );

                                CREATE INDEX IX_TSGA_SCENARIO_GPE_ASSOCIES_TS_SCENARIOID ON TSGA_SCENARIO_GPE_ASSOCIES (
                                    ""TS_SCENARIOID""
                                );

                                CREATE UNIQUE INDEX UQ_TSGA_SCENARIO_GPE_ASSOCIES ON TSGA_SCENARIO_GPE_ASSOCIES (
                                    ""TSG_SCENARIO_GPEID"",
                                    ""TS_SCENARIOID""
                                );

                                DROP TABLE sqlitestudio_temp_table13;

                                CREATE INDEX IX_TS_SCENARIOS_TE_ETATID ON TS_SCENARIOS (
                                    ""TE_ETATID""
                                );

                                CREATE INDEX IX_TS_SCENARIOS_TRST_STATUTID ON TS_SCENARIOS (
                                    ""TRST_STATUTID""
                                );

                                CREATE TABLE sqlitestudio_temp_table21 AS SELECT *
                                                                            FROM TEB_ETAT_BATCHS;

                                DROP TABLE TEB_ETAT_BATCHS;

                                CREATE TABLE TEB_ETAT_BATCHS (
                                    TEB_ETAT_BATCHID  INTEGER NOT NULL
                                                              CONSTRAINT PK_TEB_ETAT_BATCHS PRIMARY KEY AUTOINCREMENT,
                                    TEB_CMD           TEXT    NOT NULL,
                                    TEB_DATE_CREATION TEXT    NOT NULL,
                                    TEB_DESCR         TEXT,
                                    TEB_NOM_AFFICHAGE TEXT,
                                    TE_ETATID         INTEGER NOT NULL,
                                    TRST_STATUTID     TEXT    NOT NULL,
                                    CONSTRAINT FK_TEB_ETAT_BATCHS_TRST_STATUTS_TRST_STATUTID FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) ON DELETE CASCADE
                                );

                                INSERT INTO TEB_ETAT_BATCHS (
                                                                TEB_ETAT_BATCHID,
                                                                TEB_CMD,
                                                                TEB_DATE_CREATION,
                                                                TEB_DESCR,
                                                                TEB_NOM_AFFICHAGE,
                                                                TE_ETATID,
                                                                TRST_STATUTID
                                                            )
                                                            SELECT TEB_ETAT_BATCHID,
                                                                   TEB_CMD,
                                                                   TEB_DATE_CREATION,
                                                                   TEB_DESCR,
                                                                   TEB_NOM_AFFICHAGE,
                                                                   TE_ETATID,
                                                                   TRST_STATUTID
                                                              FROM sqlitestudio_temp_table21;

                                DROP TABLE sqlitestudio_temp_table21;

                                CREATE INDEX [IX_TEB_ETAT_BATCHS$TE_ETATS] ON TEB_ETAT_BATCHS (
                                    ""TE_ETATID""
                                );

                                CREATE INDEX [IX_TEB_ETAT_BATCHS$TE_ETAT_BATCHS] ON TEB_ETAT_BATCHS (
                                    ""TEB_ETAT_BATCHID""
                                );

                                CREATE INDEX IX_TEB_ETAT_BATCHS_TRST_STATUTID ON TEB_ETAT_BATCHS (
                                    ""TRST_STATUTID""
                                );

                                CREATE TABLE sqlitestudio_temp_table22 AS SELECT *
                                                                            FROM TEP_ETAT_PREREQUISS;

                                DROP TABLE TEP_ETAT_PREREQUISS;

                                CREATE TABLE TEP_ETAT_PREREQUISS (
                                    TEP_ETAT_PREREQUISID INTEGER NOT NULL
                                                                 CONSTRAINT PK_TEP_ETAT_PREREQUISS PRIMARY KEY AUTOINCREMENT,
                                    TE_ETATID            INTEGER NOT NULL,
                                    TEP_DATE_MAJ         TEXT,
                                    TRST_STATUTID        TEXT    NOT NULL,
                                    TEP_NATURE           TEXT    NOT NULL,
                                    TEP_PATH             TEXT    NOT NULL,
                                    TEP_FILEPATTERN      TEXT    NOT NULL,
                                    TEP_IS_PATTERN       TEXT    NOT NULL,
                                    TEP_COMMENTAIRE      TEXT,
                                    CONSTRAINT [FK_TEP_ETAT_PREREQUISS$TRST_STATUTS] FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) ON DELETE RESTRICT
                                );

                                INSERT INTO TEP_ETAT_PREREQUISS (
                                                                    TEP_ETAT_PREREQUISID,
                                                                    TE_ETATID,
                                                                    TEP_DATE_MAJ,
                                                                    TRST_STATUTID,
                                                                    TEP_NATURE,
                                                                    TEP_PATH,
                                                                    TEP_FILEPATTERN,
                                                                    TEP_IS_PATTERN,
                                                                    TEP_COMMENTAIRE
                                                                )
                                                                SELECT TEP_ETAT_PREREQUISID,
                                                                       TE_ETATID,
                                                                       TEP_DATE_MAJ,
                                                                       TRST_STATUTID,
                                                                       TEP_NATURE,
                                                                       TEP_PATH,
                                                                       TEP_FILEPATTERN,
                                                                       TEP_IS_PATTERN,
                                                                       TEP_COMMENTAIRE
                                                                  FROM sqlitestudio_temp_table22;

                                DROP TABLE sqlitestudio_temp_table22;

                                CREATE INDEX IX_TEP_ETAT_PREREQUISS_TE_ETATID ON TEP_ETAT_PREREQUISS (
                                    ""TE_ETATID""
                                );

                                CREATE INDEX IX_TEP_ETAT_PREREQUISS_TRST_STATUTID ON TEP_ETAT_PREREQUISS (
                                    ""TRST_STATUTID""
                                );

                                CREATE TABLE sqlitestudio_temp_table23 AS SELECT *
                                                                            FROM TER_ETAT_RESSOURCES;

                                DROP TABLE TER_ETAT_RESSOURCES;

                                CREATE TABLE TER_ETAT_RESSOURCES (
                                    TER_ETAT_RESSOURCEID INTEGER NOT NULL
                                                                 CONSTRAINT PK_TER_ETAT_RESSOURCES PRIMARY KEY AUTOINCREMENT,
                                    TE_ETATID            INTEGER NOT NULL,
                                    TER_NOM_FICHIER      TEXT    NOT NULL,
                                    TER_COMMENTAIRE      TEXT,
                                    TER_PATH_RELATIF     TEXT,
                                    TER_MODELE_DOC       TEXT,
                                    TER_MODELE_DATE      TEXT,
                                    TER_MODELE_TAILLE    INTEGER,
                                    TER_IS_PATTERN       TEXT    NOT NULL
                                                                 DEFAULT ( ('N') ),
                                    TER_NOM_MODELE       TEXT
                                );

                                INSERT INTO TER_ETAT_RESSOURCES (
                                                                    TER_ETAT_RESSOURCEID,
                                                                    TE_ETATID,
                                                                    TER_NOM_FICHIER,
                                                                    TER_COMMENTAIRE,
                                                                    TER_PATH_RELATIF,
                                                                    TER_MODELE_DOC,
                                                                    TER_MODELE_DATE,
                                                                    TER_MODELE_TAILLE,
                                                                    TER_IS_PATTERN,
                                                                    TER_NOM_MODELE
                                                                )
                                                                SELECT TER_ETAT_RESSOURCEID,
                                                                       TE_ETATID,
                                                                       TER_NOM_FICHIER,
                                                                       TER_COMMENTAIRE,
                                                                       TER_PATH_RELATIF,
                                                                       TER_MODELE_DOC,
                                                                       TER_MODELE_DATE,
                                                                       TER_MODELE_TAILLE,
                                                                       TER_IS_PATTERN,
                                                                       TER_NOM_MODELE
                                                                  FROM sqlitestudio_temp_table23;

                                DROP TABLE sqlitestudio_temp_table23;

                                CREATE INDEX [IX_TER_ETAT_RESSOURCES$ETATID] ON TER_ETAT_RESSOURCES (
                                    ""TE_ETATID""
                                );

                                CREATE UNIQUE INDEX [UK_TER_ETAT_RESSOURCES$ETATIDNOM_FICHIER] ON TER_ETAT_RESSOURCES (
                                    ""TE_ETATID"",
                                    ""TER_NOM_FICHIER""
                                );

                                CREATE TABLE sqlitestudio_temp_table24 AS SELECT *
                                                                            FROM TEL_ETAT_LOGICIELS;

                                DROP TABLE TEL_ETAT_LOGICIELS;

                                CREATE TABLE TEL_ETAT_LOGICIELS (
                                    TEL_ETAT_LOGICIELID INTEGER NOT NULL
                                                                CONSTRAINT PK_TEL_ETAT_LOGICIELS PRIMARY KEY AUTOINCREMENT,
                                    TE_ETATID           INTEGER NOT NULL,
                                    TL_LOGICIELID       INTEGER NOT NULL,
                                    CONSTRAINT [FK_TEL_ETAT_LOGICIELS$TL_LOGICIELS] FOREIGN KEY (
                                        TL_LOGICIELID
                                    )
                                    REFERENCES TL_LOGICIELS (TL_LOGICIELID) ON DELETE RESTRICT
                                );

                                INSERT INTO TEL_ETAT_LOGICIELS (
                                                                   TEL_ETAT_LOGICIELID,
                                                                   TE_ETATID,
                                                                   TL_LOGICIELID
                                                               )
                                                               SELECT TEL_ETAT_LOGICIELID,
                                                                      TE_ETATID,
                                                                      TL_LOGICIELID
                                                                 FROM sqlitestudio_temp_table24;

                                DROP TABLE sqlitestudio_temp_table24;

                                CREATE INDEX IX_TEL_ETAT_LOGICIELS_TL_LOGICIELID ON TEL_ETAT_LOGICIELS (
                                    ""TL_LOGICIELID""
                                );

                                CREATE UNIQUE INDEX UQ_TEL_ETAT_LOGICIELS ON TEL_ETAT_LOGICIELS (
                                    ""TE_ETATID"",
                                    ""TL_LOGICIELID""
                                );

                                DROP TABLE sqlitestudio_temp_table4;

                                CREATE INDEX [IX_TE_ETATS$TEM_ETAT_MASTERS] ON TE_ETATS (
                                    ""TEM_ETAT_MASTERID""
                                );

                                CREATE INDEX IX_TE_ETATS_TRST_STATUTID ON TE_ETATS (
                                    ""TRST_STATUTID""
                                );

                                CREATE INDEX IX_TE_ETATS_TRU_DECLARANTID ON TE_ETATS (
                                    ""TRU_DECLARANTID""
                                );

                                CREATE INDEX IX_TE_ETATS_TRU_ENV_VIERGE_AUTEURID ON TE_ETATS (
                                    ""TRU_ENV_VIERGE_AUTEURID""
                                );

                                CREATE TABLE sqlitestudio_temp_table25 AS SELECT *
                                                                            FROM TEM_ETAT_MASTERS;

                                DROP TABLE TEM_ETAT_MASTERS;

                                CREATE TABLE TEM_ETAT_MASTERS (
                                    TEM_ETAT_MASTERID             INTEGER NOT NULL
                                                                          CONSTRAINT PK_TEM_ETAT_MASTERS PRIMARY KEY AUTOINCREMENT,
                                    TC_CATEGORIEID                INTEGER NOT NULL,
                                    TEM_COMMENTAIRE               TEXT,
                                    TEM_DATE_CREATION             TEXT    NOT NULL,
                                    TEM_ETAT_MASTER_PARENTID      TEXT,
                                    TEM_GUID                      TEXT,
                                    TEM_NOM_ETAT_MASTER           TEXT    NOT NULL,
                                    TP_PERIMETREID                INTEGER NOT NULL,
                                    TRST_STATUTID                 TEXT    NOT NULL
                                                                          DEFAULT ( ('A') ),
                                    TRU_RESPONSABLE_FONCTIONNELID TEXT    NOT NULL,
                                    TRU_RESPONSABLE_TECHNIQUEID   TEXT,
                                    TRST_STATUTID_OLD             TEXT,
                                    CONSTRAINT FK_TEM_ETAT_MASTERS_TRST_STATUTS_TRST_STATUTID FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) ON DELETE CASCADE,
                                    CONSTRAINT [FK_TEM_ETAT_MASTERS$TC_CATEGORIES] FOREIGN KEY (
                                        TC_CATEGORIEID
                                    )
                                    REFERENCES TC_CATEGORIES (TC_CATEGORIEID) ON DELETE CASCADE,
                                    CONSTRAINT [FK_TEM_ETAT_MASTERS$TP_PERIMETRES] FOREIGN KEY (
                                        TP_PERIMETREID
                                    )
                                    REFERENCES TP_PERIMETRES (TP_PERIMETREID) ON DELETE RESTRICT
                                );

                                INSERT INTO TEM_ETAT_MASTERS (
                                                                 TEM_ETAT_MASTERID,
                                                                 TC_CATEGORIEID,
                                                                 TEM_COMMENTAIRE,
                                                                 TEM_DATE_CREATION,
                                                                 TEM_ETAT_MASTER_PARENTID,
                                                                 TEM_GUID,
                                                                 TEM_NOM_ETAT_MASTER,
                                                                 TP_PERIMETREID,
                                                                 TRST_STATUTID,
                                                                 TRU_RESPONSABLE_FONCTIONNELID,
                                                                 TRU_RESPONSABLE_TECHNIQUEID,
                                                                 TRST_STATUTID_OLD
                                                             )
                                                             SELECT TEM_ETAT_MASTERID,
                                                                    TC_CATEGORIEID,
                                                                    TEM_COMMENTAIRE,
                                                                    TEM_DATE_CREATION,
                                                                    TEM_ETAT_MASTER_PARENTID,
                                                                    TEM_GUID,
                                                                    TEM_NOM_ETAT_MASTER,
                                                                    TP_PERIMETREID,
                                                                    TRST_STATUTID,
                                                                    TRU_RESPONSABLE_FONCTIONNELID,
                                                                    TRU_RESPONSABLE_TECHNIQUEID,
                                                                    TRST_STATUTID_OLD
                                                               FROM sqlitestudio_temp_table25;

                                CREATE TABLE sqlitestudio_temp_table26 AS SELECT *
                                                                            FROM TEMF_ETAT_MASTER_FERMES;

                                DROP TABLE TEMF_ETAT_MASTER_FERMES;

                                CREATE TABLE TEMF_ETAT_MASTER_FERMES (
                                    TEMF_ETAT_MASTER_FERMEID INTEGER NOT NULL
                                                                     CONSTRAINT PK_TEMF_ETAT_MASTER_FERMES PRIMARY KEY AUTOINCREMENT,
                                    TEM_ETAT_MASTERID        INTEGER NOT NULL,
                                    TF_FERMEID               INTEGER NOT NULL,
                                    TEMF_DATE_AJOUT          TEXT,
                                    TEMF_DATE_SUPPRESSION    TEXT,
                                    TEMF_DESCR               TEXT,
                                    TEMF_ORDRE_PRIORITE      INTEGER DEFAULT ( ( (0) ) ),
                                    CONSTRAINT [FK_TEMF_ETAT_MASTER_FERMES$TF_FERMES] FOREIGN KEY (
                                        TF_FERMEID
                                    )
                                    REFERENCES TF_FERMES (TF_FERMEID) ON DELETE RESTRICT
                                );

                                INSERT INTO TEMF_ETAT_MASTER_FERMES (
                                                                        TEMF_ETAT_MASTER_FERMEID,
                                                                        TEM_ETAT_MASTERID,
                                                                        TF_FERMEID,
                                                                        TEMF_DATE_AJOUT,
                                                                        TEMF_DATE_SUPPRESSION,
                                                                        TEMF_DESCR,
                                                                        TEMF_ORDRE_PRIORITE
                                                                    )
                                                                    SELECT TEMF_ETAT_MASTER_FERMEID,
                                                                           TEM_ETAT_MASTERID,
                                                                           TF_FERMEID,
                                                                           TEMF_DATE_AJOUT,
                                                                           TEMF_DATE_SUPPRESSION,
                                                                           TEMF_DESCR,
                                                                           TEMF_ORDRE_PRIORITE
                                                                      FROM sqlitestudio_temp_table26;

                                DROP TABLE sqlitestudio_temp_table26;

                                CREATE INDEX IX_TEMF_ETAT_MASTER_FERMES_TF_FERMEID ON TEMF_ETAT_MASTER_FERMES (
                                    ""TF_FERMEID""
                                );

                                CREATE UNIQUE INDEX UK_TEMF_ETAT_MASTER_FERMES ON TEMF_ETAT_MASTER_FERMES (
                                    ""TEM_ETAT_MASTERID"",
                                    ""TF_FERMEID""
                                );

                                CREATE UNIQUE INDEX UQ_TEMF_ETAT_MASTER_FERMES ON TEMF_ETAT_MASTER_FERMES (
                                    ""TEM_ETAT_MASTERID"",
                                    ""TF_FERMEID""
                                );

                                DROP TABLE sqlitestudio_temp_table25;

                                CREATE INDEX [IX_TEM_ETAT_MASTERS$PERIMETRE] ON TEM_ETAT_MASTERS (
                                    ""TP_PERIMETREID""
                                );

                                CREATE INDEX IX_TEM_ETAT_MASTERS_TC_CATEGORIEID ON TEM_ETAT_MASTERS (
                                    ""TC_CATEGORIEID""
                                );

                                CREATE INDEX IX_TEM_ETAT_MASTERS_TRST_STATUTID ON TEM_ETAT_MASTERS (
                                    ""TRST_STATUTID""
                                );

                                CREATE INDEX IX_TEM_ETAT_MASTERS_TRU_RESPONSABLE_FONCTIONNELID ON TEM_ETAT_MASTERS (
                                    ""TRU_RESPONSABLE_FONCTIONNELID""
                                );

                                CREATE INDEX IX_TEM_ETAT_MASTERS_TRU_RESPONSABLE_TECHNIQUEID ON TEM_ETAT_MASTERS (
                                    ""TRU_RESPONSABLE_TECHNIQUEID""
                                );

                                CREATE UNIQUE INDEX UQ_TEM_ETAT_MASTER ON TEM_ETAT_MASTERS (
                                    ""TEM_NOM_ETAT_MASTER"",
                                    ""TP_PERIMETREID""
                                );

                                CREATE TABLE sqlitestudio_temp_table27 AS SELECT *
                                                                            FROM TUTE_USER_TEAMS;

                                DROP TABLE TUTE_USER_TEAMS;

                                CREATE TABLE TUTE_USER_TEAMS (
                                    TUTE_USER_TEAMID INTEGER NOT NULL
                                                             CONSTRAINT PK_TUTE_USER_TEAMS PRIMARY KEY AUTOINCREMENT,
                                    TRU_USERID       TEXT    NOT NULL,
                                    TTE_TEAMID       INTEGER NOT NULL,
                                    TUTE_COMMENTAIRE TEXT,
                                    TUTE_DATE_MAJ    TEXT    NOT NULL,
                                    TRST_STATUTID    TEXT    NOT NULL,
                                    CONSTRAINT [FK_TUTE_USER_TEAMSS$TRST_STATUTS] FOREIGN KEY (
                                        TRST_STATUTID
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID),
                                    CONSTRAINT [FK_TUTE_USER_TEAMSS$TTE_TEAM] FOREIGN KEY (
                                        TTE_TEAMID
                                    )
                                    REFERENCES TTE_TEAMS (TTE_TEAMID) 
                                );

                                INSERT INTO TUTE_USER_TEAMS (
                                                                TUTE_USER_TEAMID,
                                                                TRU_USERID,
                                                                TTE_TEAMID,
                                                                TUTE_COMMENTAIRE,
                                                                TUTE_DATE_MAJ,
                                                                TRST_STATUTID
                                                            )
                                                            SELECT TUTE_USER_TEAMID,
                                                                   TRU_USERID,
                                                                   TTE_TEAMID,
                                                                   TUTE_COMMENTAIRE,
                                                                   TUTE_DATE_MAJ,
                                                                   TRST_STATUTID
                                                              FROM sqlitestudio_temp_table27;

                                DROP TABLE sqlitestudio_temp_table27;

                                CREATE INDEX IX_TUTE_USER_TEAMS_TRST_STATUTID ON TUTE_USER_TEAMS (
                                    ""TRST_STATUTID""
                                );

                                CREATE INDEX IX_TUTE_USER_TEAMS_TTE_TEAMID ON TUTE_USER_TEAMS (
                                    ""TTE_TEAMID""
                                );

                                CREATE UNIQUE INDEX UQ_TUTE_USER_TEAMS ON TUTE_USER_TEAMS (
                                    ""TRU_USERID"",
                                    ""TTE_TEAMID""
                                );

                                CREATE TABLE sqlitestudio_temp_table28 AS SELECT *
                                                                            FROM TTAU_AUTHENTIFICATIONS;

                                DROP TABLE TTAU_AUTHENTIFICATIONS;

                                CREATE TABLE TTAU_AUTHENTIFICATIONS (
                                    TTAU_AUTHENTIFICATIONID INTEGER NOT NULL
                                                                    CONSTRAINT PK_TTAU_AUTHENTIFICATIONS PRIMARY KEY AUTOINCREMENT,
                                    TRU_USERID              TEXT,
                                    TTAU_AUTH_CODE          TEXT,
                                    TTAU_AUTH_CODE_EXP      TEXT,
                                    TTAU_REFRESH_TOKEN      TEXT,
                                    TTAU_REFRESH_TOKEN_EXP  TEXT
                                );

                                INSERT INTO TTAU_AUTHENTIFICATIONS (
                                                                       TTAU_AUTHENTIFICATIONID,
                                                                       TRU_USERID,
                                                                       TTAU_AUTH_CODE,
                                                                       TTAU_AUTH_CODE_EXP,
                                                                       TTAU_REFRESH_TOKEN,
                                                                       TTAU_REFRESH_TOKEN_EXP
                                                                   )
                                                                   SELECT TTAU_AUTHENTIFICATIONID,
                                                                          TRU_USERID,
                                                                          TTAU_AUTH_CODE,
                                                                          TTAU_AUTH_CODE_EXP,
                                                                          TTAU_REFRESH_TOKEN,
                                                                          TTAU_REFRESH_TOKEN_EXP
                                                                     FROM sqlitestudio_temp_table28;

                                DROP TABLE sqlitestudio_temp_table28;

                                CREATE INDEX IX_TTAU_AUTHENTIFICATIONS_TRU_USERID ON TTAU_AUTHENTIFICATIONS (
                                    ""TRU_USERID""
                                );

                                DROP TABLE sqlitestudio_temp_table;

                                CREATE UNIQUE INDEX IX_TRU_FULLNAME_TRU_USERS ON TRU_USERS (
                                    ""TRU_NAME"",
                                    ""TRU_FIRST_NAME""
                                );

                                CREATE UNIQUE INDEX IX_TRU_LOGIN_TRU_USERS ON TRU_USERS (
                                    ""TRU_LOGIN""
                                );

                                CREATE INDEX IX_TRU_USERS_TRLG_LNGID ON TRU_USERS (
                                    ""TRLG_LNGID""
                                );

                                CREATE INDEX IX_TRU_USERS_TRTZ_TZID ON TRU_USERS (
                                    ""TRTZ_TZID""
                                );

                                CREATE TABLE sqlitestudio_temp_table29 AS SELECT *
                                                                            FROM TR_MEL_EMail_Templates;

                                DROP TABLE TR_MEL_EMail_Templates;

                                CREATE TABLE TR_MEL_EMail_Templates (
                                    mel_id                      INTEGER  NOT NULL
                                                                         CONSTRAINT PK_TR_MEL_EMail_Templates PRIMARY KEY AUTOINCREMENT,
                                    lng_code                    TEXT,
                                    mel_additional_code         TEXT,
                                    mel_code                    TEXT     NOT NULL,
                                    mel_comments                TEXT,
                                    mel_created_by              TEXT,
                                    mel_creation_date           DATETIME NOT NULL,
                                    mel_description             TEXT,
                                    mel_email_body              TEXT     NOT NULL,
                                    mel_email_footer            TEXT     NOT NULL,
                                    mel_email_importance        TEXT,
                                    mel_email_recipients        TEXT,
                                    mel_email_recipients_in_bcc TEXT,
                                    mel_email_recipients_in_cc  TEXT,
                                    mel_email_subject           TEXT     NOT NULL,
                                    mel_update_by               TEXT,
                                    mel_update_date             DATETIME,
                                    sta_code                    TEXT,
                                    CONSTRAINT FK_TR_MEL_EMail_Templates_TRLG_LNGS_lng_code FOREIGN KEY (
                                        lng_code
                                    )
                                    REFERENCES TR_LNG_Languages (lng_code),
                                    CONSTRAINT FK_TR_MEL_EMail_Templates_TRST_STATUTS_sta_code FOREIGN KEY (
                                        sta_code
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) 
                                );

                                INSERT INTO TR_MEL_EMail_Templates (
                                                                       mel_id,
                                                                       lng_code,
                                                                       mel_additional_code,
                                                                       mel_code,
                                                                       mel_comments,
                                                                       mel_created_by,
                                                                       mel_creation_date,
                                                                       mel_description,
                                                                       mel_email_body,
                                                                       mel_email_footer,
                                                                       mel_email_importance,
                                                                       mel_email_recipients,
                                                                       mel_email_recipients_in_bcc,
                                                                       mel_email_recipients_in_cc,
                                                                       mel_email_subject,
                                                                       mel_update_by,
                                                                       mel_update_date,
                                                                       sta_code
                                                                   )
                                                                   SELECT mel_id,
                                                                          lng_code,
                                                                          mel_additional_code,
                                                                          mel_code,
                                                                          mel_comments,
                                                                          mel_created_by,
                                                                          mel_creation_date,
                                                                          mel_description,
                                                                          mel_email_body,
                                                                          mel_email_footer,
                                                                          mel_email_importance,
                                                                          mel_email_recipients,
                                                                          mel_email_recipients_in_bcc,
                                                                          mel_email_recipients_in_cc,
                                                                          mel_email_subject,
                                                                          mel_update_by,
                                                                          mel_update_date,
                                                                          sta_code
                                                                     FROM sqlitestudio_temp_table29;

                                DROP TABLE sqlitestudio_temp_table29;

                                CREATE INDEX [IX_TMT_CODE$TR_MEL_EMail_Templates] ON TR_MEL_EMail_Templates (
                                    ""mel_code""
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_lng_code ON TR_MEL_EMail_Templates (
                                    ""lng_code""
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_sta_code ON TR_MEL_EMail_Templates (
                                    ""sta_code""
                                );

                                DROP TABLE TRLG_LNGS;

                                PRAGMA foreign_keys = 1;

                                /* End */

                                /* Migrate from TRLG_LNGS to TR_LNG_Languages */

                                PRAGMA foreign_keys = 0;

                                CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                                                          FROM TR_MEL_EMail_Templates;

                                DROP TABLE TR_MEL_EMail_Templates;

                                CREATE TABLE TR_MEL_EMail_Templates (
                                    mel_id                      TEXT     NOT NULL
                                                                         CONSTRAINT PK_TR_MEL_EMail_Templates PRIMARY KEY
                                                                         DEFAULT ( (upper(hex(randomblob(4) ) ) || '-' || upper(hex(randomblob(2) ) ) || '-4' || substr(upper(hex(randomblob(2) ) ), 2) || '-' || substr('89ab', abs(random() ) % 4 + 1, 1) || substr(upper(hex(randomblob(2) ) ), 2) || '-' || upper(hex(randomblob(6) ) ) ) ),
                                    lng_code                    TEXT,
                                    mel_additional_code         TEXT,
                                    mel_code                    TEXT     NOT NULL,
                                    mel_comments                TEXT,
                                    mel_created_by              TEXT,
                                    mel_creation_date           DATETIME NOT NULL,
                                    mel_description             TEXT,
                                    mel_email_body              TEXT     NOT NULL,
                                    mel_email_footer            TEXT     NOT NULL,
                                    mel_email_importance        TEXT,
                                    mel_email_recipients        TEXT,
                                    mel_email_recipients_in_bcc TEXT,
                                    mel_email_recipients_in_cc  TEXT,
                                    mel_email_subject           TEXT     NOT NULL,
                                    mel_update_by               TEXT,
                                    mel_update_date             DATETIME,
                                    sta_code                    TEXT,
                                    CONSTRAINT FK_TR_MEL_EMail_Templates_TRLG_LNGS_lng_code FOREIGN KEY (
                                        lng_code
                                    )
                                    REFERENCES TR_LNG_Languages (lng_code),
                                    CONSTRAINT FK_TR_MEL_EMail_Templates_TRST_STATUTS_sta_code FOREIGN KEY (
                                        sta_code
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) 
                                );

                                INSERT INTO TR_MEL_EMail_Templates (
                                                                       lng_code,
                                                                       mel_additional_code,
                                                                       mel_code,
                                                                       mel_comments,
                                                                       mel_created_by,
                                                                       mel_creation_date,
                                                                       mel_description,
                                                                       mel_email_body,
                                                                       mel_email_footer,
                                                                       mel_email_importance,
                                                                       mel_email_recipients,
                                                                       mel_email_recipients_in_bcc,
                                                                       mel_email_recipients_in_cc,
                                                                       mel_email_subject,
                                                                       mel_update_by,
                                                                       mel_update_date,
                                                                       sta_code
                                                                   )
                                                                   SELECT lng_code,
                                                                          mel_additional_code,
                                                                          mel_code,
                                                                          mel_comments,
                                                                          mel_created_by,
                                                                          mel_creation_date,
                                                                          mel_description,
                                                                          mel_email_body,
                                                                          mel_email_footer,
                                                                          mel_email_importance,
                                                                          mel_email_recipients,
                                                                          mel_email_recipients_in_bcc,
                                                                          mel_email_recipients_in_cc,
                                                                          mel_email_subject,
                                                                          mel_update_by,
                                                                          mel_update_date,
                                                                          sta_code
                                                                     FROM sqlitestudio_temp_table;

                                DROP TABLE sqlitestudio_temp_table;

                                CREATE INDEX [IX_TMT_CODE$TR_MEL_EMail_Templates] ON TR_MEL_EMail_Templates (
                                    ""mel_code""
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_lng_code ON TR_MEL_EMail_Templates (
                                    ""lng_code""
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_sta_code ON TR_MEL_EMail_Templates (
                                    ""sta_code""
                                );

                                PRAGMA foreign_keys = 1;


                                /* End */

                                /* Organize TR_MEL_EMail_Templates */

                                PRAGMA foreign_keys = 0;

                                DELETE FROM TR_MEL_EMail_Templates WHERE mel_code = 'ANNUL_COM';

                                CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                                                          FROM TR_MEL_EMail_Templates;
                                
                                DROP TABLE TR_MEL_EMail_Templates;

                                CREATE TABLE TR_MEL_EMail_Templates (
                                    mel_id                      TEXT     NOT NULL
                                                                         CONSTRAINT PK_TR_MEL_EMail_Templates PRIMARY KEY
                                                                         DEFAULT ( (upper(hex(randomblob(4) ) ) || '-' || upper(hex(randomblob(2) ) ) || '-4' || substr(upper(hex(randomblob(2) ) ), 2) || '-' || substr('89ab', abs(random() ) % 4 + 1, 1) || substr(upper(hex(randomblob(2) ) ), 2) || '-' || upper(hex(randomblob(6) ) ) ) ),
                                    mel_code                    TEXT     NOT NULL,
                                    mel_additional_code         TEXT,
                                    mel_comments                TEXT,
                                    mel_description             TEXT,
                                    lng_code                    TEXT,
                                    sta_code                    TEXT,
                                    mel_email_recipients        TEXT,
                                    mel_email_recipients_in_bcc TEXT,
                                    mel_email_recipients_in_cc  TEXT,
                                    mel_email_importance        TEXT,
                                    mel_email_subject           TEXT     NOT NULL,
                                    mel_email_body              TEXT     NOT NULL,
                                    mel_email_footer            TEXT     NOT NULL,
                                    mel_creation_date           DATETIME NOT NULL,
                                    mel_created_by              TEXT,
                                    mel_update_date             DATETIME,
                                    mel_update_by               TEXT,
                                    CONSTRAINT FK_TR_MEL_EMail_Templates_TRLG_LNGS_lng_code FOREIGN KEY (
                                        lng_code
                                    )
                                    REFERENCES TR_LNG_Languages (lng_code),
                                    CONSTRAINT FK_TR_MEL_EMail_Templates_TRST_STATUTS_sta_code FOREIGN KEY (
                                        sta_code
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) 
                                );

                                INSERT INTO TR_MEL_EMail_Templates (
                                                                       mel_id,
                                                                       mel_code,
                                                                       mel_additional_code,
                                                                       mel_comments,
                                                                       mel_description,
                                                                       lng_code,
                                                                       sta_code,
                                                                       mel_email_recipients,
                                                                       mel_email_recipients_in_bcc,
                                                                       mel_email_recipients_in_cc,
                                                                       mel_email_importance,
                                                                       mel_email_subject,
                                                                       mel_email_body,
                                                                       mel_creation_date,
                                                                       mel_email_footer,
                                                                       mel_created_by,
                                                                       mel_update_date,
                                                                       mel_update_by
                                                                   )
                                                                   SELECT mel_id,
                                                                          mel_code,
                                                                          mel_additional_code,
                                                                          mel_comments,
                                                                          mel_description,
                                                                          lng_code,
                                                                          sta_code,
                                                                          mel_email_recipients,
                                                                          mel_email_recipients_in_bcc,
                                                                          mel_email_recipients_in_cc,
                                                                          mel_email_importance,
                                                                          mel_email_subject,
                                                                          mel_email_body,
                                                                          mel_creation_date,
                                                                          mel_email_footer,
                                                                          mel_created_by,
                                                                          mel_update_date,
                                                                          mel_update_by
                                                                     FROM sqlitestudio_temp_table;

                                DROP TABLE sqlitestudio_temp_table;

                                CREATE INDEX [IX_TMT_CODE$TR_MEL_EMail_Templates] ON TR_MEL_EMail_Templates (
                                    ""mel_code""
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_lng_code ON TR_MEL_EMail_Templates (
                                    ""lng_code""
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_sta_code ON TR_MEL_EMail_Templates (
                                    ""sta_code""
                                );

                                PRAGMA foreign_keys = 1;

                                /* End */

                                INSERT INTO TR_MEL_EMail_Templates (
                                                                       mel_update_by,
                                                                       mel_update_date,
                                                                       mel_created_by,
                                                                       mel_creation_date,
                                                                       mel_email_footer,
                                                                       mel_email_body,
                                                                       mel_email_subject,
                                                                       mel_email_importance,
                                                                       mel_email_recipients_in_cc,
                                                                       mel_email_recipients_in_bcc,
                                                                       mel_email_recipients,
                                                                       sta_code,
                                                                       lng_code,
                                                                       mel_description,
                                                                       mel_comments,
                                                                       mel_additional_code,
                                                                       mel_code,
                                                                       mel_id
                                                                   )
                                                                   VALUES (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>Ce message est envoyé par un traitement automatique, merci de ne pas répondre à ce mail.</i></p><br />[LOGO_ORKESTRA]',
                                                                       '<p>Bonjour,<br /><br />La commande n°[TCMD_COMMANDEID] a été prise en charge par [TMCD_EXPLOITANTID] le [TMCD_SP_DATE_MODIF].<br />Vous serez notifié par email de la livraison de la commande.</p>',
                                                                       'Prise en charge de la commande n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'fr-FR',
                                                                       'Prise en charge d’une commande',
                                                                       'Mail pour notifier le commanditaire que sa commande est en cours',
                                                                       NULL,
                                                                       'PROGRES_COM',
                                                                       '13BC37FB-1AAA-4543-8D0B-F4E3BC084192'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>This is an automated message please do not reply.</i></p>',
                                                                       '<p>Hello,<br /><br />The order n°[TCMD_COMMANDEID] has been has been taken care of by [TMCD_EXPLOITANTID] on [TMCD_SP_DATE_MODIF].<br />You will be notified by email when the order has been delivered.</p>',
                                                                       'Handling of the order n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'en-US',
                                                                       'Handling of an order',
                                                                       'Email to notify  the applicant that the order has been taken care of',
                                                                       NULL,
                                                                       'PROGRES_COM',
                                                                       '4F6FAEB7-333E-4FE5-A6B6-C8069ADF3543'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>Ce message est envoyé par un traitement automatique, merci de ne pas répondre à ce mail.</i></p><br />[LOGO_ORKESTRA]',
                                                                       '<p>Bonjour,<br /><br />La commande n°[TCMD_COMMANDEID] a été rejetée par [TRU_AUTEUR_MODIFID] le [TMCD_SP_DATE_MODIF].<br /><br /><b>Raison</b> : [TMCD_RP_LIB_FR].<br /><b>Commentaire</b> :  [TMCD_SP_COMMENTAIRE].</p>',
                                                                       'Rejet de la commande n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'fr-FR',
                                                                       'Rejet d’une commande',
                                                                       'Mail pour notifier le commanditaire que sa commande a été rejetée',
                                                                       NULL,
                                                                       'REJET_COM',
                                                                       '033F9450-042C-421D-A490-C8B685CAD129'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>This is an automated message please do not reply.</i></p>',
                                                                       '<p>Hello,<br /><br />The order n°[TCMD_COMMANDEID] has been rejected by [TRU_AUTEUR_MODIFID] on [TMCD_SP_DATE_MODIF].<br /><br /><b>Reason</b>: [TMCD_RP_LIB_EN].<br /><b>Comment</b>: [TMCD_SP_COMMENTAIRE].</p>',
                                                                       'Rejection of the order n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'en-US',
                                                                       'Rejection of an order',
                                                                       'Email to notify  the applicant that the order has been rejected',
                                                                       NULL,
                                                                       'REJET_COM',
                                                                       'ADD5E798-2573-4BF2-9FC2-D5862C5C49AD'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>Ce message est envoyé par un traitement automatique, merci de ne pas répondre à ce mail.</i></p><br />[LOGO_ORKESTRA]',
                                                                       '<p>Bonjour,<br /><br />La commande n°[TCMD_COMMANDEID] a été livrée par [TRU_AUTEUR_MODIFID] le [TMCD_SP_DATE_MODIF].</p>',
                                                                       'Livraison de la commande n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'fr-FR',
                                                                       'Livraison d’une commande',
                                                                       'Mail pour notifier le commanditaire que sa commande a été livrée',
                                                                       NULL,
                                                                       'LIVRE_COM',
                                                                       'EFA33BE1-91F3-478B-9CE4-F880BEDD68D5'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>This is an automated message please do not reply.</i></p>',
                                                                       '<p>Hello,<br /><br />The order n°[TCMD_COMMANDEID] has been delivered by [TRU_AUTEUR_MODIFID] on [TMCD_SP_DATE_MODIF].</p>',
                                                                       'Delivery of the order n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'en-US',
                                                                       'Delivery of an order',
                                                                       'Email to notify  the applicant that the order has been has been delivered ',
                                                                       NULL,
                                                                       'LIVRE_COM',
                                                                       '69E4AE89-4F87-4F9E-81DB-43803E80D509'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>Ce message est envoyé par un traitement automatique, merci de ne pas répondre à ce mail.</i></p><br />[LOGO_ORKESTRA]',
                                                                       '<p>Bonjour,<br /><br />La commande n°[TCMD_COMMANDEID] qui vous était assignée, a été clôturée par [TRU_AUTEUR_MODIFID] le [TMCD_SP_DATE_MODIF].</p>',
                                                                       'Clôture de la commande n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'fr-FR',
                                                                       'Clôture d’une commande',
                                                                       'Mail pour notifier l’exploitant que la commande a été clôturée',
                                                                       NULL,
                                                                       'FIN_COM',
                                                                       '20EF6696-F652-4861-BBD7-5A30D75F1794'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>This is an automated message please do not reply.</i></p>',
                                                                       '<p>Hello,<br /><br />The order n°[TCMD_COMMANDEID] which had been assigned to you, has been closed by [TRU_AUTEUR_MODIFID] on [TMCD_SP_DATE_MODIF].</p>',
                                                                       'Close of the order n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'en-US',
                                                                       'Close of the order',
                                                                       'Email to notify the operator that the order has been closed',
                                                                       NULL,
                                                                       'FIN_COM',
                                                                       'A9E4B966-FD90-405A-B4D9-8AF34C04219E'
                                                                   ),

                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>Ce message est envoyé par un traitement automatique, merci de ne pas répondre à ce mail.</i></p><br />[LOGO_ORKESTRA]',
                                                                       '<p>Bonjour,<br /><br />La commande n°[TCMD_COMMANDEID] qui vous est assignée, a été annulée par [TRU_AUTEUR_MODIFID] le [TMCD_SP_DATE_MODIF].<br /><br /><b>Raison</b> : [TMCD_RP_LIB_FR].<br /><b>Commentaire</b> :  [TMCD_SP_COMMENTAIRE].<br /><br />La commande a été passée automatiquement au statut ""Annulée"".</p>',
                                                                       'Annilation de la commande n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'fr-FR',
                                                                       'Annulation d’une commande',
                                                                       'Mail pour notifier l’exploitant que la commande a été annulée',
                                                                       NULL,
                                                                       'ANNUL_COM',
                                                                       '620D1691-6025-4FAB-8EC3-31561BD74FC2'
                                                                   ),
                                                                   (
                                                                       NULL,
                                                                       NULL,
                                                                       'Admin',
                                                                       strftime('%Y-%m-%d %H:%M:%S', datetime('now')),
                                                                       '<p><i>This is an automated message please do not reply.</i></p>',
                                                                       '<p>Hello,<br /><br />The order n°[TCMD_COMMANDEID] which had been assigned to you, has been cancelled by [TRU_AUTEUR_MODIFID] on [TMCD_SP_DATE_MODIF].<br /><br /><b>Reason</b>: [TMCD_RP_LIB_EN].<br /><b>Comment</b>: [TMCD_SP_COMMENTAIRE].<br /><br />The order has been automatically qualified as ""Cancelled"".</p>',
                                                                       'Cancellation of the order n° [TCMD_COMMANDEID]',
                                                                       'H',
                                                                       NULL,
                                                                       NULL,
                                                                       NULL,
                                                                       'A',
                                                                       'en-US',
                                                                       'Cancellation of an order',
                                                                       'Email to notify the operator that the order has been cancelled',
                                                                       NULL,
                                                                       'ANNUL_COM',
                                                                       'CCE9A343-6E5C-4A4B-8597-A6A9775F08D2'
                                                                   );

                            ";

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
