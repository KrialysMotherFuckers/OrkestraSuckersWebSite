using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

public partial class DbUnivers_35 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
                UPDATE TRCCL_CATALOG_CLAIMS
                    SET TRCCL_VALUE=TRCCL_VALUE*2
                WHERE TRCL_CLAIMID=2 and TRCCL_VALUE*1>2;
            ");

        migrationBuilder.Sql(@"
                INSERT OR REPLACE INTO TRCL_CLAIMS
                    (TRCL_CLAIM_NAME, TRCL_CLAIM_DESCRIPTION, TRCLI_MULTIVALUE, TRCL_STATUS)
                VALUES
                    ('Role', 'Rôle', 'F', 'A'),
                    ('TokenLifetime', 'Durée de vie du jeton d authentification en secondes (Remplace la valeur définie dans appsettings.json)', 'F', 'A');
            ");

        migrationBuilder.Sql(@"
                INSERT INTO TRCCL_CATALOG_CLAIMS
                    (TRCCL_STATUS, TRCL_CLAIMID, TRCCL_VALUE, TRCCL_DESCRIPTION, TRCCL_KIND, TRCCL_VALUE_LABEL)
                VALUES
                    ('A', 1, '4', 'Droits gérés au niveau des Data métier', 'U', 'Data Driven');
            ");

        migrationBuilder.Sql(@"UPDATE TRUCL_USERS_CLAIMS set 
                TRUCL_CLAIM_VALUE= TRUCL_CLAIM_VALUE * 2  
                where 
                TRCL_CLAIMID = 2 and TRUCL_CLAIM_VALUE*1> 3
                and TRUCL_CLAIM_VALUE<>10;");

        migrationBuilder.Sql(@" UPDATE TRUCL_USERS_CLAIMS set 
                TRUCL_CLAIM_VALUE= 8 
                where 
                TRCL_CLAIMID = 2 
                and TRCLI_CLIENTAPPLICATIONID=7
                and TRUCL_CLAIM_VALUE=10;");

        migrationBuilder.Sql(@" UPDATE TRUCL_USERS_CLAIMS set 
                TRUCL_CLAIM_VALUE= 18 
                where 
                TRCL_CLAIMID = 2 
                and TRCLI_CLIENTAPPLICATIONID<>7
                and TRUCL_CLAIM_VALUE=10;");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
