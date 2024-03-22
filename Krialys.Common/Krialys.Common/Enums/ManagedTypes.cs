using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Krialys.Common.Enums;

/// <summary>
/// Managed typed - mainly used by RefManager
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ManagedTypes
{
    [EnumMember(Value = "Null")]
    Null,

    [EnumMember(Value = "String")]
    String,

    [EnumMember(Value = "Boolean")]
    Boolean,

    [EnumMember(Value = "Int32")]
    Int32,

    [EnumMember(Value = "Int64")]
    Int64,

    [EnumMember(Value = "Decimal")]
    Decimal,

    [EnumMember(Value = "DateTime")]
    DateTime,

    [EnumMember(Value = "DateTimeOffset")]
    DateTimeOffset,
}

/// <summary>
/// Supported Database types - mainly used by RefManager
/// </summary>
public static class DbTypes
{
    // TEXT
    public const string Guid = "GUID";
    public const string VarChar2 = "VARCHAR2";
    public const string NVarChar2 = "NVARCHAR2";
    public const string Char = "CHAR";
    public const string Text = "TEXT";

    // NUMERIC
    public const string Float = "FLOAT";
    public const string Double = "DOUBLE";
    public const string BinaryFloat = "BINARY_FLOAT";
    public const string Number = "NUMBER";

    // WHOLE NUMBER
    public const string Long = "LONG";
    public const string BigInt = "BIGINT";
    public const string Int = "INT";
    public const string Integer = "INTEGER";

    // BOOLEAN
    public const string Bool = "BOOL";
    public const string Boolean = "BOOLEAN";

    // DATE
    public const string Date = "DATE";
    public const string DateTime = "DATETIME";
    public const string DateTime2 = "DATETIME2";
}