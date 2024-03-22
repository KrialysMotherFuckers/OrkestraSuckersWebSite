using Syncfusion.Blazor.Grids;
using System.Collections.Concurrent;
using static Krialys.Orkestra.Web.Module.Common.DI.SfGridColumnParameterServices;

namespace Krialys.Orkestra.Web.Module.Common.DI;

public interface ISfGridColumnParameterServices
{
    IList<ForeignValue> ForeignValuesList { get; set; }
    ConcurrentDictionary<string, string> ForeignValuesDico { get; set; }
    bool Autofit { get; set; }
    ColumnType ColumnType { get; set; }
    //QueryBuilder.ColumnType ColumnQueryType { get; set; }
    string Field { get; set; }
    string ForeignKeyName { get; set; }
    Type ForeignKeyType { get; set; }
    string ForeignKeyValue { get; set; }
    string Format { get; set; }
    string HeaderText { get; set; }
    bool IsForeignKey { get; set; }
    bool IsIdentity { get; set; }
    bool IsInGrid { get; set; }
    bool IsPrimaryKey { get; set; }
    public Type Type { get; set; }
    bool Visible { get; set; }
    string Width { get; set; }
}

/// <summary>
/// Properties of a column, used by SfGrid components.
/// </summary>
public sealed class SfGridColumnParameterServices : ISfGridColumnParameterServices
{
    public Type Type { get; set; }

    public ColumnType ColumnType { get; set; }

    //public Syncfusion.Blazor.QueryBuilder.ColumnType ColumnQueryType { get; set; }

    public string Field { get; set; }

    public string Format { get; set; }

    public string HeaderText { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsIdentity { get; set; }

    public bool Visible { get; set; }

    public bool Autofit { get; set; }

    public string Width { get; set; }

    public bool IsInGrid { get; set; }

    public bool IsForeignKey { get; set; }

    public string ForeignKeyName { get; set; }

    public string ForeignKeyValue { get; set; }

    public Type ForeignKeyType { get; set; }

    public IList<ForeignValue> ForeignValuesList { get; set; }
    public ConcurrentDictionary<string, string> ForeignValuesDico { get; set; }

    public SfGridColumnParameterServices()
    {
        /* Default values. */
        Visible = true;
        Autofit = false;
        IsInGrid = true;
        ForeignValuesList = new List<ForeignValue>();
        ForeignValuesDico = new ConcurrentDictionary<string, string>();
    }

    /// <summary>
    /// Foreign value composed of a foreign key and the text displayed in place of it.
    /// </summary>
    public class ForeignValue
    {
        /// <summary>
        /// Foreign key (object)
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Text displayed in place of the foreign key.
        /// </summary>
        public string Label { get; set; }
    }
}