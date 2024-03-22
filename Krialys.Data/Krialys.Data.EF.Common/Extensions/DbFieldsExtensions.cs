using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krialys.Entities.COMMON;

public static class DbFieldsExtensions
{
    // Take data as it was given, don't try to encode
    #region JsonSerializer
    public static JsonSerializerOptions SerializerOptions(bool ignoreCycles = false)
    {
        var options = new JsonSerializerOptions();
        {
            if (ignoreCycles)
                options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.Converters.Add(new JsonStringEnumConverter());
#if DEBUG
            options.WriteIndented = true;
#endif
        }

        return options;
    }
    #endregion

    /// <summary>Gets PK Name.</summary>
    public static string GetPkName(this PropertyInfo pi)
        => pi.Name;

    /// <summary>Gets a PK list.</summary>
    public static PropertyInfo GetPkList<TEntity>() where TEntity : class
        => typeof(TEntity).GetProperties().FirstOrDefault(pi => Attribute.GetCustomAttribute(pi, typeof(KeyAttribute)) is KeyAttribute);

    /// <summary>Gets PK Value.</summary>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<string> GetPkValue<TEntity>(PropertyInfo pi, TEntity value)
    {
        var pkValue = new List<TEntity> { value }
            .Select(item => typeof(TEntity)
                .GetProperty(pi?.Name ?? string.Empty)
                ?.GetValue(item ?? new object(), null)
                ?.ToString());

        return pkValue;
    }

    public static int GetLastRowId(DbContext context, string tableName, bool isSqlite)
    {
        var sqlConnection = context.Database.GetDbConnection();
        sqlConnection.Open();
        using var command = sqlConnection.CreateCommand();
        command.CommandText = isSqlite ? "SELECT last_insert_rowid()" : $"SELECT IDENT_CURRENT('{tableName}')";
        var lastRowIdScalar = Convert.ToInt32(command.ExecuteScalar());

        return lastRowIdScalar;
    }

    /// <summary>
    /// Get TEntity's foreign keys
    /// </summary>
    /// <typeparam name="TEntity">Entity name</typeparam>
    /// <returns>List of foreign keys or null if none</returns>
    public static IEnumerable<string> GetFkList<TEntity>() where TEntity : class
    {
        List<string> listFk = new();
        // Test #1: TPS_PREREQUIS_SCENARIOS -> (TEP_ETAT_PREREQUISID, TS_SCENARIOID)
        // Test #2: TRU_USERS -> (y'en a pas)
        var piList = typeof(TEntity).GetProperties().Where(pi =>
            Attribute.GetCustomAttribute(pi, typeof(ForeignKeyAttribute)) is ForeignKeyAttribute);

        foreach (var pi in piList)
        {
            foreach (var attr in pi.GetCustomAttributesData()
                .Where(name => name.AttributeType == typeof(ForeignKeyAttribute)))
            {
                switch (attr.ConstructorArguments.Count)
                {
                    case > 0:
                        {
                            var value = attr.ConstructorArguments[0].Value;
                            if (value != null)
                                listFk.Add(value.ToString());
                            break;
                        }
                }
            }
        }

        return listFk.Any() ? listFk : null;
    }

    /// <summary>
    /// Get Data annotation from a given T instance
    /// Example: InfosDemande.GetAttributeFrom DisplayAttribute ("CATEGORIE").Name;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static T GetAttributeFrom<T>(this object instance, string propertyName) where T : Attribute
        => (T)instance.GetType().GetProperty(propertyName)?.GetCustomAttributes(typeof(T), false).FirstOrDefault();

    /// <summary>
    /// Computes the default value from a given data type
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Defaulted value</returns>
    private static object GetDefaultValueType(this Type type)
    {
        switch (type)
        {
            case { } t when t == typeof(bool):
                return default(bool);                   // => false

            case { } t when t == typeof(string):
                return string.Empty;                    // => ""

            case { } t when t == typeof(byte):
            case { } u when u == typeof(short):
            case { } v when v == typeof(int):
            case { } w when w == typeof(long):
                return byte.MinValue;                   // => 0

            case { } t when t == typeof(float):
            case { } u when u == typeof(double):
            case { } v when v == typeof(decimal):
                return 0.001f;                          // => 0.001

            case { } t when t == typeof(DateTime):
            case { } u when u == typeof(DateTimeOffset):
                return DateTime.MinValue.ToString("g"); // => "01/01/0001 00:00"

            default:
                return null;                            // => null
        }
    }

    /// <summary>
    /// Get 'real' column name using reflection
    /// </summary>
    /// <param name="entity">Entity name</param>
    /// <param name="fieldName">Column name</param>
    /// <returns></returns>
    public static string GetColumnName(this Type entity, string fieldName)
    {
        var property = entity.GetProperty(fieldName);

        if (property == null)
            return fieldName;

        return (Attribute.GetCustomAttribute(property, typeof(ColumnAttribute)) as ColumnAttribute)?.Name
            ?? fieldName;
    }
}