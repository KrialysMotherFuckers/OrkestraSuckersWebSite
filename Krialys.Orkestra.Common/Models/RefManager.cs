using Krialys.Data.Model;
using System.Text.Json.Serialization;

namespace Krialys.Orkestra.Common.Models;

public class GdbRequest
{
    public string RequestId { get; set; }
}

public class GdbRequestToHandle : GdbRequest
{
    public IEnumerable<ReferentielInfo> ReferentielInfos { get; set; }
}

public class GdbRequestHandled : GdbRequest
{
    public GdbRequestAction RequestAction { get; set; } = GdbRequestAction.Read;
    public bool GlobalResult { get; set; }
    public IEnumerable<ReferentielInfoHandled> ReferentielInfos { get; set; }
}

public class ReferentielInfoHandled : ReferentielInfoBase
{
    public string ErrorMessage { get; set; } // Expected only for GdbRequestToHandle
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GdbRequestAction
{
    Read = 0,
    Write = 1
}

public class ReferentielInfoNotMapped
{
    [JsonIgnore] public int TableId { get; set; }
    [JsonIgnore] public string TableFunctionalName { get; set; }
    [JsonIgnore] public int? ScenarioId { get; set; }
    [JsonIgnore] public string LabelDataClonedJsonList { get; set; }
    [JsonIgnore] public string ParamLabelObjectCode { get; set; }
    [JsonIgnore] public string LabelCodeFieldName { get; set; }
}

public class ReferentielInfoBase : ReferentielInfoNotMapped
{
    public string TableName { get; set; }
    public string TableData { get; set; }
    public string TableMeta { get; set; }
}

public class ReferentielInfo : ReferentielInfoBase
{
    public DatabaseSettings DatabaseSettings { get; set; }
    public RefManagerTypologyType Typology { get; set; }
    public string Schema { get; set; }
    public string SelectQuery { get; set; }
    public string SqlCriteria { get; set; }
    public UpdateInfo Update { get; set; }
    public int MaxRowsToRead { get; set; }
}

public class DatabaseSettings
{
    public DatabaseType DatabaseType { get; set; }
    public string ServerName { get; set; }
    public int Port { get; set; }
    public string DatabaseName { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
}

public class UpdateInfo
{
    public string[] Columns { get; set; }     // expected array in Talend Job, not just a plain string
    public string[] PrimaryKeys { get; set; } // expected array in Talend Job, not just a plain string
}