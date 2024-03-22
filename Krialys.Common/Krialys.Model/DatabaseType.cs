using System.Text.Json.Serialization;

namespace Krialys.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DatabaseType
{
    SqlServer = 0,
    Oracle,
    Sqlite,
    PostGreSql,
}
