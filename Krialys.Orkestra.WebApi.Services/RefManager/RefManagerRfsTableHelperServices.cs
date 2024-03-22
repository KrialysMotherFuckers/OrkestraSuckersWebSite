using EntityFrameworkCore.RawSQLExtensions.Extensions;
using Krialys.Common.Extensions;
using Krialys.Data.EF.RefManager;
using Krialys.Entities.COMMON;
using Krialys.Orkestra.WebApi.Services.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Services.RefManager;

/// <summary>
/// Helper service to directly update TM_RFS_ReferentialSettings.RfsTable's JSON stored data.
/// <br/>Few milliseconds to achieve atomically query instead of replacing globally/risky in-memory dictionary.
/// </summary>
public interface IRefManagerRfsTableHelperServices : IScopedService
{
    /// <summary>
    /// Insert into TM_RFS_ReferentialSettings.RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="keyValue">GUID target value</param>
    /// <param name="jsonDataRow">Json payload</param>
    /// <param name="top">Insert new data on top of existing data?</param>
    /// <returns>True when data has been inserted and/or if data was already present, otherwise returns false.</returns>
    ValueTask<bool> InsertEntryAsync(int refId, object keyValue, string jsonDataRow, bool top = true);

    /// <summary>
    /// Update TM_RFS_ReferentialSettings.RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="keyValue">GUID target value</param>
    /// <param name="jsonDataRow">Json payload</param>
    /// <returns>True when data has been updated and/or if data was already present, otherwise returns false.</returns>
    ValueTask<bool> UpdateEntryAsync(int refId, object keyValue, string jsonDataRow);

    /// <summary>
    /// Delete TM_RFS_ReferentialSettings.RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="keyValue">GUID target value</param>
    /// <returns>True when data has been deleted and/or if data was already present, otherwise returns false.</returns>
    ValueTask<bool> DeleteEntryAsync(int refId, object keyValue);

    /// <summary>
    /// Get all keys from TM_RFS_ReferentialSettings RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <returns>All GUID entries</returns>
    ValueTask<IEnumerable<string>> GetAllKeysAsync(int refId);

    /// <summary>
    /// Clone label object code entries.<br/>
    /// Check if labelCode exists and if $labelCode does not exist
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="labelCode">Value of the property to look for</param>
    /// <returns>1 when entries have been succesfully cloned, 0 when the labelcode has already been cloned, else return -1 when Rfs_LabelCodeFieldName is null.</returns>
    ValueTask<int> CloneLabelObjectCodeEntriesAsync(int refId, string labelCode);

    /// <summary>
    /// Get a new json matrix from the searched label, to be send to GDB.
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="labelKey">Name of the label column</param>
    /// <param name="labelValue">Label value to look up</param>
    /// <param name="LabelNewValue">New label value that will replace labelValue</param>
    /// <returns>A json matrix.</returns>
    ValueTask<string> GetNewLabelMatrix(int refId, string labelKey, string labelValue, string labelNewValue);

    /// <summary>
    /// Update the json matrix from the searched label -> to be used once the GDB workflow has completed to avoid incorrect data.
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="labelKey">Name of the label column</param>
    /// <param name="labelValue">Label value to look up</param>
    /// <param name="labelNewValue">New label value that will replace labelValue</param>
    /// <returns>True when everything went well, otherwise returns false.</returns>
    ValueTask<bool> UpdateNewLabelMatrix(int refId, string labelKey, string labelValue, string labelNewValue);
}

public class RefManagerRfsTableHelperServices : IRefManagerRfsTableHelperServices
{
    #region Parameters

    private readonly IQueryable<TM_RFS_ReferentialSettings> _referentialSettings;
    private const string LabelObjectCodePrefix = "$";
    private readonly KrialysDbContext _dbContext;
    private readonly ICommonServices _commonService;
    private readonly ILogger _logger;
    private (string userId, string userName) _user;

    private static string _tableName;
    private static string _columnName;
    private static string _columnId;
    private static string _guid;

    #endregion

