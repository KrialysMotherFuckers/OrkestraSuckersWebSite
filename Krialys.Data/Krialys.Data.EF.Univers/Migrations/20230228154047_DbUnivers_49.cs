using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers_49 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TCMD_DOC_TAILLE",
            table: "TCMD_DOC_DOCUMENTS",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.Sql(@"
                INSERT INTO TPM_PARAMS
                    (TPM_PARAMID, TPM_VALEUR, TPM_INFO)
                VALUES
                    ('Command_DocumentMaxSize', '20971520', 'CMD - unité: octet - soit 20Mo - taille max autorisée'),
                    ('Command_DocumentExtensions','.xls,.xlsx,.doc,.docx,.ppt,.pptx,.csv,.txt,.odt,.ods,.odp,.odf,.pdf', 'CMD - format des fichiers autorisés en upload');
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='Command_DocumentMaxSize')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='Command_DocumentMaxSize') > 1
                AND TPM_PARAMID = 'Command_DocumentMaxSize'
            ");

        migrationBuilder.Sql(@"
                DELETE FROM TPM_PARAMS 
                WHERE 
                    ROWID NOT IN (SELECT MIN(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='Command_DocumentExtensions')
                AND (SELECT COUNT(ROWID) FROM TPM_PARAMS WHERE TPM_PARAMID ='Command_DocumentExtensions') > 1
                AND TPM_PARAMID = 'Command_DocumentExtensions'
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TCMD_DOC_TAILLE",
            table: "TCMD_DOC_DOCUMENTS");
    }
}
