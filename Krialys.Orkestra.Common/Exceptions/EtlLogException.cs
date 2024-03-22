using System.Text.Json;

namespace Krialys.Orkestra.Common.Exceptions;

#region ETL LOG EXCEPTIONS
/// <summary>
/// ETL log exception
/// Attention: each '\' must be replaced by '/'
/// Attention: TimeSpan can be considered as a string in ETL, ex. "10:05:03"
/// </summary>
public class EtlLogException
{
    public EtlLogException(string etlName, string version, string machineName, int demandeId, string utd, string module,
        string userName, string timeUtc, string globalResult, DetailedProduction[] detailedProduction, DetailedError[] detailedErrors)
    {
        EtlName = etlName;
        Version = version;
        MachineName = machineName;
        DemandeId = demandeId;
        Utd = utd;
        Module = module;
        UserName = userName;
        TimeUtc = timeUtc;
        GlobalResult = globalResult;
        DetailedProduction = detailedProduction;
        DetailedErrors = detailedErrors;
    }

    public string EtlName { get; set; }
    public string Version { get; set; }
    public string MachineName { get; set; }
    public int DemandeId { get; set; }
    public string Utd { get; set; } // ETL must concatenate 'nom_etat' + ['etat_version'] as of june 13th, 2023
    public string Module { get; set; } // New field representing 'NOM_SCENARIO' as of june 13th, 2023
    public string UserName { get; set; }
    public string TimeUtc { get; set; }
    public string GlobalResult { get; set; }
    public DetailedProduction[] DetailedProduction { get; set; }
    public DetailedError[] DetailedErrors { get; set; }
}

public class DetailedProduction
{
    public DetailedProduction(string workflow, string result, int errors, int fieldConversionErrors, int warnings, TimeSpan elapsedTime)
    {
        Workflow = workflow;
        Result = result;
        Errors = errors;
        FieldConversionErrors = fieldConversionErrors;
        Warnings = warnings;
        ElapsedTime = elapsedTime;
    }

    public string Workflow { get; set; } // Renamed from 'Module' to 'Workflow' elsewhere in the specs
    public string Result { get; set; }
    public int Errors { get; set; }
    public int FieldConversionErrors { get; set; }
    public int Warnings { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    //public string Directory { get; set; } // Obsolete as of june 15th, 2023
}

public class DetailedError
{
    public DetailedError(string workflow, int? toolId, string messageType, string message, string directory)
    {
        Workflow = workflow;
        ToolId = toolId;
        MessageType = messageType;
        Message = message;
        Directory = directory;
    }

    public string Workflow { get; set; } // Renamed from 'Module' to 'Workflow' elsewhere in the specs
    public int? ToolId { get; set; } // Talend does not have toolId, this field is nullable as of june 13th, 2023
    public string MessageType { get; set; }
    public string Message { get; set; }
    public string Directory { get; set; }
}
#endregion

#region DATAGRID UI
/// <summary>
/// Represents the Datagrid level 1
/// </summary>
public class EtlLogExceptionGridLevel1
{
    #region Properties
    public int DemandeId { get; init; }              // EtlLogException
    public string Utd { get; init; }                 // EtlLogException
    public string Module { get; init; }              // EtlLogException
    public string GlobalResult { get; set; }         // EtlLogException (OK, WA, KO)
    public string Workflow { get; init; }            // DetailedProduction
    public string Result { get; set; }               // DetailedProduction (OK, NA, KO)
    public int Errors { get; init; }                 // DetailedProduction
    public int FieldConversionErrors { get; init; }  // DetailedProduction (masked)
    public int Warnings { get; set; }                // DetailedProduction
    public string UserName { get; init; }            // EtlLogException
    public string TimeUtc { get; set; }              // EtlLogException
    public TimeSpan ElapsedTime { get; init; }       // DetailedProduction
    public string MachineName { get; init; }         // EtlLogException (masked)
    public string EtlName { get; init; }             // EtlLogException (masked)
    public string Version { get; init; }             // EtlLogException (masked)
    #endregion

