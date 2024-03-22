using Krialys.Orkestra.Common.Constants;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Krialys.Orkestra.WebApi.Services.Common;

public interface ISqlRaw : ITransientService
{
    /// <summary>
    /// READ using raw SQL, then reconstruct a new IDictionary
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    IList<IDictionary<string, object>> GetAllSqlRaw(DbContext dbContext, string sql, ref int count);

    /// <summary>
    /// READ using raw SQL
    /// </summary>
    /// <param name="dbContext">Dbcontext</param>
    /// <param name="sql">Sql code</param>
    /// <returns></returns>
    (IEnumerable<TClass> Data, int Count) GetAllSqlRaw<TClass>(DbContext dbContext, string sql);
}

public class SqlRaw : ISqlRaw
{
    /// <summary>
    /// READ using raw SQL, then reconstruct a new IDictionary
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="sql"></param>
    /// <returns></returns>
    public (IEnumerable<TClass> Data, int Count) GetAllSqlRaw<TClass>(DbContext dbContext, string sql)
    {
        int count = 0;
        var value = GetAllSqlRaw(dbContext, sql, ref count);

        return (value != null && count != -1
                ? JsonSerializer.Deserialize<IEnumerable<TClass>>(JsonSerializer.SerializeToUtf8Bytes(value))
                : Enumerable.Empty<TClass>(),
            count);
    }

    /// <summary>
    /// READ using raw SQL, then reconstruct a new IDictionary
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="sql"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public IList<IDictionary<string, object>> GetAllSqlRaw(DbContext dbContext, string sql, ref int count)
    {
        IList<IDictionary<string, object>> listDico = new List<IDictionary<string, object>>();

        try
        {
            // Extensions: ora.dll, uuid.dll, dbdump.dll and exec.dll
            //sql = "select crc32('Hello') AS CRC_32, uuid() AS STR_UUID, dbdump() as DUMP, exec('powershell -nologo \"Get-Content D:/Zig/README.md\"') as README;";

            using var connection = dbContext.Database.GetDbConnection();

            // Manage SQLITE extensions, see ref. https://github.com/little-brother/sqlite-gui/wiki#extensions
            if (dbContext.Database.IsSqlite())
            {
                var platform = string.Empty;
                var pattern = string.Empty;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    platform = nameof(OSPlatform.Linux);
                    pattern = "*.so";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    platform = nameof(OSPlatform.Windows);
                    pattern = "*.dll";
                }

                var extensionPath = Path.Combine(Globals.AssemblyDirectory, "App_Data/Extensions", platform).Replace('\\', '/');
                if (Directory.Exists(extensionPath))
                {
                    foreach (var library in Directory.GetFiles(extensionPath, pattern))
                    {
                        var extension = Path.Combine(extensionPath, library).Replace('\\', '/');
                        if (File.Exists(extension))
                        {
                            ((SqliteConnection)connection).EnableExtensions(true);
                            ((SqliteConnection)connection).LoadExtension(extension);
                        }
                    }
                }
            }

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();

            IList<string> columns = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToList();

            while (reader.Read())
            {
                IDictionary<string, object> dico = new Dictionary<string, object>();

                foreach (var col in columns)
                {
                    int colIndex = reader.GetOrdinal(col);

                    // Get column type
                    var colType = reader.GetFieldType(colIndex);

                    // Get column value
                    var value = reader.IsDBNull(colIndex)
                        ? DBNull.Value
                        : reader.GetValue(colIndex);

                    // Do conversion when necessary
                    if (value != DBNull.Value)
                    {
                        if (colType == typeof(string))
                        {
                            if (DateTime.TryParse(value.ToString(), out var dateTime))
                                value = dateTime;
                            else if (DateTimeOffset.TryParse(value.ToString(), out var dateTimeOffset))
                                value = dateTimeOffset;
                        }
                    }
                    else
                        value = null;

                    dico.TryAdd(col, value);
                }

                listDico.Add(dico);
            }

            count = listDico.Count;
        }
        catch (Exception ex)
        {
            count = -1;
            listDico.Clear();
            listDico.Add(new Dictionary<string, object> {
                { "Exception", ex.Message },
                { "StackTrace", ex.StackTrace }
            });

        }

        return listDico;
    }
}