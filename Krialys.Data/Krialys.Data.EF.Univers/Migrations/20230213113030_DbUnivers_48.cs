using Microsoft.EntityFrameworkCore.Migrations;


namespace Krialys.Entities.Migrations;

/// <inheritdoc />
public partial class DbUnivers48 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TMT_MAILING_LIST_CC",
            table: "TMT_MAIL_TEMPLATES",
            type: "TEXT",
            nullable: true);


        migrationBuilder.Sql(@"DELETE FROM TMT_MAIL_TEMPLATES WHERE TMT_CODE ='ANNUL_COM';");

        migrationBuilder.Sql(@"INSERT INTO TMT_MAIL_TEMPLATES (TMT_CODE,TMT_CODEN2,TMT_LIB,TRST_STATUTID,TRLG_LNGID,TMT_OBJET,TMT_CORPS,TMT_PIED,TMT_COMMENTAIRE,TMT_IMPORTANCE,TMT_DATE_CREATION,TMT_MAILING_LIST,	TMT_MAILING_LIST_CC) VALUES (
'ANNUL_COM',NULL,
'Annulation d''une commande',
'A','fr-FR',
'Annulation de la commande n&deg; [TCMD_COMMANDEID]',
'<p>Bonjour,<br /><br />La commande n&deg;[TCMD_COMMANDEID] qui vous est assign&eacute;e, a &eacute;t&eacute; annul&eacute;e le [TMCD_SP_DATE_MODIF] par [TRU_AUTEUR_MODIFID].<br /><br />Raison : [TMCD_RP_LIB_FR].<br /><br />Commentaire :  [TMCD_SP_COMMENTAIRE].<br /><br />La commande a &eacute;t&eacute; pass&eacute;e automatiquement au statut &quot;Annul&eacute;e&quot;.</p>',
'<p>Ce message est envoy&eacute; par un traitement automatique, merci de ne pas r&eacute;pondre &agrave; ce mail.</p><br />[LOGO_ORKESTRA]',
'Mail pour notifier l''exploitant que la commanditaire a annulé la commande','H','2023-02-13 09:30:00', null, null);");

        migrationBuilder.Sql(@"INSERT INTO TMT_MAIL_TEMPLATES (TMT_CODE,TMT_CODEN2,TMT_LIB,TRST_STATUTID,TRLG_LNGID,TMT_OBJET,TMT_CORPS,TMT_PIED,TMT_COMMENTAIRE,TMT_IMPORTANCE,TMT_DATE_CREATION,TMT_MAILING_LIST,	TMT_MAILING_LIST_CC) VALUES (
'ANNUL_COM',NULL,
'Cancellation of an order',
'A','en-US',
'Cancellation of the order n&deg; [TCMD_COMMANDEID]',
'<p>Hello,<br />The order n&deg;[TCMD_COMMANDEID] which had been assigned to you, has been cancelled on [TMCD_SP_DATE_MODIF] by [TRU_AUTEUR_MODIFID].<br />Reason: [TMCD_RP_LIB_EN].<br />Comment: [TMCD_SP_COMMENTAIRE].<br /><br />The order has been automatically qualified as &quot;Cancelled&quot;.</p>',
'<p>This message is sent by an automatic processing, please do not reply to this email.</p><br />[LOGO_ORKESTRA]',
'Email to notify the operator the applicant has canceled the order','H','2023-02-13 09:30:00', null, null);");


    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TMT_MAILING_LIST_CC",
            table: "TMT_MAIL_TEMPLATES");
    }
}
