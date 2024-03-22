using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.Common;

namespace Krialys.Orkestra.WebApi.Infrastructure.Common;

// Sample SQL Connection Health Check
// => https://dejanstojanovic.net/aspnet/2018/november/adding-healthchecks-just-got-a-lot-easier-in-aspnet-core-22/
public class SqlConnectionHealthCheck : IHealthCheck
{
    private const string DefaultTestQuery = "SELECT 1";
    private string ConnectionString { get; }
    private string TestQuery { get; }
    private string DbType { get; }

    public SqlConnectionHealthCheck(string connectionString, string dbType)
        : this(connectionString, DefaultTestQuery, dbType)
    {
    }

    private SqlConnectionHealthCheck(string connectionString, string testQuery, string dbType)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        TestQuery = testQuery;
        DbType = dbType;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (DbType)
            {
                case "SQLSERVER":
                    {
                        await using var connection = new SqlConnection(ConnectionString);
                        await connection.OpenAsync(cancellationToken);
                        await using var command = connection.CreateCommand();
                        command.CommandText = TestQuery;
                        await command.ExecuteNonQueryAsync(cancellationToken);
                        break;
                    }

                case "SQLITE":
                    {
                        await using var connection = new SqliteConnection(ConnectionString);
                        await connection.OpenAsync(cancellationToken);
                        await using var command = connection.CreateCommand();
                        command.CommandText = TestQuery;
                        await command.ExecuteNonQueryAsync(cancellationToken);
                        break;
                    }

                default:
                    throw new NotSupportedException($"Unsupported driver type: {DbType}");
            }
        }
        catch (DbException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

        return HealthCheckResult.Healthy();
    }
}
