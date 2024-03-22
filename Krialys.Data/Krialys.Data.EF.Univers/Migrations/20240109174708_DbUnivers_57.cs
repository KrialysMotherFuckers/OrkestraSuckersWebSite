using Krialys.Data.EF.Univers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Data;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_57 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var ctx = new KrialysDbContext())
            {
                //var connStr = ctx.Database.GetConnectionString();
                using (var conn = new SqliteConnection($"DataSource=App_Data/Database/db-Univers.db3"))
                    try
                    {
                        conn.Open();
                        using (var cmd = new SqliteCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = @"

                                PRAGMA foreign_keys = 0;

                                DROP TABLE TR_WST_WebSite_Settings;

                                CREATE TABLE TR_WST_WebSite_Settings (
                                    wst_id          INTEGER PRIMARY KEY ASC AUTOINCREMENT,
                                    wst_code        TEXT    NOT NULL
                                                            UNIQUE,
                                    wst_label       TEXT,
                                    wst_value       TEXT    NOT NULL,
                                    wst_description TEXT
                                );

                                INSERT INTO TR_WST_WebSite_Settings (
                                                                        wst_code,
                                                                        wst_value,
                                                                        wst_description
                                                                    )
                                                                    SELECT TPM_PARAMID,
                                                                           TPM_VALEUR,
                                                                           TPM_INFO
                                                                      FROM TPM_PARAMS;

                                INSERT INTO TR_WST_WebSite_Settings (
                                                                        wst_description,
                                                                        wst_value,
                                                                        wst_label,
                                                                        wst_code
                                                                    )
                                                                    VALUES (
                                                                        'Defines the menu structure of the website',
                                                                        'Json',
                                                                        NULL,
                                                                        'WebSiteMenu'
                                                                    );

                                DROP TABLE TPM_PARAMS;

                                CREATE TABLE TR_MEL_EMail_Templates (
                                    mel_id                      INTEGER NOT NULL
                                                                        CONSTRAINT PK_TM_MEL_EMail_Templates PRIMARY KEY AUTOINCREMENT,
                                    sta_code                    TEXT,
                                    lng_code                    TEXT,
                                    mel_code                    TEXT    NOT NULL,
                                    mel_additional_code         TEXT,
                                    mel_description             TEXT,
                                    mel_email_subject           TEXT    NOT NULL,
                                    mel_email_body              TEXT    NOT NULL,
                                    mel_email_footer            TEXT    NOT NULL,
                                    mel_email_importance        TEXT,
                                    mel_email_recipients        TEXT,
                                    mel_email_recipients_in_cc  TEXT,
                                    mel_email_recipients_in_bcc TEXT,
                                    mel_comments                TEXT,
                                    mel_creation_date           TEXT NOT NULL DEFAULT(datetime('now', 'utc')) ,
                                    mel_created_by              TEXT,
                                    mel_update_date             TEXT,
                                    mel_update_by               TEXT,
                                    CONSTRAINT FK_TM_MEL_EMail_Templates_TRLG_LNGS_TRLG_LNGID FOREIGN KEY (
                                        lng_code
                                    )
                                    REFERENCES TRLG_LNGS (TRLG_LNGID),
                                    CONSTRAINT FK_TM_MEL_EMail_Templates_TRST_STATUTS_TRST_STATUTID FOREIGN KEY (
                                        sta_code
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) 
                                )
                                STRICT;

                                INSERT INTO TR_MEL_EMail_Templates (
                                                                       sta_code,
                                                                       lng_code,
                                                                       mel_code,
                                                                       mel_additional_code,
                                                                       mel_description,
                                                                       mel_email_subject,
                                                                       mel_email_body,
                                                                       mel_email_footer,
                                                                       mel_email_importance,
                                                                       mel_email_recipients,
                                                                       mel_email_recipients_in_cc,
                                                                       mel_email_recipients_in_bcc,
                                                                       mel_comments
                                                                   )
                                                                   SELECT 
                                                                          sta_code,
                                                                          lng_code,
                                                                          mel_code,
                                                                          mel_additional_code,
                                                                          mel_description,
                                                                          mel_email_subject,
                                                                          mel_email_body,
                                                                          mel_email_footer,
                                                                          mel_email_importance,
                                                                          mel_email_recipients,
                                                                          mel_email_recipients_in_cc,
                                                                          mel_email_recipients_in_bcc,
                                                                          mel_comments
                                                                     FROM TM_MEL_EMail_Templates;

                                CREATE INDEX IX_TR_MEL_EMail_Templates_TRLG_LNGID ON TR_MEL_EMail_Templates (
                                    lng_code
                                );

                                CREATE INDEX IX_TR_MEL_EMail_Templates_TRST_STATUTID ON TR_MEL_EMail_Templates (
                                    sta_code
                                );

                                DROP INDEX IX_TM_MEL_EMail_Templates_TRLG_LNGID;
                                DROP INDEX IX_TM_MEL_EMail_Templates_TRST_STATUTID;
                                DROP TABLE TM_MEL_EMail_Templates;

                                PRAGMA foreign_keys = 1;
                            ";

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
