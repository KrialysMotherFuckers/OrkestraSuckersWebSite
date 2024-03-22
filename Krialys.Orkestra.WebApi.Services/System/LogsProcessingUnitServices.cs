using EntityFrameworkCore.RawSQLExtensions.Extensions;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Exceptions;
using Krialys.Orkestra.WebApi.Services.Common;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Services.System;

public interface ILogsProcessingUnitServices : IScopedService
{
    ValueTask<IEnumerable<TClass>> GetEtlLogExceptionGridLevelAsync<TClass>(DateTime startDate, DateTime endDate, int? demandeId = null, string workFlow = "");
}

public class LogsProcessingUnitServices : ILogsProcessingUnitServices
{
    private readonly Krialys.Data.EF.Logs.KrialysDbContext _dbContext;
    private readonly ICommonServices _commonServices;

    public LogsProcessingUnitServices(Krialys.Data.EF.Logs.KrialysDbContext dbContext, ICommonServices commonServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
    }

    /// <summary>
    /// Get all EtlLogexceptionGridLevel1 from LogUnivers
    /// <br />Records have a limit: <b>Globals.MaxTop</b><br />
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="demandeId"></param>
    /// <param name="workFlow"></param>
    /// </summary>
    public async ValueTask<IEnumerable<TClass>> GetEtlLogExceptionGridLevelAsync<TClass>(DateTime startDate, DateTime endDate, int? demandeId = null, string workFlow = "")
    {
        var sqlEx = string.Empty;

        try
        {
            // Simplified & speed up data processing thanks to virtual fields (decoded from the Properties json field)
            sqlEx = $@"""
                    SELECT json_extract(log_message_details, '$.EtlLogException[0]') as EtlLogException
                    FROM TM_LOG_Logs L
                    WHERE
	                    L.log_origin = 'ETL'
	                    AND L.log_type = 'Error'
                        AND L.req_id {(demandeId.HasValue ? $"= {demandeId}" : "!= 0")}
                        AND L.log_creation_date >= '{startDate:yyyy-MM-dd 00:00:00}' AND L.log_creation_date <= '{endDate:yyyy-MM-dd 23:59:59}'
                    ORDER by L.log_id DESC
                    LIMIT {Globals.MaxTop};""".Replace("\"", "").Trim();

            // AND L.Timestamp >= strftime('%Y-%m-%d %H:%M:%S', date('now','-{fromDays} days'))

            switch (demandeId.HasValue)
            {
                case false:
                    {
                        return new EtlLogExceptionGridLevel1()
                            .GetListEtlLogExceptionGridLevel1(await _dbContext.Database.SqlQuery<string>(sqlEx).ToListAsync())
                            .OfType<TClass>();
                    }

                case true:
                    {
                        return new EtlLogExceptionGridLevel2()
                            .GetListEtlLogExceptionGridLevel2(await _dbContext.Database.SqlQuery<string>(sqlEx).ToListAsync(), workFlow)
                            .OfType<TClass>();
                    }
            }
        }
        catch (Exception ex)
        {
            var user = _commonServices.GetUserIdAndName();
            _commonServices.StdLogException(new LogException(GetType(), ex), user.userId, user.userName, sqlEx);
        }

        return Enumerable.Empty<TClass>();
    }

    /// <summary>
    /// Get SqlTableInfos that describes an Sqlite table
    /// </summary>
    /// <param name="tablename">Name of the table</param>
    /// <returns></returns>
    public SqlTableInfos GetSqlTableInfos(string tableName)
    {
        var sql = $"""
            WITH table_list AS (
                SELECT c.name as Name
                FROM sqlite_schema c
                WHERE c.type = 'table' AND c.name NOT IN ('__EFMigrationsHistory', 'sqlite_sequence', 'sqlean_define')
                ORDER BY c.name
            )
            SELECT a.Name, c.name AS ColumnName, c.type AS ColumnType, c.pk AS Pk
                FROM table_list a
                JOIN pragma_table_info(a.Name) c
                WHERE a.Name = '{tableName}'
            ORDER BY a.Name, c.cid;
            """;

        return default; // new SqlTableInfos("Name", "ColumnName", "ColumnType", 1);
    }
}