using Krialys.Data.EF.Univers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_56 : Migration
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

                                CREATE TABLE TM_MEL_EMail_Templates (
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
                                    mel_creation_date           TEXT    NOT NULL,
                                    mel_comments                TEXT,
                                    CONSTRAINT FK_TM_MEL_EMail_Templates_TRLG_LNGS_TRLG_LNGID FOREIGN KEY (
                                        lng_code
                                    )
                                    REFERENCES TRLG_LNGS (TRLG_LNGID),
                                    CONSTRAINT FK_TM_MEL_EMail_Templates_TRST_STATUTS_TRST_STATUTID FOREIGN KEY (
                                        sta_code
                                    )
                                    REFERENCES TRST_STATUTS (TRST_STATUTID) 
                                ) STRICT;

                                INSERT INTO TM_MEL_EMail_Templates (
                                                                        mel_id,
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
                                                                        mel_creation_date,
                                                                        mel_comments
                                                                    )
                                                                    SELECT TMT_MAIL_TEMPLATEID,
                                                                            TRST_STATUTID,
                                                                            TRLG_LNGID,
                                                                            TMT_CODE,
                                                                            TMT_CODEN2,
                                                                            TMT_LIB,
                                                                            TMT_OBJET,
                                                                            TMT_CORPS,
                                                                            TMT_PIED,
                                                                            TMT_IMPORTANCE,
                                                                            TMT_MAILING_LIST,
                                                                            TMT_MAILING_LIST_CC,
                                                                            TMT_DATE_CREATION,
                                                                            TMT_COMMENTAIRE
                                                                        FROM TMT_MAIL_TEMPLATES;

                                DROP TABLE TMT_MAIL_TEMPLATES;

                                CREATE INDEX [IX_TMT_CODE$TM_MEL_EMail_Templates] ON TM_MEL_EMail_Templates (
                                    mel_code
                                );

                                CREATE INDEX IX_TM_MEL_EMail_Templates_TRLG_LNGID ON TM_MEL_EMail_Templates (
                                    lng_code
                                );

                                CREATE INDEX IX_TM_MEL_EMail_Templates_TRST_STATUTID ON TM_MEL_EMail_Templates (
                                    sta_code
                                );

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
