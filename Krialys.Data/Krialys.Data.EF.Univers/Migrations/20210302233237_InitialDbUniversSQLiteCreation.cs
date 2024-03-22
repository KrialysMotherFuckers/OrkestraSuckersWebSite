using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class InitialDbUniversSQLiteCreation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TLE_LOGICIEL_EDITEURS",
            columns: table => new
            {
                TLE_LOGICIEL_EDITEURID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TLE_EDITEUR = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TLE_LOGICIEL_EDITEURS", x => x.TLE_LOGICIEL_EDITEURID);
            });

        migrationBuilder.CreateTable(
            name: "TPM_PARAMS",
            columns: table => new
            {
                TPM_PARAMID = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TPM_VALEUR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TPM_INFO = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPM_PARAMS", x => x.TPM_PARAMID);
            });

        migrationBuilder.CreateTable(
            name: "TQM_QUALIF_MODELES",
            columns: table => new
            {
                TQM_QUALIF_MODELEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TQM_LIB = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                TQM_VALEUR_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TQM_VALEUR_MAX = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QUALIF_MODELES", x => x.TQM_QUALIF_MODELEID);
            });

        migrationBuilder.CreateTable(
            name: "TRCL_CLAIMS",
            columns: table => new
            {
                TRCL_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRCL_CLAIM_NAME = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                TRCL_CLAIM_DESCRIPION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRCL_CLAIMS", x => x.TRCL_CLAIMID);
            });

        migrationBuilder.CreateTable(
            name: "TRCLI_CLIENTAPPLICATIONS",
            columns: table => new
            {
                TRCLI_APPLICATIONID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRCLI_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRCLI_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRCLI_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TRCLI_AUTH_PUBLIC = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TRCLI_AUTH_SECRET = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRCLI_CLIENTAPPLICATIONS", x => x.TRCLI_APPLICATIONID);
            });

        migrationBuilder.CreateTable(
            name: "TRLG_LNGS",
            columns: table => new
            {
                TRLG_LNGID = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                TRLG_PREFERED_LNG = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRLG_LNGS", x => x.TRLG_LNGID);
            });

        migrationBuilder.CreateTable(
            name: "TRST_STATUTS",
            columns: table => new
            {
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TRST_INFO = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_INFO_EN = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_REGLE01 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE02 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE03 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE04 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE05 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE06 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE07 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE08 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE09 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_REGLE10 = table.Column<int>(type: "INTEGER", nullable: true),
                TRST_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRST_STATUTS", x => x.TRST_STATUTID);
            });

        migrationBuilder.CreateTable(
            name: "TRTZ_TZS",
            columns: table => new
            {
                TRTZ_TZID = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                TRTZ_PREFERED_TZ = table.Column<int>(type: "INTEGER", nullable: true),
                TRTZ_INFO_TZ = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRTZ_TZS", x => x.TRTZ_TZID);
            });

        migrationBuilder.CreateTable(
            name: "TRU_UTILISATEURS",
            columns: table => new
            {
                TRU_UTILISATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TRU_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TRU_LOGIN = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_PWD = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_FIRST_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_EMAIL = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRU_UTILISATEURS", x => x.TRU_UTILISATEURID);
            });

        migrationBuilder.CreateTable(
            name: "TL_LOGICIELS",
            columns: table => new
            {
                TL_LOGICIELID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TL_NOM_LOGICIEL = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                TL_LOGICIEL_VERSION = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                TL_DATE_LOGICIEL = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TLE_LOGICIEL_EDITEURID = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TL_LOGICIELS", x => x.TL_LOGICIELID);
                table.ForeignKey(
                    name: "FK_TL_LOGICIELS$TLE_LOGICIEL_EDITEURS",
                    column: x => x.TLE_LOGICIEL_EDITEURID,
                    principalTable: "TLE_LOGICIEL_EDITEURS",
                    principalColumn: "TLE_LOGICIEL_EDITEURID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TLEM_LOGICIEL_EDITEUR_MODELES",
            columns: table => new
            {
                TLEM_LOGICIEL_EDITEUR_MODELEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TLE_LOGICIEL_EDITEURID = table.Column<int>(type: "INTEGER", nullable: false),
                TLEM_ACTION = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TLEM_FILE_TYPE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TLEM_PATH_NAME = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TLEM_FILENAME_PATTERN = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                TLEM_INFO = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TLEM_LOGICIEL_EDITEUR_MODELES", x => x.TLEM_LOGICIEL_EDITEUR_MODELEID);
                table.ForeignKey(
                    name: "FK_TLEM_LOGICIEL_EDITEUR_MODELES$TLE_LOGICIEL_EDITEURS",
                    column: x => x.TLE_LOGICIEL_EDITEURID,
                    principalTable: "TLE_LOGICIEL_EDITEURS",
                    principalColumn: "TLE_LOGICIEL_EDITEURID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            columns: table => new
            {
                TRCLICL_CLIENTAPPLICATION_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRCL_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false),
                TRCLI_CLIENTAPPLICATIONID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRCLICL_CLIENTAPPLICATIONS_CLAIMS", x => x.TRCLICL_CLIENTAPPLICATION_CLAIMID);
                table.ForeignKey(
                    name: "FK_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCL_CLAIMS",
                    column: x => x.TRCL_CLAIMID,
                    principalTable: "TRCL_CLAIMS",
                    principalColumn: "TRCL_CLAIMID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCLI_CLIENTAPPLICATIONS",
                    column: x => x.TRCLI_CLIENTAPPLICATIONID,
                    principalTable: "TRCLI_CLIENTAPPLICATIONS",
                    principalColumn: "TRCLI_APPLICATIONID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TF_FERMES",
            columns: table => new
            {
                TF_FERMEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TF_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValueSql: "('A')"),
                TF_EN_MAINTENANCE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('N')")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FERMES", x => x.TF_FERMEID);
                table.ForeignKey(
                    name: "FK_TF_FERMES$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TI_INFOS",
            columns: table => new
            {
                TI_INFOID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TI_DATE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TI_MSG = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TI_MSG_EN = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TI_IMG = table.Column<byte[]>(type: "BLOB", nullable: true),
                TI_DT_DEBUT_VALIDITE = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TI_DT_FIN_VALIDITE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TI_INFOS", x => x.TI_INFOID);
                table.ForeignKey(
                    name: "FK_TI_INFOS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TP_PERIMETRES",
            columns: table => new
            {
                TP_PERIMETREID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TP_PERIMETRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TP_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TP_LOGO = table.Column<byte[]>(type: "BLOB", nullable: true),
                TQM_QUALIF_MODELEID = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PERIMETRES", x => x.TP_PERIMETREID);
                table.ForeignKey(
                    name: "FK_TP_PERIMETRES$TQM_QUALIF_MODELES",
                    column: x => x.TQM_QUALIF_MODELEID,
                    principalTable: "TQM_QUALIF_MODELES",
                    principalColumn: "TQM_QUALIF_MODELEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TP_PERIMETRES$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPR_PROFILS",
            columns: table => new
            {
                TPR_PROFILID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TPR_LIB = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPR_PROFILS", x => x.TPR_PROFILID);
                table.ForeignKey(
                    name: "FK_TPR_PROFILS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TSRV_SERVEURS",
            columns: table => new
            {
                TSRV_SERVEURID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TSRV_NOM = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TSRV_IP = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                TSRV_DATE_ACTIVATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TSRV_DATE_DERNIERE_ACTIVITE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TSRV_DUREE_EXPLOITATION = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "((0))"),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValueSql: "('I')"),
                TSRV_OS = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TSRV_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TSRV_SERVEURS", x => x.TSRV_SERVEURID);
                table.ForeignKey(
                    name: "FK_TSRV_SERVEURS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TRUCL_UTILISATEURS_CLAIMS",
            columns: table => new
            {
                TRUCL_UTILISATEUR_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRU_UTILISATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TRCL_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false),
                TRUCL_CLAIM_VALUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRUCL_UTILISATEURS_CLAIMS", x => x.TRUCL_UTILISATEUR_CLAIMID);
                table.ForeignKey(
                    name: "FK_TRUCL_UTILISATEURS_CLAIMS_TRCL_CLAIMS",
                    column: x => x.TRCL_CLAIMID,
                    principalTable: "TRCL_CLAIMS",
                    principalColumn: "TRCL_CLAIMID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TRUCL_UTILISATEURS_CLAIMS_TRU_UTILISATEURS",
                    column: x => x.TRU_UTILISATEURID,
                    principalTable: "TRU_UTILISATEURS",
                    principalColumn: "TRU_UTILISATEURID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TTAU_AUTHENTIFICATIONS",
            columns: table => new
            {
                TTAU_AUTHENTIFICATIONID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRU_UTILISATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TTAU_AUTH_CODE = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                TTAU_AUTH_CODE_EXP = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TTAU_REFRESH_TOKEN = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                TTAU_REFRESH_TOKEN_EXP = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TTAU_AUTHENTIFICATIONS", x => x.TTAU_AUTHENTIFICATIONID);
                table.ForeignKey(
                    name: "FK_TTAU_AUTHENTIFICATIONS_TRU_UTILISATEURS",
                    column: x => x.TRU_UTILISATEURID,
                    principalTable: "TRU_UTILISATEURS",
                    principalColumn: "TRU_UTILISATEURID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TC_CATEGORIES",
            columns: table => new
            {
                TP_PERIMETREID = table.Column<int>(type: "INTEGER", nullable: false),
                TC_CATEGORIEID = table.Column<int>(type: "INTEGER", nullable: false),
                TC_NOM = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                TC_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TC_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TC_CATEGORIES", x => new { x.TC_CATEGORIEID, x.TP_PERIMETREID });
                table.ForeignKey(
                    name: "FK_TC_CATEGORIES$TP_PERIMETRES",
                    column: x => x.TP_PERIMETREID,
                    principalTable: "TP_PERIMETRES",
                    principalColumn: "TP_PERIMETREID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPU_PARALLELEUS",
            columns: table => new
            {
                TPU_PARALLELEUID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TPU_INSTANCE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TPU_DATE_PREMIERE_ACTIVITE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TPU_DATE_DERNIERE_ACTIVITE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TPU_DUREE_EXPLOITATION = table.Column<int>(type: "INTEGER", nullable: true),
                TPU_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TSRV_SERVEURID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPU_PARALLELEUS", x => x.TPU_PARALLELEUID);
                table.ForeignKey(
                    name: "FK_TPU_PARALLELEUS$SERVEUR",
                    column: x => x.TSRV_SERVEURID,
                    principalTable: "TSRV_SERVEURS",
                    principalColumn: "TSRV_SERVEURID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPU_PARALLELEUS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TSL_SERVEUR_LOGICIELS",
            columns: table => new
            {
                TSL_SERVEUR_LOGICIELID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TSRV_SERVEURID = table.Column<int>(type: "INTEGER", nullable: false),
                TL_LOGICIELID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TSL_SERVEUR_LOGICIELS", x => x.TSL_SERVEUR_LOGICIELID);
                table.ForeignKey(
                    name: "FK_TSL_SERVEUR_LOGICIELS$SERVEUR",
                    column: x => x.TSRV_SERVEURID,
                    principalTable: "TSRV_SERVEURS",
                    principalColumn: "TSRV_SERVEURID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TSL_SERVEUR_LOGICIELS$TL_LOGICIELS",
                    column: x => x.TL_LOGICIELID,
                    principalTable: "TL_LOGICIELS",
                    principalColumn: "TL_LOGICIELID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TSP_SERVEUR_PARAMS",
            columns: table => new
            {
                TSP_SERVEUR_PARAMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TSRV_SERVEURID = table.Column<int>(type: "INTEGER", nullable: false),
                TSP_KEY = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TSP_VALUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TSP_TYPE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TSP_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TSP_SERVEUR_PARAMS", x => x.TSP_SERVEUR_PARAMID);
                table.ForeignKey(
                    name: "FK_TSP_SERVEUR_PARAMS$SERVEUR",
                    column: x => x.TSRV_SERVEURID,
                    principalTable: "TSRV_SERVEURS",
                    principalColumn: "TSRV_SERVEURID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TSP_SERVEUR_PARAMS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TEM_ETAT_MASTERS",
            columns: table => new
            {
                TEM_ETAT_MASTERID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TEM_NOM_ETAT_MASTER = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TEM_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValueSql: "('A')"),
                TEM_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TEM_ETAT_MASTER_PARENTID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TRU_RESPONSABLE_FONCTIONNELID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TP_PERIMETREID = table.Column<int>(type: "INTEGER", nullable: false),
                TC_CATEGORIEID = table.Column<int>(type: "INTEGER", nullable: true),
                TEM_GUID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TEM_ETAT_MASTERS", x => x.TEM_ETAT_MASTERID);
                table.ForeignKey(
                    name: "FK_TEM_ETAT_MASTERS$TC_CATEGORIES",
                    columns: x => new { x.TC_CATEGORIEID, x.TP_PERIMETREID },
                    principalTable: "TC_CATEGORIES",
                    principalColumns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TEM_ETAT_MASTERS$TP_PERIMETRES",
                    column: x => x.TP_PERIMETREID,
                    principalTable: "TP_PERIMETRES",
                    principalColumn: "TP_PERIMETREID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPUF_PARALLELEU_FERMES",
            columns: table => new
            {
                TPUF_PARALLELEU_FERMEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TF_FERMEID = table.Column<int>(type: "INTEGER", nullable: false),
                TPU_PARALLELEUID = table.Column<int>(type: "INTEGER", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TPUF_DATE_MODIFICATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TPUF_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPUF_PARALLELEU_FERMES", x => x.TPUF_PARALLELEU_FERMEID);
                table.ForeignKey(
                    name: "FK_TPUF_PARALLELEU_FERMES$TF_FERMES",
                    column: x => x.TF_FERMEID,
                    principalTable: "TF_FERMES",
                    principalColumn: "TF_FERMEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPUF_PARALLELEU_FERMES$TPU_PARALLELEUS",
                    column: x => x.TPU_PARALLELEUID,
                    principalTable: "TPU_PARALLELEUS",
                    principalColumn: "TPU_PARALLELEUID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPUF_PARALLELEU_FERMES$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPUP_PARALLELEU_PARAMS",
            columns: table => new
            {
                TPUP_PARALLELEU_PARAMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TPU_PARALLELEUID = table.Column<int>(type: "INTEGER", nullable: false),
                TPUP_KEY = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TPUP_VALUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TPUP_TYPE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TPUP_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPUP_PARALLELEU_PARAMS", x => x.TPUP_PARALLELEU_PARAMID);
                table.ForeignKey(
                    name: "FK_TPUF_PARALLELEU_PARAMS$TPU_PARALLELEUS",
                    column: x => x.TPU_PARALLELEUID,
                    principalTable: "TPU_PARALLELEUS",
                    principalColumn: "TPU_PARALLELEUID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPUP_PARALLELEU_PARAMS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TE_ETATS",
            columns: table => new
            {
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TEM_ETAT_MASTERID = table.Column<int>(type: "INTEGER", nullable: false),
                TE_NOM_ETAT = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TE_NOM_DATABASE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TE_NOM_TECHNIQUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TE_INDICE_REVISION_L1 = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "((1))"),
                TE_INDICE_REVISION_L2 = table.Column<int>(type: "INTEGER", nullable: false),
                TE_INDICE_REVISION_L3 = table.Column<int>(type: "INTEGER", nullable: false),
                TE_DATE_REVISION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValueSql: "('B')"),
                TE_TYPE_SORTIE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false, defaultValueSql: "('M')"),
                TE_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TE_INFO_REVISION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TE_DUREE_PRODUCTION_ESTIMEE = table.Column<int>(type: "INTEGER", nullable: true),
                TE_DUREE_DERNIERE_PRODUCTION = table.Column<int>(type: "INTEGER", nullable: true),
                TE_DATE_DERNIERE_PRODUCTION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TE_GUID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                PARENT_ETATID = table.Column<int>(type: "INTEGER", nullable: true),
                TE_VALIDATION_IMPLICITE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('N')"),
                TE_SEND_MAIL_GESTIONNAIRE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('O')"),
                TE_SEND_MAIL_CLIENT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('O')"),
                TE_LEVEL_IMPORTANCE = table.Column<int>(type: "INTEGER", nullable: true),
                TE_ENV_VIERGE_TAILLE = table.Column<int>(type: "INTEGER", nullable: false),
                TE_ENV_VIERGE_UPLOADED = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false, defaultValueSql: "('N')"),
                TRU_ENV_VIERGE_AUTEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TRU_DECLARANTID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TE_ETATS", x => x.TE_ETATID);
                table.ForeignKey(
                    name: "FK_TE_ETATS$TEM_ETAT_MASTERS",
                    column: x => x.TEM_ETAT_MASTERID,
                    principalTable: "TEM_ETAT_MASTERS",
                    principalColumn: "TEM_ETAT_MASTERID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TEMF_ETAT_MASTER_FERMES",
            columns: table => new
            {
                TEMF_ETAT_MASTER_FERMEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TEM_ETAT_MASTERID = table.Column<int>(type: "INTEGER", nullable: false),
                TF_FERMEID = table.Column<int>(type: "INTEGER", nullable: false),
                TEMF_DATE_AJOUT = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TEMF_DATE_SUPPRESSION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TEMF_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TEMF_ORDRE_PRIORITE = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "((0))")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TEMF_ETAT_MASTER_FERMES", x => x.TEMF_ETAT_MASTER_FERMEID);
                table.ForeignKey(
                    name: "FK_TEMF_ETAT_MASTER_FERMES$TEM_ETAT_MASTERS",
                    column: x => x.TEM_ETAT_MASTERID,
                    principalTable: "TEM_ETAT_MASTERS",
                    principalColumn: "TEM_ETAT_MASTERID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TEMF_ETAT_MASTER_FERMES$TF_FERMES",
                    column: x => x.TF_FERMEID,
                    principalTable: "TF_FERMES",
                    principalColumn: "TF_FERMEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TD_DEMANDES",
            columns: table => new
            {
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TD_DATE_DEMANDE = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TD_DATE_EXECUTION_SOUHAITEE = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TD_DATE_PRISE_EN_CHARGE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TD_DATE_LIVRAISON = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValueSql: "('DB')"),
                TD_COMMENTAIRE_UTILISATEUR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TD_INFO_RETOUR_TRAITEMENT = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRU_DEMANDEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TSRV_SERVEURID = table.Column<int>(type: "INTEGER", nullable: true),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false),
                TRU_GESTIONNAIRE_VALIDEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TD_DEMANDE_ORIGINEID = table.Column<int>(type: "INTEGER", nullable: true),
                TPF_PLANIF_ORIGINEID = table.Column<int>(type: "INTEGER", nullable: true),
                TD_DUREE_PRODUCTION_REEL = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "((0))"),
                TD_FULL_RESULT_FILE_PATH = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TD_COMMENTAIRE_GESTIONNAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TD_DATE_AVIS_GESTIONNAIRE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TD_SEND_MAIL_GESTIONNAIRE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TD_SEND_MAIL_CLIENT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('O')"),
                TD_RESULT_EXIST_FILE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TD_RESULT_FILE_SIZE = table.Column<int>(type: "INTEGER", nullable: true),
                TD_RESULT_NB_DOWNLOAD = table.Column<int>(type: "INTEGER", nullable: true, defaultValueSql: "((0))"),
                TD_DATE_DERNIER_DOWNLOAD = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TD_PREREQUIS_DELAI_CHK = table.Column<int>(type: "INTEGER", nullable: true),
                TD_QUALIF_EXIST_FILE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TD_QUALIF_BILAN = table.Column<int>(type: "INTEGER", nullable: true),
                TD_QUALIF_FILE_SIZE = table.Column<int>(type: "INTEGER", nullable: true),
                TD_GUID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TD_DEMANDES", x => x.TD_DEMANDEID);
                table.UniqueConstraint("AK_TD_DEMANDES_TD_DEMANDEID_TE_ETATID", x => new { x.TD_DEMANDEID, x.TE_ETATID });
                table.ForeignKey(
                    name: "FK_TD_DEMANDES$SERVEUR",
                    column: x => x.TSRV_SERVEURID,
                    principalTable: "TSRV_SERVEURS",
                    principalColumn: "TSRV_SERVEURID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TD_DEMANDES$TE_ETATS",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TEB_ETAT_BATCHS",
            columns: table => new
            {
                TEB_ETAT_BATCHID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TEB_NOM_AFFICHAGE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TEB_NOM_TECHNIQUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TEB_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TEB_CMD = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TEB_DATE_CREATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TEB_ETAT_BATCHS", x => x.TEB_ETAT_BATCHID);
                table.ForeignKey(
                    name: "FK_TEB_ETAT_BATCHS$TE_ETATS",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TEL_ETAT_LOGICIELS",
            columns: table => new
            {
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TL_LOGICIELID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TEL_ETAT_LOGICIELS", x => new { x.TE_ETATID, x.TL_LOGICIELID });
                table.ForeignKey(
                    name: "FK_TEL_ETAT_LOGICIELS$TE_ETATS",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TEL_ETAT_LOGICIELS$TL_LOGICIELS",
                    column: x => x.TL_LOGICIELID,
                    principalTable: "TL_LOGICIELS",
                    principalColumn: "TL_LOGICIELID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TEP_ETAT_PREREQUISS",
            columns: table => new
            {
                TEP_ETAT_PREREQUISID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TEP_DATE_MAJ = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TEP_NATURE = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                TEP_PATH = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TEP_FILEPATTERN = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TEP_IS_PATTERN = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TEP_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TEP_ETAT_PREREQUISS", x => x.TEP_ETAT_PREREQUISID);
                table.ForeignKey(
                    name: "FK_TEP_ETAT_PREREQUISS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TEP_PREREQUISS$TE_ETATS",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TER_ETAT_RESSOURCES",
            columns: table => new
            {
                TER_ETAT_RESSOURCEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TER_NOM_FICHIER = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TER_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TER_PATH_RELATIF = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TER_MODELE_DOC = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TER_MODELE_DATE = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TER_MODELE_TAILLE = table.Column<int>(type: "INTEGER", nullable: true),
                TER_IS_PATTERN = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false, defaultValueSql: "('N')"),
                TER_NOM_MODELE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TER_ETAT_RESSOURCES", x => x.TER_ETAT_RESSOURCEID);
                table.ForeignKey(
                    name: "FK_TER_ETAT_RESSOURCES$TE_ETATS",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TS_SCENARIOS",
            columns: table => new
            {
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TS_NOM_SCENARIO = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TS_DESCR = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TS_IS_DEFAULT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TS_GUID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TS_SCENARIOS", x => x.TS_SCENARIOID);
                table.ForeignKey(
                    name: "FK_TS_SCENARIOS$TE_ETATS",
                    column: x => x.TE_ETATID,
                    principalTable: "TE_ETATS",
                    principalColumn: "TE_ETATID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TDQ_DEMANDE_QUALIFS",
            columns: table => new
            {
                TDQ_DEMANDE_QUALIFID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TDQ_NUM_ORDRE = table.Column<int>(type: "INTEGER", nullable: true),
                TDQ_CODE = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                TDQ_NOM = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                TDQ_VALEUR = table.Column<int>(type: "INTEGER", nullable: true),
                TDQ_NATURE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TDQ_DATASET = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TDQ_OBJECTIF = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TDQ_TYPOLOGIE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TDQ_COMMENT = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TDQ_DATE_PROD = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TDQ_DEMANDE_QUALIFS", x => x.TDQ_DEMANDE_QUALIFID);
                table.ForeignKey(
                    name: "FK_TDQ_DEMANDE_QUALIFS$TD_DEMANDES",
                    column: x => x.TD_DEMANDEID,
                    principalTable: "TD_DEMANDES",
                    principalColumn: "TD_DEMANDEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPF_PLANIFS",
            columns: table => new
            {
                TPF_PLANIFID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TPF_DATE_DEBUT = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                TPF_DATE_FIN = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TRST_STATUTID = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                TPF_DEMANDE_ORIGINEID = table.Column<int>(type: "INTEGER", nullable: false),
                TPF_PREREQUIS_DELAI_CHK = table.Column<int>(type: "INTEGER", nullable: true),
                TPF_CRON = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPF_PLANIFS", x => x.TPF_PLANIFID);
                table.ForeignKey(
                    name: "FK_TPF_PLANIFS$TD_DEMANDES",
                    column: x => x.TPF_DEMANDE_ORIGINEID,
                    principalTable: "TD_DEMANDES",
                    principalColumn: "TD_DEMANDEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPF_PLANIFS$TRST_STATUTS",
                    column: x => x.TRST_STATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TBD_BATCH_DEMANDES",
            columns: table => new
            {
                TBD_BATCH_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TBD_ORDRE_EXECUTION = table.Column<int>(type: "INTEGER", nullable: false),
                TBD_EXECUTION = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TBD_CODE_RETOUR = table.Column<int>(type: "INTEGER", nullable: true),
                TEB_ETAT_BATCHID = table.Column<int>(type: "INTEGER", nullable: false),
                TBD_DATE_DEBUT_EXECUTION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TBD_DATE_FIN_EXECUTION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TBD_BATCH_DEMANDES", x => x.TBD_BATCH_DEMANDEID);
                table.ForeignKey(
                    name: "FK_TBD_BATCH_DEMANDES$TD_DEMANDES",
                    columns: x => new { x.TD_DEMANDEID, x.TE_ETATID },
                    principalTable: "TD_DEMANDES",
                    principalColumns: new[] { "TD_DEMANDEID", "TE_ETATID" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TBD_BATCH_DEMANDES$TEB_ETAT_BATCHS",
                    column: x => x.TEB_ETAT_BATCHID,
                    principalTable: "TEB_ETAT_BATCHS",
                    principalColumn: "TEB_ETAT_BATCHID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPD_PREREQUIS_DEMANDES",
            columns: table => new
            {
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TEP_ETAT_PREREQUISID = table.Column<int>(type: "INTEGER", nullable: false),
                TPD_VALIDE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TPD_DATE_VALIDATION = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TPD_DATE_LAST_CHECK = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                TPD_NB_FILE_TROUVE = table.Column<int>(type: "INTEGER", nullable: true),
                TPD_MSG_LAST_CHECK = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPD_PREREQUIS_DEMANDES", x => new { x.TD_DEMANDEID, x.TEP_ETAT_PREREQUISID });
                table.ForeignKey(
                    name: "FK_TPD_PREREQUIS_DEMANDES$TD_DEMANDES",
                    column: x => x.TD_DEMANDEID,
                    principalTable: "TD_DEMANDES",
                    principalColumn: "TD_DEMANDEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPD_PREREQUIS_DEMANDES$TEP_ETAT_PREREQUIS",
                    column: x => x.TEP_ETAT_PREREQUISID,
                    principalTable: "TEP_ETAT_PREREQUISS",
                    principalColumn: "TEP_ETAT_PREREQUISID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TRD_RESSOURCE_DEMANDES",
            columns: table => new
            {
                TRD_RESSOURCE_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TE_ETATID = table.Column<int>(type: "INTEGER", nullable: false),
                TD_DEMANDEID = table.Column<int>(type: "INTEGER", nullable: false),
                TER_ETAT_RESSOURCEID = table.Column<int>(type: "INTEGER", nullable: false),
                TRD_NOM_FICHIER = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                TRD_NOM_FICHIER_ORIGINAL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRD_FICHIER_PRESENT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('N')"),
                TRD_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRD_TAILLE_FICHIER = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRD_RESSOURCE_DEMANDES", x => x.TRD_RESSOURCE_DEMANDEID);
                table.ForeignKey(
                    name: "FK_TRD_RESSOURCE_DEMANDES$TD_DEMANDES",
                    columns: x => new { x.TD_DEMANDEID, x.TE_ETATID },
                    principalTable: "TD_DEMANDES",
                    principalColumns: new[] { "TD_DEMANDEID", "TE_ETATID" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRD_RESSOURCE_DEMANDES$TER_ETAT_RESSOURCES",
                    column: x => x.TER_ETAT_RESSOURCEID,
                    principalTable: "TER_ETAT_RESSOURCES",
                    principalColumn: "TER_ETAT_RESSOURCEID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TBS_BATCH_SCENARIOS",
            columns: table => new
            {
                TEB_ETAT_BATCHID = table.Column<int>(type: "INTEGER", nullable: false),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false),
                TBS_ORDRE_EXECUTION = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TBS_BATCH_SCENARIOS", x => new { x.TEB_ETAT_BATCHID, x.TS_SCENARIOID });
                table.ForeignKey(
                    name: "FK_TBS_BATCH_SCENARIOS$TEB_ETAT_BATCHS",
                    column: x => x.TEB_ETAT_BATCHID,
                    principalTable: "TEB_ETAT_BATCHS",
                    principalColumn: "TEB_ETAT_BATCHID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TBS_BATCH_SCENARIOS$TS_SCENARIOS",
                    column: x => x.TS_SCENARIOID,
                    principalTable: "TS_SCENARIOS",
                    principalColumn: "TS_SCENARIOID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TPS_PREREQUIS_SCENARIOS",
            columns: table => new
            {
                TEP_ETAT_PREREQUISID = table.Column<int>(type: "INTEGER", nullable: false),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false),
                TPS_NB_FILE_MIN = table.Column<int>(type: "INTEGER", nullable: true),
                TPS_NB_FILE_MAX = table.Column<int>(type: "INTEGER", nullable: true),
                TPS_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TPS_PREREQUIS_SCENARIOS", x => new { x.TEP_ETAT_PREREQUISID, x.TS_SCENARIOID });
                table.ForeignKey(
                    name: "FK_TPS_PREREQUIS_SCENARIOS$TEP_ETAT_PREREQUIS",
                    column: x => x.TEP_ETAT_PREREQUISID,
                    principalTable: "TEP_ETAT_PREREQUISS",
                    principalColumn: "TEP_ETAT_PREREQUISID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TPS_PREREQUIS_SCENARIOS$TS_SCENARIOS",
                    column: x => x.TS_SCENARIOID,
                    principalTable: "TS_SCENARIOS",
                    principalColumn: "TS_SCENARIOID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TRS_RESSOURCE_SCENARIOS",
            columns: table => new
            {
                TER_ETAT_RESSOURCEID = table.Column<int>(type: "INTEGER", nullable: false),
                TS_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false),
                TRS_FICHIER_OBLIGATOIRE = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true, defaultValueSql: "('N')"),
                TRS_COMMENTAIRE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRS_RESSOURCE_SCENARIOS", x => new { x.TER_ETAT_RESSOURCEID, x.TS_SCENARIOID });
                table.ForeignKey(
                    name: "FK_TRS_RESSOURCE_SCENARIOS$TER_ETAT_RESSOURCES",
                    column: x => x.TER_ETAT_RESSOURCEID,
                    principalTable: "TER_ETAT_RESSOURCES",
                    principalColumn: "TER_ETAT_RESSOURCEID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TRS_RESSOURCE_SCENARIOS$TS_SCENARIOS",
                    column: x => x.TS_SCENARIOID,
                    principalTable: "TS_SCENARIOS",
                    principalColumn: "TS_SCENARIOID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TBD_BATCH_DEMANDE$TD_DEMANDES",
            table: "TBD_BATCH_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TE_ETATID", "TBD_ORDRE_EXECUTION" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TBD_BATCH_DEMANDES_TEB_ETAT_BATCHID",
            table: "TBD_BATCH_DEMANDES",
            column: "TEB_ETAT_BATCHID");

        migrationBuilder.CreateIndex(
            name: "IX_TBD_BATCH_DEMANDES$ETAT_BATCH",
            table: "TBD_BATCH_DEMANDES",
            columns: new[] { "TE_ETATID", "TBD_ORDRE_EXECUTION" });

        migrationBuilder.CreateIndex(
            name: "IX_TBD_BATCH_DEMANDES$TD_DEMANDEID",
            table: "TBD_BATCH_DEMANDES",
            column: "TD_DEMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TBS_BATCH_SCENARIOS_TS_SCENARIOID",
            table: "TBS_BATCH_SCENARIOS",
            column: "TS_SCENARIOID");

        migrationBuilder.CreateIndex(
            name: "IX_TC_CATEGORIES_TP_PERIMETREID",
            table: "TC_CATEGORIES",
            column: "TP_PERIMETREID");

        migrationBuilder.CreateIndex(
            name: "IX_TD_DEMANDES$ETAT",
            table: "TD_DEMANDES",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "IX_TD_DEMANDES$NUM_CP",
            table: "TD_DEMANDES",
            column: "TRU_DEMANDEURID");

        migrationBuilder.CreateIndex(
            name: "IX_TD_DEMANDES$SERVEUR",
            table: "TD_DEMANDES",
            column: "TSRV_SERVEURID");

        migrationBuilder.CreateIndex(
            name: "UK_TD_DEMANDES_TD_DEMANDEID_TE_ETATID",
            table: "TD_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TE_ETATID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_TD_DEMANDES",
            table: "TD_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TE_ETATID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TDQ_DEMANDE_QUALIFS_TD_DEMANDEID",
            table: "TDQ_DEMANDE_QUALIFS",
            column: "TD_DEMANDEID");

        migrationBuilder.CreateIndex(
            name: "IX_TE_ETATS$TEM_ETAT_MASTERS",
            table: "TE_ETATS",
            column: "TEM_ETAT_MASTERID");

        migrationBuilder.CreateIndex(
            name: "IX_TEB_ETAT_BATCHS$TE_ETATS",
            table: "TEB_ETAT_BATCHS",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "IX_TEL_ETAT_LOGICIELS_TL_LOGICIELID",
            table: "TEL_ETAT_LOGICIELS",
            column: "TL_LOGICIELID");

        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS_TC_CATEGORIEID_TP_PERIMETREID",
            table: "TEM_ETAT_MASTERS",
            columns: new[] { "TC_CATEGORIEID", "TP_PERIMETREID" });

        migrationBuilder.CreateIndex(
            name: "IX_TEM_ETAT_MASTERS$PERIMETRE",
            table: "TEM_ETAT_MASTERS",
            column: "TP_PERIMETREID");

        migrationBuilder.CreateIndex(
            name: "UQ_TEM_ETAT_MASTER",
            table: "TEM_ETAT_MASTERS",
            columns: new[] { "TEM_NOM_ETAT_MASTER", "TP_PERIMETREID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TEMF_ETAT_MASTER_FERMES_TF_FERMEID",
            table: "TEMF_ETAT_MASTER_FERMES",
            column: "TF_FERMEID");

        migrationBuilder.CreateIndex(
            name: "UK_TEMF_ETAT_MASTER_FERMES",
            table: "TEMF_ETAT_MASTER_FERMES",
            columns: new[] { "TEM_ETAT_MASTERID", "TF_FERMEID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_TEMF_ETAT_MASTER_FERMES",
            table: "TEMF_ETAT_MASTER_FERMES",
            columns: new[] { "TEM_ETAT_MASTERID", "TF_FERMEID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TEP_ETAT_PREREQUISS_TE_ETATID",
            table: "TEP_ETAT_PREREQUISS",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "IX_TEP_ETAT_PREREQUISS_TRST_STATUTID",
            table: "TEP_ETAT_PREREQUISS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TER_ETAT_RESSOURCES$ETATID",
            table: "TER_ETAT_RESSOURCES",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "UK_TER_ETAT_RESSOURCES$ETATIDNOM_FICHIER",
            table: "TER_ETAT_RESSOURCES",
            columns: new[] { "TE_ETATID", "TER_NOM_FICHIER" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TF_FERMES_TRST_STATUTID",
            table: "TF_FERMES",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TI_INFOS_TRST_STATUTID",
            table: "TI_INFOS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TL_LOGICIELS_TLE_LOGICIEL_EDITEURID",
            table: "TL_LOGICIELS",
            column: "TLE_LOGICIEL_EDITEURID");

        migrationBuilder.CreateIndex(
            name: "IX_TLEM_LOGICIEL_EDITEUR_MODELES_TLE_LOGICIEL_EDITEURID",
            table: "TLEM_LOGICIEL_EDITEUR_MODELES",
            column: "TLE_LOGICIEL_EDITEURID");

        migrationBuilder.CreateIndex(
            name: "IX_TP_PERIMETRES_TQM_QUALIF_MODELEID",
            table: "TP_PERIMETRES",
            column: "TQM_QUALIF_MODELEID");

        migrationBuilder.CreateIndex(
            name: "IX_TP_PERIMETRES_TRST_STATUTID",
            table: "TP_PERIMETRES",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TPD_PREREQUIS_DEMANDES_TEP_ETAT_PREREQUISID",
            table: "TPD_PREREQUIS_DEMANDES",
            column: "TEP_ETAT_PREREQUISID");

        migrationBuilder.CreateIndex(
            name: "IX_TPF_PLANIFS_TPF_DEMANDE_ORIGINEID",
            table: "TPF_PLANIFS",
            column: "TPF_DEMANDE_ORIGINEID");

        migrationBuilder.CreateIndex(
            name: "IX_TPF_PLANIFS_TRST_STATUTID",
            table: "TPF_PLANIFS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TPR_PROFILS_TRST_STATUTID",
            table: "TPR_PROFILS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TPS_PREREQUIS_SCENARIOS_TS_SCENARIOID",
            table: "TPS_PREREQUIS_SCENARIOS",
            column: "TS_SCENARIOID");

        migrationBuilder.CreateIndex(
            name: "IX_TPU_PARALLELEUS_TRST_STATUTID",
            table: "TPU_PARALLELEUS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TPU_PARALLELEUS_TSRV_SERVEURID",
            table: "TPU_PARALLELEUS",
            column: "TSRV_SERVEURID");

        migrationBuilder.CreateIndex(
            name: "UQ_TPU_PARALLELEUS",
            table: "TPU_PARALLELEUS",
            columns: new[] { "TPU_INSTANCE", "TSRV_SERVEURID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TPUF_PARALLELEU_FERMES_TF_FERMEID",
            table: "TPUF_PARALLELEU_FERMES",
            column: "TF_FERMEID");

        migrationBuilder.CreateIndex(
            name: "IX_TPUF_PARALLELEU_FERMES_TPU_PARALLELEUID",
            table: "TPUF_PARALLELEU_FERMES",
            column: "TPU_PARALLELEUID");

        migrationBuilder.CreateIndex(
            name: "IX_TPUF_PARALLELEU_FERMES_TRST_STATUTID",
            table: "TPUF_PARALLELEU_FERMES",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "IX_TPUP_PARALLELEU_PARAMS_TRST_STATUTID",
            table: "TPUP_PARALLELEU_PARAMS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "UQ_PARALLELEU_PARAM",
            table: "TPUP_PARALLELEU_PARAMS",
            columns: new[] { "TPU_PARALLELEUID", "TPUP_KEY" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TRCLICL_CLIENTAPPLICATIONS_CLAIMS_TRCL_CLAIMID",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            column: "TRCL_CLAIMID");

        migrationBuilder.CreateIndex(
            name: "UQ_TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            columns: new[] { "TRCLI_CLIENTAPPLICATIONID", "TRCL_CLAIMID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TRD_RESSOURCE_DEMANDES_TER_ETAT_RESSOURCEID",
            table: "TRD_RESSOURCE_DEMANDES",
            column: "TER_ETAT_RESSOURCEID");

        migrationBuilder.CreateIndex(
            name: "IX_TRD_RESSOURCE_DEMANDES$ETAT_RESSOURCE",
            table: "TRD_RESSOURCE_DEMANDES",
            columns: new[] { "TD_DEMANDEID", "TE_ETATID", "TRD_NOM_FICHIER" });

        migrationBuilder.CreateIndex(
            name: "IX_TRD_RESSOURCE_DEMANDES$TE_ETATID",
            table: "TRD_RESSOURCE_DEMANDES",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "IX_TRS_RESSOURCE_SCENARIOS$ETAT_RESSOURCE",
            table: "TRS_RESSOURCE_SCENARIOS",
            column: "TER_ETAT_RESSOURCEID");

        migrationBuilder.CreateIndex(
            name: "IX_TRS_RESSOURCE_SCENARIOS$TS_SCENARIOS",
            table: "TRS_RESSOURCE_SCENARIOS",
            column: "TS_SCENARIOID");

        migrationBuilder.CreateIndex(
            name: "IX_TRUCL_UTILISATEURS_CLAIMS_TRCL_CLAIMID",
            table: "TRUCL_UTILISATEURS_CLAIMS",
            column: "TRCL_CLAIMID");

        migrationBuilder.CreateIndex(
            name: "UQ_TRUCL_UTILISATEURS_CLAIMS",
            table: "TRUCL_UTILISATEURS_CLAIMS",
            columns: new[] { "TRU_UTILISATEURID", "TRCL_CLAIMID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TS_SCENARIOS_TE_ETATID",
            table: "TS_SCENARIOS",
            column: "TE_ETATID");

        migrationBuilder.CreateIndex(
            name: "IX_TSL_SERVEUR_LOGICIELS_TL_LOGICIELID",
            table: "TSL_SERVEUR_LOGICIELS",
            column: "TL_LOGICIELID");

        migrationBuilder.CreateIndex(
            name: "UQ_SERVEUR_LOGICIEL",
            table: "TSL_SERVEUR_LOGICIELS",
            columns: new[] { "TSRV_SERVEURID", "TL_LOGICIELID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TSP_SERVEUR_PARAMS_TRST_STATUTID",
            table: "TSP_SERVEUR_PARAMS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "UQ_TSP_SERVEUR_PARAM",
            table: "TSP_SERVEUR_PARAMS",
            columns: new[] { "TSRV_SERVEURID", "TSP_KEY" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TSRV_SERVEURS_TRST_STATUTID",
            table: "TSRV_SERVEURS",
            column: "TRST_STATUTID");

        migrationBuilder.CreateIndex(
            name: "UQ_TSRV_SERVEURS$TSRV_NOM",
            table: "TSRV_SERVEURS",
            column: "TSRV_NOM",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TTAU_AUTHENTIFICATIONS_TRU_UTILISATEURID",
            table: "TTAU_AUTHENTIFICATIONS",
            column: "TRU_UTILISATEURID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TBD_BATCH_DEMANDES");

        migrationBuilder.DropTable(
            name: "TBS_BATCH_SCENARIOS");

        migrationBuilder.DropTable(
            name: "TDQ_DEMANDE_QUALIFS");

        migrationBuilder.DropTable(
            name: "TEL_ETAT_LOGICIELS");

        migrationBuilder.DropTable(
            name: "TEMF_ETAT_MASTER_FERMES");

        migrationBuilder.DropTable(
            name: "TI_INFOS");

        migrationBuilder.DropTable(
            name: "TLEM_LOGICIEL_EDITEUR_MODELES");

        migrationBuilder.DropTable(
            name: "TPD_PREREQUIS_DEMANDES");

        migrationBuilder.DropTable(
            name: "TPF_PLANIFS");

        migrationBuilder.DropTable(
            name: "TPM_PARAMS");

        migrationBuilder.DropTable(
            name: "TPR_PROFILS");

        migrationBuilder.DropTable(
            name: "TPS_PREREQUIS_SCENARIOS");

        migrationBuilder.DropTable(
            name: "TPUF_PARALLELEU_FERMES");

        migrationBuilder.DropTable(
            name: "TPUP_PARALLELEU_PARAMS");

        migrationBuilder.DropTable(
            name: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS");

        migrationBuilder.DropTable(
            name: "TRD_RESSOURCE_DEMANDES");

        migrationBuilder.DropTable(
            name: "TRLG_LNGS");

        migrationBuilder.DropTable(
            name: "TRS_RESSOURCE_SCENARIOS");

        migrationBuilder.DropTable(
            name: "TRTZ_TZS");

        migrationBuilder.DropTable(
            name: "TRUCL_UTILISATEURS_CLAIMS");

        migrationBuilder.DropTable(
            name: "TSL_SERVEUR_LOGICIELS");

        migrationBuilder.DropTable(
            name: "TSP_SERVEUR_PARAMS");

        migrationBuilder.DropTable(
            name: "TTAU_AUTHENTIFICATIONS");

        migrationBuilder.DropTable(
            name: "TEB_ETAT_BATCHS");

        migrationBuilder.DropTable(
            name: "TEP_ETAT_PREREQUISS");

        migrationBuilder.DropTable(
            name: "TF_FERMES");

        migrationBuilder.DropTable(
            name: "TPU_PARALLELEUS");

        migrationBuilder.DropTable(
            name: "TRCLI_CLIENTAPPLICATIONS");

        migrationBuilder.DropTable(
            name: "TD_DEMANDES");

        migrationBuilder.DropTable(
            name: "TER_ETAT_RESSOURCES");

        migrationBuilder.DropTable(
            name: "TS_SCENARIOS");

        migrationBuilder.DropTable(
            name: "TRCL_CLAIMS");

        migrationBuilder.DropTable(
            name: "TL_LOGICIELS");

        migrationBuilder.DropTable(
            name: "TRU_UTILISATEURS");

        migrationBuilder.DropTable(
            name: "TSRV_SERVEURS");

        migrationBuilder.DropTable(
            name: "TE_ETATS");

        migrationBuilder.DropTable(
            name: "TLE_LOGICIEL_EDITEURS");

        migrationBuilder.DropTable(
            name: "TEM_ETAT_MASTERS");

        migrationBuilder.DropTable(
            name: "TC_CATEGORIES");

        migrationBuilder.DropTable(
            name: "TP_PERIMETRES");

        migrationBuilder.DropTable(
            name: "TQM_QUALIF_MODELES");

        migrationBuilder.DropTable(
            name: "TRST_STATUTS");
    }
}
