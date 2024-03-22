using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations.ETQ;

public partial class DbEtqRev09 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"update TPRS_PROCESSUS SET TPRS_DATE_CREATION = SUBSTR(TPRS_DATE_CREATION, 1, 19);");
        migrationBuilder.Sql(@"update TOBJE_OBJET_ETIQUETTES SET TOBJE_DATE_CREATION = SUBSTR(TOBJE_DATE_CREATION, 1, 19);");
        migrationBuilder.Sql(@"update TPRCP_PRC_PERIMETRES SET TPRCP_DATE_CREATION = SUBSTR(TPRCP_DATE_CREATION, 1, 19);");
        migrationBuilder.Sql(@"update TSEQ_SUIVI_EVENEMENT_ETQS SET TSEQ_DATE_EVENEMENT = SUBSTR(TSEQ_DATE_EVENEMENT, 1, 19);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
