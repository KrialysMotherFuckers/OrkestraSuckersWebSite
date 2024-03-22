using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TTAU_AUTHENTIFICATIONS_TRU_UTILISATEURS",
            table: "TTAU_AUTHENTIFICATIONS");

        migrationBuilder.DropTable(
            name: "TRUCL_UTILISATEURS_CLAIMS");

        migrationBuilder.DropTable(
            name: "TRU_UTILISATEURS");

        migrationBuilder.DropIndex(
            name: "UQ_TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TD_FULL_RESULT_FILE_PATH",
            table: "TD_DEMANDES");

        migrationBuilder.RenameColumn(
            name: "TRU_UTILISATEURID",
            table: "TTAU_AUTHENTIFICATIONS",
            newName: "TRU_USERID");

        migrationBuilder.RenameIndex(
            name: "IX_TTAU_AUTHENTIFICATIONS_TRU_UTILISATEURID",
            table: "TTAU_AUTHENTIFICATIONS",
            newName: "IX_TTAU_AUTHENTIFICATIONS_TRU_USERID");

        migrationBuilder.RenameColumn(
            name: "TRCLI_STATUT",
            table: "TRCLI_CLIENTAPPLICATIONS",
            newName: "TRCLI_STATUS");

        migrationBuilder.RenameColumn(
            name: "TRCLI_LIB",
            table: "TRCLI_CLIENTAPPLICATIONS",
            newName: "TRCLI_LABEL");

        migrationBuilder.AlterColumn<string>(
            name: "TTAU_REFRESH_TOKEN_EXP",
            table: "TTAU_AUTHENTIFICATIONS",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<string>(
            name: "TTAU_AUTH_CODE_EXP",
            table: "TTAU_AUTHENTIFICATIONS",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "TEXT");

        migrationBuilder.AddColumn<string>(
            name: "TRCLICL_CLAIM_VALUE",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            type: "TEXT",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TRCLI_MULTIVALUE",
            table: "TRCL_CLAIMS",
            type: "TEXT",
            maxLength: 1,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TRCLI_STATUS",
            table: "TRCL_CLAIMS",
            type: "TEXT",
            maxLength: 1,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TRU_DECLARANTID",
            table: "TPF_PLANIFS",
            type: "TEXT",
            maxLength: 36,
            nullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TE_DATE_DERNIERE_PRODUCTION",
            table: "TE_ETATS",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TD_DATE_EXECUTION_SOUHAITEE",
            table: "TD_DEMANDES",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "TEXT");

        migrationBuilder.CreateTable(
            name: "PARALLELEU",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
            });

        migrationBuilder.CreateTable(
            name: "TRAS_AUTH_SCENARIOS",
            columns: table => new
            {
                TRAS_AUTH_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRAS_LABEL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRAS_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRAS_AUTH_SCENARIOS", x => x.TRAS_AUTH_SCENARIOID);
            });

        migrationBuilder.CreateTable(
            name: "TRCCL_CATALOG_CLAIMS",
            columns: table => new
            {
                TRCCL_CATALOG_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRCCL_STATUS = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TRCL_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false),
                TRCCL_VALUE = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                TRCCL_DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRCCL_KIND = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRCCL_CATALOG_CLAIMS", x => x.TRCCL_CATALOG_CLAIMID);
                table.ForeignKey(
                    name: "FK_RCCL_CATALOG_CLAIMS_TTRCL_CLAIMS",
                    column: x => x.TRCL_CLAIMID,
                    principalTable: "TRCL_CLAIMS",
                    principalColumn: "TRCL_CLAIMID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TRU_USERS",
            columns: table => new
            {
                TRU_USERID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TRU_STATUS = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                TRU_LOGIN = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_PWD = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_FIRST_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_EMAIL = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRU_USERS", x => x.TRU_USERID);
            });

        migrationBuilder.CreateTable(
            name: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",
            columns: table => new
            {
                TRAPLAS_APPLICATIONS_AUTH_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRAS_AUTH_SCENARIOID = table.Column<int>(type: "INTEGER", nullable: false),
                TRCLI_CLIENTAPPLICATIONID = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS", x => x.TRAPLAS_APPLICATIONS_AUTH_SCENARIOID);
                table.ForeignKey(
                    name: "FK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOS_TRAS_AUTH_SCENARIOID",
                    column: x => x.TRAS_AUTH_SCENARIOID,
                    principalTable: "TRAS_AUTH_SCENARIOS",
                    principalColumn: "TRAS_AUTH_SCENARIOID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRCLI_CLIENTAPPLICATIONS",
                    column: x => x.TRCLI_CLIENTAPPLICATIONID,
                    principalTable: "TRCLI_CLIENTAPPLICATIONS",
                    principalColumn: "TRCLI_APPLICATIONID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TRUCL_USERS_CLAIMS",
            columns: table => new
            {
                TRUCL_USER_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRU_USERID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                TRCL_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false),
                TRUCL_CLAIM_VALUE = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRUCL_USERS_CLAIMS", x => x.TRUCL_USER_CLAIMID);
                table.ForeignKey(
                    name: "FK_TRUCL_USERS_CLAIMS_TRCL_CLAIMS",
                    column: x => x.TRCL_CLAIMID,
                    principalTable: "TRCL_CLAIMS",
                    principalColumn: "TRCL_CLAIMID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TRUCL_USERS_CLAIMS_TRU_USERS",
                    column: x => x.TRU_USERID,
                    principalTable: "TRU_USERS",
                    principalColumn: "TRU_USERID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "UQ_TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            columns: new[] { "TRCLI_CLIENTAPPLICATIONID", "TRCL_CLAIMID" });

        migrationBuilder.CreateIndex(
            name: "IX_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",
            table: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",
            columns: new[] { "TRAS_AUTH_SCENARIOID", "TRCLI_CLIENTAPPLICATIONID" });

        migrationBuilder.CreateIndex(
            name: "IX_TRAPLAS_APPLICATIONS_AUTH_SCENARIOS_TRCLI_CLIENTAPPLICATIONID",
            table: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",
            column: "TRCLI_CLIENTAPPLICATIONID");

        migrationBuilder.CreateIndex(
            name: "IX_TRCCL_CATALOG_CLAIMS_TRCL_CLAIMID",
            table: "TRCCL_CATALOG_CLAIMS",
            column: "TRCL_CLAIMID");

        migrationBuilder.CreateIndex(
            name: "IX_TRUCL_USERS_CLAIMS_TRCL_CLAIMID",
            table: "TRUCL_USERS_CLAIMS",
            column: "TRCL_CLAIMID");

        migrationBuilder.CreateIndex(
            name: "UQ_TRUCL_USERS_CLAIMS",
            table: "TRUCL_USERS_CLAIMS",
            columns: new[] { "TRU_USERID", "TRCL_CLAIMID" });

        migrationBuilder.AddForeignKey(
            name: "FK_TTAU_AUTHENTIFICATIONS_TRU_USERS",
            table: "TTAU_AUTHENTIFICATIONS",
            column: "TRU_USERID",
            principalTable: "TRU_USERS",
            principalColumn: "TRU_USERID",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TTAU_AUTHENTIFICATIONS_TRU_USERS",
            table: "TTAU_AUTHENTIFICATIONS");

        migrationBuilder.DropTable(
            name: "PARALLELEU");

        migrationBuilder.DropTable(
            name: "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS");

        migrationBuilder.DropTable(
            name: "TRCCL_CATALOG_CLAIMS");

        migrationBuilder.DropTable(
            name: "TRUCL_USERS_CLAIMS");

        migrationBuilder.DropTable(
            name: "TRAS_AUTH_SCENARIOS");

        migrationBuilder.DropTable(
            name: "TRU_USERS");

        migrationBuilder.DropIndex(
            name: "UQ_TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRCLICL_CLAIM_VALUE",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRCLI_MULTIVALUE",
            table: "TRCL_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRCLI_STATUS",
            table: "TRCL_CLAIMS");

        migrationBuilder.DropColumn(
            name: "TRU_DECLARANTID",
            table: "TPF_PLANIFS");

        migrationBuilder.RenameColumn(
            name: "TRU_USERID",
            table: "TTAU_AUTHENTIFICATIONS",
            newName: "TRU_UTILISATEURID");

        migrationBuilder.RenameIndex(
            name: "IX_TTAU_AUTHENTIFICATIONS_TRU_USERID",
            table: "TTAU_AUTHENTIFICATIONS",
            newName: "IX_TTAU_AUTHENTIFICATIONS_TRU_UTILISATEURID");

        migrationBuilder.RenameColumn(
            name: "TRCLI_STATUS",
            table: "TRCLI_CLIENTAPPLICATIONS",
            newName: "TRCLI_STATUT");

        migrationBuilder.RenameColumn(
            name: "TRCLI_LABEL",
            table: "TRCLI_CLIENTAPPLICATIONS",
            newName: "TRCLI_LIB");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TTAU_REFRESH_TOKEN_EXP",
            table: "TTAU_AUTHENTIFICATIONS",
            type: "TEXT",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TTAU_AUTH_CODE_EXP",
            table: "TTAU_AUTHENTIFICATIONS",
            type: "TEXT",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TE_DATE_DERNIERE_PRODUCTION",
            table: "TE_ETATS",
            type: "TEXT",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(DateTimeOffset),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TD_DATE_EXECUTION_SOUHAITEE",
            table: "TD_DEMANDES",
            type: "TEXT",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(DateTimeOffset),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TD_FULL_RESULT_FILE_PATH",
            table: "TD_DEMANDES",
            type: "TEXT",
            maxLength: 255,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TRU_UTILISATEURS",
            columns: table => new
            {
                TRU_UTILISATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                TRU_EMAIL = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                TRU_FIRST_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_LOGIN = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_PWD = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                TRU_STATUT = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TRU_UTILISATEURS", x => x.TRU_UTILISATEURID);
            });

        migrationBuilder.CreateTable(
            name: "TRUCL_UTILISATEURS_CLAIMS",
            columns: table => new
            {
                TRUCL_UTILISATEUR_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TRCL_CLAIMID = table.Column<int>(type: "INTEGER", nullable: false),
                TRUCL_CLAIM_VALUE = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                TRU_UTILISATEURID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true)
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

        migrationBuilder.CreateIndex(
            name: "UQ_TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            table: "TRCLICL_CLIENTAPPLICATIONS_CLAIMS",
            columns: new[] { "TRCLI_CLIENTAPPLICATIONID", "TRCL_CLAIMID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TRUCL_UTILISATEURS_CLAIMS_TRCL_CLAIMID",
            table: "TRUCL_UTILISATEURS_CLAIMS",
            column: "TRCL_CLAIMID");

        migrationBuilder.CreateIndex(
            name: "UQ_TRUCL_UTILISATEURS_CLAIMS",
            table: "TRUCL_UTILISATEURS_CLAIMS",
            columns: new[] { "TRU_UTILISATEURID", "TRCL_CLAIMID" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_TTAU_AUTHENTIFICATIONS_TRU_UTILISATEURS",
            table: "TTAU_AUTHENTIFICATIONS",
            column: "TRU_UTILISATEURID",
            principalTable: "TRU_UTILISATEURS",
            principalColumn: "TRU_UTILISATEURID",
            onDelete: ReferentialAction.Restrict);
    }
}
