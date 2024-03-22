using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers43 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW VACCGD_ACCUEIL_GRAPHE_DEMANDES");
        migrationBuilder.Sql("DROP VIEW VACCGQ_ACCUEIL_GRAPHE_QUALITES");
        migrationBuilder.Sql("DROP VIEW VDE_DEMANDES_ETENDUES");
        migrationBuilder.Sql("DROP VIEW VDE_DEMANDES_RESSOURCES");
        migrationBuilder.Sql("DROP VIEW VDTFH_HABILITATIONS");
        migrationBuilder.Sql("DROP VIEW VPD_PLANIF_DETAILS");
        migrationBuilder.Sql("DROP VIEW VPE_PLANIF_ENTETES");
        migrationBuilder.Sql("DROP VIEW VSCU_CTRL_STRUCTURE_UPLOADS");

        migrationBuilder.Sql(@"UPDATE TRU_USERS SET TRLG_LNGID='fr-FR';");
        migrationBuilder.Sql(@"UPDATE TRLG_LNGS SET TRLG_LNGID='fr-FR' where TRLG_LNGID='FR';");
        migrationBuilder.Sql(@"DELETE FROM TMT_MAIL_TEMPLATES;");

        migrationBuilder.AlterColumn<string>(
            name: "TRU_STATUS",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 1,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 1,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TRU_NAME",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TRU_LOGIN",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TRU_FIRST_NAME",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TRTZ_TZID",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TRLG_LNGID",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.Sql("DROP TABLE IF EXISTS ef_temp_TRU_USERS;");

        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_TRU_FULLNAME_TRU_USERS ON TRU_USERS(TRU_NAME, TRU_FIRST_NAME);");
        //migrationBuilder.CreateIndex(
        //    name: "IX_TRU_FULLNAME_TRU_USERS",
        //    table: "TRU_USERS",
        //    columns: new[] { "TRU_NAME", "TRU_FIRST_NAME" },
        //    unique: true);

        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_TRU_LOGIN_TRU_USERS ON TRU_USERS(TRU_LOGIN);");
        //migrationBuilder.CreateIndex(
        //    name: "IX_TRU_LOGIN_TRU_USERS",
        //    table: "TRU_USERS",
        //    column: "TRU_LOGIN",
        //    unique: true);

        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_TRU_USERS_TRLG_LNGID ON TRU_USERS(TRLG_LNGID);");
        //migrationBuilder.CreateIndex(
        //    name: "IX_TRU_USERS_TRLG_LNGID",
        //    table: "TRU_USERS",
        //    column: "TRLG_LNGID");

        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_TRU_USERS_TRTZ_TZID ON TRU_USERS(TRTZ_TZID);");
        //migrationBuilder.CreateIndex(
        //    name: "IX_TRU_USERS_TRTZ_TZID",
        //    table: "TRU_USERS",
        //    column: "TRTZ_TZID");

        migrationBuilder.AddForeignKey(
            name: "FK_TRU_USERS_TRLG_LNGS_TRLG_LNGID",
            table: "TRU_USERS",
            column: "TRLG_LNGID",
            principalTable: "TRLG_LNGS",
            principalColumn: "TRLG_LNGID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TRU_USERS_TRTZ_TZS_TRTZ_TZID",
            table: "TRU_USERS",
            column: "TRTZ_TZID",
            principalTable: "TRTZ_TZS",
            principalColumn: "TRTZ_TZID",
            onDelete: ReferentialAction.Cascade);

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TRU_USERS_TRLG_LNGS_TRLG_LNGID",
            table: "TRU_USERS");

        migrationBuilder.DropForeignKey(
            name: "FK_TRU_USERS_TRTZ_TZS_TRTZ_TZID",
            table: "TRU_USERS");

        migrationBuilder.DropIndex(
            name: "IX_TRU_FULLNAME_TRU_USERS",
            table: "TRU_USERS");

        migrationBuilder.DropIndex(
            name: "IX_TRU_LOGIN_TRU_USERS",
            table: "TRU_USERS");

        migrationBuilder.DropIndex(
            name: "IX_TRU_USERS_TRLG_LNGID",
            table: "TRU_USERS");

        migrationBuilder.DropIndex(
            name: "IX_TRU_USERS_TRTZ_TZID",
            table: "TRU_USERS");

        migrationBuilder.AlterColumn<string>(
            name: "TRU_STATUS",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 1,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 1);

        migrationBuilder.AlterColumn<string>(
            name: "TRU_NAME",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "TRU_LOGIN",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "TRU_FIRST_NAME",
            table: "TRU_USERS",
            type: "TEXT",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "TRTZ_TZID",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<string>(
            name: "TRLG_LNGID",
            table: "TRU_USERS",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");
    }
}
