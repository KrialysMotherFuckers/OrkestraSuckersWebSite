using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers42 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TRLG_LNGID",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TRTZ_TZID",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TCMD_COMMENTAIRE",
            table: "TCMD_COMMANDES",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.CreateTable(
            name: "TMT_MAIL_TEMPLATES",
            columns: table => new
            {
                TMTMAILTEMPLATEID = table.Column<int>(name: "TMT_MAIL_TEMPLATEID", type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TMTCODE = table.Column<string>(name: "TMT_CODE", type: "TEXT", nullable: false),
                TMTCODEN2 = table.Column<string>(name: "TMT_CODEN2", type: "TEXT", nullable: true),
                TMTLIB = table.Column<string>(name: "TMT_LIB", type: "TEXT", nullable: true),
                TRSTSTATUTID = table.Column<string>(name: "TRST_STATUTID", type: "TEXT", maxLength: 3, nullable: true),
                TRLGLNGID = table.Column<string>(name: "TRLG_LNGID", type: "TEXT", nullable: true),
                TMTOBJET = table.Column<string>(name: "TMT_OBJET", type: "TEXT", nullable: false),
                TMTCORPS = table.Column<string>(name: "TMT_CORPS", type: "TEXT", nullable: false),
                TMTPIED = table.Column<string>(name: "TMT_PIED", type: "TEXT", nullable: false),
                TMTCOMMENTAIRE = table.Column<string>(name: "TMT_COMMENTAIRE", type: "TEXT", nullable: true),
                TMTIMPORTANCE = table.Column<string>(name: "TMT_IMPORTANCE", type: "TEXT", maxLength: 1, nullable: true),
                TMTDATECREATION = table.Column<DateTime>(name: "TMT_DATE_CREATION", type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TMT_MAIL_TEMPLATES", x => x.TMTMAILTEMPLATEID);
                table.ForeignKey(
                    name: "FK_TMT_MAIL_TEMPLATES_TRLG_LNGS_TRLG_LNGID",
                    column: x => x.TRLGLNGID,
                    principalTable: "TRLG_LNGS",
                    principalColumn: "TRLG_LNGID");
                table.ForeignKey(
                    name: "FK_TMT_MAIL_TEMPLATES_TRST_STATUTS_TRST_STATUTID",
                    column: x => x.TRSTSTATUTID,
                    principalTable: "TRST_STATUTS",
                    principalColumn: "TRST_STATUTID");
            });

        migrationBuilder.CreateIndex(
            name: "IX_TMT_CODE$TMT_MAIL_TEMPLATES",
            table: "TMT_MAIL_TEMPLATES",
            column: "TMT_CODE");

        migrationBuilder.CreateIndex(
            name: "IX_TMT_MAIL_TEMPLATES_TRLG_LNGID",
            table: "TMT_MAIL_TEMPLATES",
            column: "TRLG_LNGID");

        migrationBuilder.CreateIndex(
            name: "IX_TMT_MAIL_TEMPLATES_TRST_STATUTID",
            table: "TMT_MAIL_TEMPLATES",
            column: "TRST_STATUTID");



        /* addon sur données existantes */
        migrationBuilder.Sql("INSERT INTO TRLG_LNGS(TRLG_LNGID, TRLG_PREFERED_LNG) Values('FR',1);");
        migrationBuilder.Sql("INSERT INTO TRLG_LNGS(TRLG_LNGID, TRLG_PREFERED_LNG) Values('EN',0);");

        migrationBuilder.Sql("INSERT INTO TRTZ_TZS (TRTZ_TZID, TRTZ_PREFERED_TZ, TRTZ_INFO_TZ) VALUES('Europe/Paris', 1, '');");


        migrationBuilder.Sql("UPDATE TRU_USERS SET TRLG_LNGID = 'FR';");
        migrationBuilder.Sql("UPDATE TRU_USERS SET TRTZ_TZID = 'Europe/Paris';");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TMT_MAIL_TEMPLATES");

        migrationBuilder.DropColumn(
            name: "TRLG_LNGID",
            table: "TRU_USERS");

        migrationBuilder.DropColumn(
            name: "TRTZ_TZID",
            table: "TRU_USERS");

        migrationBuilder.AlterColumn<string>(
            name: "TCMD_COMMENTAIRE",
            table: "TCMD_COMMANDES",
            type: "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);
    }
}
