using System.Data;
using System.Data.Common;
using System.Reflection;

// https://stackoverflow.com/a/48042166
namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// In both cases you can use something like this:
/// db.Query("Namespace.MyTable").Where(...) or db.Query(typeof(MyTable)).Where(...)
/// </summary>
public static class EntityFrameworkCoreExtensions
{
    public static IQueryable Query(this DbContext context, string entityName)
        => context.Query(context.Model.FindEntityType(entityName)?.ClrType);

    private static IQueryable Query(this DbContext context, Type entityType)
        => (IQueryable)SetMethod.MakeGenericMethod(entityType).Invoke(context, null);

    private static readonly MethodInfo SetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes);
}

public static class Helper
{
    /// <summary>
    /// SELECT name FROM sqlite_schema WHERE type IN('table','view') AND name NOT LIKE 'sqlite_%' ORDER BY 1;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="query"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static IEnumerable<T> RawSqlQuery<T>(DbContext context, string query, Func<DbDataReader, T> map)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;

        context.Database.OpenConnection();

        using var result = command.ExecuteReader();
        IList<T> entities = new List<T>();

        while (result.Read())
        {
            entities.Add(map(result));
        }

        return entities;
    }
}