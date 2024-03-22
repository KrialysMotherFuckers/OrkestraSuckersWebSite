using Microsoft.EntityFrameworkCore.Migrations;

namespace Krialys.Entities.Migrations;

public partial class DbUnivers_13 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"CREATE VIEW IF NOT EXISTS VSCU_CTRL_STRUCTURE_UPLOADS AS
                SELECT TEL_ETAT_LOGICIEL.TE_ETATID, 
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_ACTION,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILE_TYPE,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_PATH_NAME,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILENAME_PATTERN
                FROM  
                    TEL_ETAT_LOGICIELS as TEL_ETAT_LOGICIEL
                    join TL_LOGICIELS as TL_LOGICIEL on TEL_ETAT_LOGICIEL.TL_LOGICIELID = TL_LOGICIEL.TL_LOGICIELID
                    join TLE_LOGICIEL_EDITEURS as TLE_LOGICIEL_EDITEUR  on TL_LOGICIEL.TLE_LOGICIEL_EDITEURID = TLE_LOGICIEL_EDITEUR.TLE_LOGICIEL_EDITEURID
                    join TLEM_LOGICIEL_EDITEUR_MODELES as TLEM_LOGICIEL_EDITEUR_MODELE  on TLE_LOGICIEL_EDITEUR.TLE_LOGICIEL_EDITEURID = TLEM_LOGICIEL_EDITEUR_MODELE.TLE_LOGICIEL_EDITEURID
                group by TEL_ETAT_LOGICIEL.TE_ETATID ,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_ACTION,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILE_TYPE,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_PATH_NAME,
                TLEM_LOGICIEL_EDITEUR_MODELE.TLEM_FILENAME_PATTERN;"
        );

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP VIEW IF EXISTS VSCU_CTRL_STRUCTURE_UPLOADS;");
    }
}
