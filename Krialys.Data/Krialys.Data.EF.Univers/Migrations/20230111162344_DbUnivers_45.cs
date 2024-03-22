using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers45 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TCMD_SP_COMMENTAIRE",
            table: "TCMD_SP_SUIVI_PHASES",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");
        migrationBuilder.Sql(@"update TCMD_PH_PHASES
                    set TCMD_PH_CODE ='AR',
                    TCMD_PH_LIB_FR = 'Archivée',
                    TCMD_PH_LIB_EN ='TRAD_Archivée'
                    where TCMD_PH_CODE ='EX';");

        migrationBuilder.Sql(@"INSERT INTO TCMD_PH_PHASES(TCMD_PH_CODE, TCMD_PH_LIB_FR ,TCMD_PH_LIB_EN)
                values ('AN', 'Annulée', 'TRAD_Annulée');");

        migrationBuilder.Sql(@"delete from TCMD_RP_RAISON_PHASES;");

        migrationBuilder.Sql(@" INSERT INTO TCMD_RP_RAISON_PHASES (  TCMD_RP_LIB_FR,TCMD_RP_LIB_EN,TCMD_PH_PHASEID)
                 SELECT   'Création de la commande', 'TRAD_Création de la commande', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='BR' 
                UNION ALL SELECT 'Commande validée par le client', 'TRAD_Commande validée par le client', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='AA'
                UNION ALL SELECT 'Commande acceptée et assignée', 'TRAD_Commande acceptée et assignée', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='EC'
                UNION ALL SELECT 'Commande incomplète', 'TRAD_Commande incomplète', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='RJ'
                UNION ALL SELECT 'Commande déjà traitée', 'TRAD_Commande déjà traitée', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='RJ'
                UNION ALL SELECT 'Commande non traitable', 'TRAD_Commande non traitable', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='RJ'
                UNION ALL SELECT 'Autre', 'Other', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='RJ'
                UNION ALL  SELECT 'En attente de précision du client', 'TRAD_En attente de précision du client', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='GL'
                UNION ALL SELECT 'En attente de fichier', 'TRAD_En attente de fichier', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='GL'
                UNION ALL SELECT 'Autre', 'Other', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='GL'
                UNION ALL  SELECT 'Commande livrée', 'TRAD_Commande livrée', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='LI'
                UNION ALL SELECT 'Commande terminée', 'TRAD_Commande terminée', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='TE'
                UNION ALL SELECT 'Commande erronée', 'TRAD_Commande erronée', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='AN'
                UNION ALL SELECT 'Commande obsolète', 'TRAD_Commande obsolète', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='AN'
                UNION ALL SELECT 'Autre', 'Other', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='AN'
                UNION ALL SELECT 'Phase d''origine obsolète', 'TRAD_Phase d''origine obsolète', TCMD_PH_PHASEID FROM TCMD_PH_PHASES  where TCMD_PH_CODE ='AR';");

        migrationBuilder.Sql(@"INSERT INTO TRTZ_TZS(TRTZ_TZID, TRTZ_PREFERED_TZ, TRTZ_INFO_TZ) VALUES ('America/New_York',0, '-5');");

        migrationBuilder.Sql(@"INSERT INTO TRTZ_TZS(TRTZ_TZID, TRTZ_PREFERED_TZ, TRTZ_INFO_TZ) VALUES ('Australia/Sydney',0, '+10');");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TCMD_SP_COMMENTAIRE",
            table: "TCMD_SP_SUIVI_PHASES",
            type: "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);
    }
}