    /// <summary>
    /// Convert a Json list of EtlLogException
    /// </summary>
    /// <param name="jsonDatas">String list of EtlLogException</param>
    /// <returns>List of EtlLogExceptionGridLevel1</returns>
    public IEnumerable<EtlLogExceptionGridLevel1> GetListEtlLogExceptionGridLevel1(IEnumerable<string> jsonDatas)
    {
        var listGrid = new List<EtlLogExceptionGridLevel1>(capacity: 0);

        foreach (var jsonData in jsonDatas)
        {
            var etlLogEx = JsonSerializer.Deserialize<EtlLogException>(jsonData);

            // Avoid crash when etlLogEx.DetailedProduction is null
            if (etlLogEx.DetailedProduction != null)
            {
                listGrid.AddRange(
                    from dp in etlLogEx.DetailedProduction
                    select new EtlLogExceptionGridLevel1
                    {
                        DemandeId = etlLogEx.DemandeId,
                        Utd = etlLogEx.Utd,
                        Module = etlLogEx.Module,
                        GlobalResult = etlLogEx.GlobalResult,
                        Workflow = dp.Workflow,
                        Result = dp.Result,
                        Errors = dp.Errors,
                        FieldConversionErrors = dp.FieldConversionErrors,
                        Warnings = dp.Warnings,
                        UserName = etlLogEx.UserName,
                        TimeUtc = etlLogEx.TimeUtc,
                        ElapsedTime = (dp.ElapsedTime.TotalMilliseconds / 1000) < 1
                            ? new TimeSpan(dp.ElapsedTime.Hours, dp.ElapsedTime.Minutes, 1)
                            : new TimeSpan(dp.ElapsedTime.Hours, dp.ElapsedTime.Minutes, dp.ElapsedTime.Seconds),
                        MachineName = etlLogEx.MachineName,
                        EtlName = etlLogEx.EtlName,
                        Version = etlLogEx.Version,
                    });
            }
        }

        return listGrid;
    }
}

/// <summary>
/// Represents the Datagrid level 2 (sub level)
/// </summary>
public class EtlLogExceptionGridLevel2
{
    #region Properties
    public string Workflow { get; init; }            // DetailedError (masked)
    public string MessageType { get; set; }          // DetailedError (KO, WA, FCE)
    public string Message { get; init; }             // DetailedError
    public int? ToolId { get; init; }                // DetailedError
    public string Directory { get; init; }           // DetailedError
    #endregion

    /// <summary>
    /// Convert a Json list of EtlLogException, then filters items that belongs to same Workflow that belongs to DetailedErrors 
    /// </summary>
    /// <param name="jsonDatas">List of EtlLogException</param>
    /// <param name="workFlow">Workflow</param>
    /// <returns>List of EtlLogExceptionGridLevel2</returns>
    public IEnumerable<EtlLogExceptionGridLevel2> GetListEtlLogExceptionGridLevel2(IEnumerable<string> jsonDatas, string workFlow)
    {
        var listGrid = new List<EtlLogExceptionGridLevel2>(capacity: 0);

        foreach (var jsonData in jsonDatas)
        {
            var etlLogEx = JsonSerializer.Deserialize<EtlLogException>(jsonData);

            // Avoid crash when etlLogEx.DetailedErrors is null
            if (etlLogEx.DetailedErrors != null)
            {
                listGrid.AddRange(
                    from dp in etlLogEx.DetailedErrors
                    where dp.Workflow == workFlow
                    select new EtlLogExceptionGridLevel2
                    {
                        Workflow = dp.Workflow,
                        MessageType = dp.MessageType,
                        Message = dp.Message,
                        ToolId = dp.ToolId,
                        Directory = dp.Directory,
                    });
            }
        }

        return listGrid;
    }
}
#endregion

#region SQLITE UTILITIES
/// <summary>
/// SQL code that retrieves all informations from an SQLite table
/// </summary>
public class SqlTableInfos
{
    public string Name { get; init; }
    public string ColumnName { get; init; }
    public string ColumnType { get; init; }
    public int Pk { get; init; }
}

///// <summary>
///// SQL code that retrieves all errors belonging to the EtlLogException category
///// </summary>
//public class SqlEtlLogExceptions
//{
//    public int LogId { get; init; }
//    public DateTime Timestamp { get; init; }
//    public string Level { get; init; }
//    public int DemandeId { get; set; }
//    public EtlLogException EtlLogException { get; init; }
//}
#endregion
