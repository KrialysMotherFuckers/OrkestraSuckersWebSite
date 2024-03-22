using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbLogs_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Kind virtual column, categorizing exceptions
            migrationBuilder.Sql("""
            ALTER TABLE LogUnivers ADD COLUMN Kind TEXT GENERATED ALWAYS AS (
                CASE
                    WHEN RenderedMessage LIKE '{@EtlLogException}%' THEN 'ETL'
                    WHEN RenderedMessage LIKE '{@CpuLogException}%' THEN 'CPU'
                    WHEN RenderedMessage LIKE '{@StdLogException}%' THEN 'STD'
                    WHEN RenderedMessage LIKE '{@LogExceptionEx}%' THEN 'STD'
                    ELSE NULL
                END
            ) VIRTUAL;
            """);

            // Add DemandeId virtual column, extracting DemandeId when possible
            migrationBuilder.Sql("""
            ALTER TABLE LogUnivers ADD COLUMN DemandeId INTEGER GENERATED ALWAYS AS (
                CASE
                    WHEN RenderedMessage LIKE '{@EtlLogException}%' THEN json_extract(Properties, '$.EtlLogException[0].DemandeId')
                    WHEN RenderedMessage LIKE '{@CpuLogException}%' THEN json_extract(Properties, '$.CpuLogException.DemandeId')
                    ELSE NULL
                END
            ) VIRTUAL;
            """);

            // Add indexes for Kind virtual column (no need to drop this index if table for the Down part of the migration)
            migrationBuilder.Sql("CREATE INDEX IDX_VT_Kind ON LogUnivers(Kind);");
            migrationBuilder.Sql("CREATE INDEX IDX_VT_DemandeId ON LogUnivers(DemandeId);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Virtual column
            migrationBuilder.Sql("""
                PRAGMA foreign_keys = 0;

                CREATE TABLE logUnivers_temp_table AS SELECT * FROM LogUnivers;

                DROP TABLE LogUnivers;

                CREATE TABLE LogUnivers (
                    id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp       TEXT,
                    Level           VARCHAR (16),
                    Exception       TEXT,
                    RenderedMessage TEXT,
                    Properties      TEXT
                );

                INSERT INTO LogUnivers (      id,
                                              Timestamp,
                                              Level,
                                              Exception,
                                              RenderedMessage,
                                              Properties
                                       )
                                       SELECT id,
                                              Timestamp,
                                              Level,
                                              Exception,
                                              RenderedMessage,
                                              Properties
                                         FROM logUnivers_temp_table;

                DROP TABLE logUnivers_temp_table;

                PRAGMA foreign_keys = 1;
                """);
        }
    }
}
