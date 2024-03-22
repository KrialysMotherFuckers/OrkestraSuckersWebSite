using Microsoft.Data.Sqlite;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Krialys.Orkestra.WebApi.Services.Common.Factories;

/// <summary>
/// SqlKata Sqlite query factory to build dynamic SQL queries.
/// See: https://sqlkata.com/docs
/// </summary>
public interface ISqliteQueryFactory
{
    /// <summary>
    /// Create a new Sqlite query factory
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <returns></returns>
    QueryFactory Create(string connectionString);
}

public class SqliteQueryFactory : ISqliteQueryFactory
{
    private static SqliteCompiler _sqliteCompiler = new SqliteCompiler();

    public QueryFactory Create(string connectionString)
        => new QueryFactory(new SqliteConnection(connectionString), _sqliteCompiler);
}
