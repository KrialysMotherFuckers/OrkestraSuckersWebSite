using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbLogs_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var ctx = new Krialys.Data.EF.Logs.KrialysDbContext())
            using (var conn = new SqliteConnection($"DataSource=App_Data/Database/db-Logs.db3"))
                try
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"

                            PRAGMA foreign_keys = 0;

                            DROP TABLE IF EXISTS TM_LOG_Logs;

                            CREATE TABLE TM_LOG_Logs (
                                log_id              INTEGER       PRIMARY KEY AUTOINCREMENT,
                                log_creation_date   DATETIME,
                                log_type            VARCHAR,
                                log_exception       TEXT,
                                log_message         TEXT,
                                log_message_details TEXT,
                                log_origin          TEXT    GENERATED ALWAYS AS (CASE WHEN log_message LIKE '{@EtlLogException}%' THEN 'ETL' WHEN log_message LIKE '{@CpuLogException}%' THEN 'CPU' WHEN log_message LIKE '{@StdLogException}%' THEN 'STD' WHEN log_message LIKE '{@LogExceptionEx}%' THEN 'STD' ELSE NULL END) VIRTUAL,
                                req_id              INTEGER GENERATED ALWAYS AS (CASE WHEN log_message LIKE '{@EtlLogException}%' THEN json_extract(log_message_details, '$.EtlLogException[0].DemandeId') WHEN log_message LIKE '{@CpuLogException}%' THEN json_extract(log_message_details, '$.CpuLogException.DemandeId') ELSE NULL END) VIRTUAL
                            );

                            INSERT INTO TM_LOG_Logs (
                                                        log_id,
                                                        log_creation_date,
                                                        log_type,
                                                        log_exception,
                                                        log_message,
                                                        log_message_details
                                                    )
                                                    SELECT id,
                                                           Timestamp,
                                                           Level,
                                                           Exception,
                                                           RenderedMessage,
                                                           Properties
                                                      FROM LogUnivers;

                            DROP TABLE LogUnivers;

                            CREATE INDEX IDX_VT_DemandeId ON TM_LOG_Logs (
                                req_id
                            );

                            CREATE INDEX IDX_VT_Kind ON TM_LOG_Logs (
                                log_origin
                            );

                            PRAGMA foreign_keys = 1;

                            ";

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    conn.Close();
                }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