    public RefManagerRfsTableHelperServices(KrialysDbContext dbContext, ICommonServices commonService, Serilog.ILogger logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _commonService = commonService ?? throw new ArgumentNullException(nameof(commonService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _referentialSettings = _dbContext.Set<TM_RFS_ReferentialSettings>().AsNoTracking();
        _user = commonService.GetUserIdAndName();
        GetEntityInfos();
    }

    #region Private functions
    private static void GetEntityInfos()
    {
        if (string.IsNullOrEmpty(_tableName))
        {
            var entity = typeof(TX_RFX_ReferentialSettingsData);
            _tableName = entity.Name;
            _columnName = entity.GetColumnName(nameof(TX_RFX_ReferentialSettingsData.Rfx_TableData));
            _columnId = entity.GetColumnName(nameof(TX_RFX_ReferentialSettingsData.Rfs_id));
            _guid = JsonExtensions.Id;
        }
    }

    /// <summary>
    /// Check if a given fieldName and all its rows already have data.
    /// <br/>Supports any kind of data supported by Sqlite.
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="fieldName">Name of the field to look for</param>
    /// <param name="fieldValue">Value of the field to look for</param>
    /// <param name="equalsOperator">Contains or not the searched value</param>
    /// <param name="usePrefix">Use the declared constant prefix</param>
    /// <returns>true when there is data.<br/>false when there is no data.<br/>null when an exception has occurred.</returns>
    private long? ContainsWithCount(int refId, string fieldName, object fieldValue, bool equalsOperator, bool usePrefix)
    {
        if (fieldValue == null || fieldValue.ToString().Length == 0)
            return null;

        var sqlEx = "";

        try
        {
            var value = fieldValue.ToString();

            if (fieldValue is string)
                if (usePrefix) // specific case for LabelObjectCode, all other cases will be used as regular string value
                    value = value[..1] == LabelObjectCodePrefix ? $"'{value}'" : $"'{LabelObjectCodePrefix}{value}'";
                else
                    value = $"'{value}'";

            sqlEx = $"""
            "
            SELECT count(*) AS result
                FROM {_tableName}, json_each({_columnName}) AS json_data
                WHERE json_data.value->>'{fieldName}' {(equalsOperator ? "=" : "!=")} {value}
                AND {_columnId} = {refId};
            "
            """[1..^1].Trim();

            return _dbContext.Database.SqlQuery<long>(sqlEx).FirstOrDefault();
        }
        catch (Exception ex)
        {
            // Log error
            if (!string.IsNullOrEmpty(sqlEx))
                _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return null;
    }
    #endregion

    /// <summary>
    /// Clone label object code entries<br/>
    /// Check if labelCode exists and if $labelCode does not exist
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="labelCode">Value of the property to look for</param>
    /// <returns>1 when entries have been succesfully cloned, 0 when the labelcode has already been cloned, else return -1 when Rfs_LabelCodeFieldName is null.</returns>
    public async ValueTask<int> CloneLabelObjectCodeEntriesAsync(int refId, string labelCode)
    {
        var sqlEx = "";

        try
        {
            var labelCodeFieldName = _referentialSettings.FirstOrDefault(e => e.Rfs_id == refId
                && e.Rfs_TableTypology == Krialys.Data.Model.RefManagerTypologyType.WithLabel)
                ?.Rfs_LabelCodeFieldName;

            if (string.IsNullOrEmpty(labelCodeFieldName)) // => the table has not been correctly filled
                return -1;

            // Already cloned?
            switch (ContainsWithCount(refId, labelCodeFieldName, labelCode, equalsOperator: true, usePrefix: true))
            {
                case > 0:
                    return 0;

                case 0:
                    sqlEx = $"""
                    "
                    UPDATE {_tableName} SET {_columnName} = (
                        SELECT json_group_array(json(result)) FROM
                        (
                    	    SELECT json_replace(json_data.value, '$.{labelCodeFieldName}', '{LabelObjectCodePrefix}' || json_extract(json_data.value, '$.{labelCodeFieldName}')) as result
                    			FROM {_tableName}, json_each({_columnName}) AS json_data
                                WHERE json_data.value->>'{labelCodeFieldName}' = '{labelCode}'
                    			AND {_columnId} = {refId}
                    	    UNION ALL
                    	    SELECT json_data.value
                    			FROM {_tableName}, json_each({_columnName}) AS json_data
                    			WHERE json_data.value->>'{labelCodeFieldName}' != '{labelCode}'
                    			AND {_columnId} = {refId}
                        ))
                    WHERE {_columnId} = {refId};"
                    """[1..^1].Trim();

                    if (await _dbContext.Database.ExecuteSqlRawAsync(sqlEx) > 0)
                    {
                        var count = ContainsWithCount(refId, labelCodeFieldName, labelCode, equalsOperator: true, usePrefix: true);
                        if (count.HasValue && count.Value > 0)
                            return (int)count;
                    }
                    break;

                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log error
            if (!string.IsNullOrEmpty(sqlEx))
                _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return -2;
    }

    /// <summary>
    /// Get a new json matrix from the searched label, to be send to GDB.
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="labelKey">Name of the label column</param>
    /// <param name="labelValue">Label value to look up</param>
    /// <param name="LabelNewValue">New label value that will replace labelValue</param>
    /// <returns>A json matrix.</returns>
    public async ValueTask<string> GetNewLabelMatrix(int refId, string labelKey, string labelValue, string LabelNewValue)
    {
        var sqlEx = "";

        try
        {
            sqlEx = $"""
            "
            SELECT '[' || group_concat(json_replace(json_data.value, '$.{labelKey}', '{LabelNewValue}'), ',') || ']' AS result
            FROM {_tableName}
                JOIN json_each({_columnName}) AS json_data ON 1=1
                WHERE json_data.value->>'{labelKey}' = '{labelValue}'
            AND {_columnId} = {refId};"
            """[1..^1].Trim();

            return await _dbContext.Database.SqlQuery<string>(sqlEx).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            // Log error
            _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return string.Empty;
    }

    /// <summary>
    /// Update the json matrix from the searched label -> to be used once the workflow has completed to avoid incorrect data.
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="labelKey">Name of the label column</param>
    /// <param name="labelValue">Label value to look up</param>
    /// <param name="LabelNewValue">New label value that will replace labelValue</param>
    /// <returns>True when everything went well, otherwise returns false.</returns>
    public async ValueTask<bool> UpdateNewLabelMatrix(int refId, string labelKey, string labelValue, string labelNewValue)
    {
        var sqlEx = "";

        try
        {
            sqlEx = $"""
            "
            UPDATE {_tableName} SET {_columnName} = (
            	SELECT '[' || group_concat(result, ',') || ']' FROM ( 
            	SELECT json_replace(json_data.value, '$.{labelKey}', '{labelNewValue}') AS result
            			FROM {_tableName}
            					JOIN json_each({_columnName}) AS json_data ON 1=1
            					WHERE json_data.value->>'{labelKey}' = '{labelValue}'
            			AND {_columnId} = {refId}
            	UNION ALL
            	SELECT json_data.value AS result
            			FROM {_tableName}
            					JOIN json_each({_columnName}) AS json_data ON 1=1
            					WHERE json_data.value->>'{labelKey}' != '{labelValue}'
            			AND {_columnId} = {refId}
            	))
            WHERE {_columnId} = {refId};"
            """[1..^1].Trim();

            return await _dbContext.Database.ExecuteSqlRawAsync(sqlEx) > 0;
        }
        catch (Exception ex)
        {
            // Log error
            _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return false;
    }

    /// <summary>
    /// Insert into TM_RFS_ReferentialSettings.RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="keyValue">GUID target value</param>
    /// <param name="jsonDataRow">Json payload</param>
    /// <param name="top">Insert new data on top of existing data?</param>
    /// <returns>True when data has been inserted and/or if data was already present, otherwise returns false.</returns>
    public async ValueTask<bool> InsertEntryAsync(int refId, object keyValue, string jsonDataRow, bool top = true)
    {
        var sqlEx = "";

        try
        {
            var jsonRow = $"json('{{{jsonDataRow}}}')";

            sqlEx = $"""
            "
            UPDATE {_tableName}
            SET {_columnName} = CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM json_each({_columnName})
                    WHERE json_extract(value, '$.{_guid}') = '{keyValue}'
                    AND {_columnId} = {refId}
                )
                THEN {_columnName}"
            """.Replace("\"", "");

            if (top)
            {
                sqlEx += $"""
                " ELSE
                    '['
                    || {jsonRow}
                    || ','
                    || rtrim(substr({_columnName}, 2), ']')
                    || ']'
                END
                WHERE {_columnId} = {refId};"
                """[1..^1];
            }
            else
            {
                sqlEx += $"""
                " ELSE rtrim({_columnName}, ']')
                    || ','
                    || {jsonRow}
                    || ']'
                END
                WHERE {_columnId} = {refId};"
                """[1..^1];
            }

            return await _dbContext.Database.ExecuteSqlRawAsync(sqlEx) > 0;
        }
        catch (Exception ex)
        {
            // Log error
            _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return false;
    }

    /// <summary>
    /// Update TM_RFS_ReferentialSettings.RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="keyValue">GUID target value</param>
    /// <param name="jsonDataRow">Json payload</param>
    /// <returns>True when data has been updated and/or if data was already present, otherwise returns false.</returns>
    public async ValueTask<bool> UpdateEntryAsync(int refId, object keyValue, string jsonDataRow)
    {
        var sqlEx = "";

        try
        {
            var jsonRow = $"json('{{{jsonDataRow}}}')";

            sqlEx = $"""
            "
            UPDATE {_tableName}
            SET {_columnName} = (
                SELECT json_group_array(
                    CASE
                        WHEN json_extract(value, '$.{_guid}') = '{keyValue}'
                        THEN {jsonRow}
                        ELSE json(value)
                    END
                )
                FROM json_each({_columnName})
                WHERE {_columnId} = {refId}
            )
            WHERE {_columnId} = {refId};"
            """[1..^1].Trim();

            return await _dbContext.Database.ExecuteSqlRawAsync(sqlEx) > 0;
        }
        catch (Exception ex)
        {
            // Log error
            _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return false;
    }

    /// <summary>
    /// Delete TM_RFS_ReferentialSettings.RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <param name="keyValue">GUID target value</param>
    /// <returns>True when data has been deleted and/or if data was already present, otherwise returns false.</returns>
    public async ValueTask<bool> DeleteEntryAsync(int refId, object keyValue)
    {
        var sqlEx = "";

        try
        {
            sqlEx = $"""
            "
            UPDATE {_tableName}
            SET {_columnName} = (
                SELECT '[' || group_concat(value, ',') || ']'
                FROM (
                    SELECT value
                    FROM json_each({_columnName})
                    WHERE json_extract(value, '$.{_guid}') <> '{keyValue}'
                )
            )
            WHERE {_columnId} = {refId};"
            """[1..^1].Trim();

            return await _dbContext.Database.ExecuteSqlRawAsync(sqlEx) > 0;
        }
        catch (Exception ex)
        {
            // Log error
            _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return false;
    }

    /// <summary>
    /// Get all keys from TM_RFS_ReferentialSettings RfsTable
    /// </summary>
    /// <param name="refId">Table reference Id</param>
    /// <returns>All GUID entries.</returns>
    public async ValueTask<IEnumerable<string>> GetAllKeysAsync(int refId)
    {
        var sqlEx = "";

        try
        {
            sqlEx = $"""
            "
            SELECT json_extract(value, '$.{_guid}') AS {_guid}
                FROM {_tableName},
                json_each({_columnName})
                WHERE {_columnId} = {refId};"
            """[1..^1].Trim();

            return await _dbContext.Database.SqlQuery<string>(sqlEx).ToListAsync();
        }
        catch (Exception ex)
        {
            // Log error
            _commonService.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, sqlEx);
        }

        return Enumerable.Empty<string>();
    }
}