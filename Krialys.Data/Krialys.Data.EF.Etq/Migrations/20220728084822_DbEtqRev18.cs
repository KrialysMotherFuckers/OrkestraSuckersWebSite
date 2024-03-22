using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbEtqRev18 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        migrationBuilder.Sql(@"UPDATE TPRCP_PRC_PERIMETRES SET TPRCP_LIB='A RENSEIGNER' where  TPRCP_LIB is null or LENGTH(TPRCP_LIB)<1;");

        migrationBuilder.Sql(@"UPDATE TDOM_DOMAINES SET TDOM_LIB='A RENSEIGNER' where  TDOM_LIB is null or LENGTH(TDOM_LIB)<1;");

        migrationBuilder.AlterColumn<string>(
            name: "TPRCP_LIB",
            table: "TPRCP_PRC_PERIMETRES",
            type: "TEXT",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TDOM_LIB",
            table: "TDOM_DOMAINES",
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
        migrationBuilder.AlterColumn<string>(
            name: "TPRCP_LIB",
            table: "TPRCP_PRC_PERIMETRES",
            type: "TEXT",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "TDOM_LIB",
            table: "TDOM_DOMAINES",
            type: "TEXT",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 100);
    }
}
