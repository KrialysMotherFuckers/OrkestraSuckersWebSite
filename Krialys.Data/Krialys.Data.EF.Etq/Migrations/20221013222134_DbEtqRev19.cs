using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev19 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"UPDATE TOBJE_OBJET_ETIQUETTES SET TOBJE_LIB='A RENSEIGNER' where  TOBJE_LIB is null or LENGTH(TOBJE_LIB)<1;");

        migrationBuilder.Sql(@"UPDATE TETQ_ETIQUETTES SET TETQ_PATTERN=replace(TETQ_PATTERN,'#','¤');");

        migrationBuilder.AddColumn<bool>(
            name: "TPRCP_ALLOW_DTS_ACCESS",
            table: "TPRCP_PRC_PERIMETRES",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AlterColumn<string>(
            name: "TOBJE_LIB",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "TEXT",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 100,
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TPRCP_ALLOW_DTS_ACCESS",
            table: "TPRCP_PRC_PERIMETRES");

        migrationBuilder.AlterColumn<string>(
            name: "TOBJE_LIB",
            table: "TOBJE_OBJET_ETIQUETTES",
            type: "TEXT",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 100);
    }
}
