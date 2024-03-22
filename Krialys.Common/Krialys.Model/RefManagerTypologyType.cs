using System.Text.Json.Serialization;

namespace Krialys.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefManagerTypologyType
{
    /// <summary>
    /// None (disable datagrid buttons: Add, Update, Delete)
    /// </summary>
    None = 0,

    /// <summary>
    /// Insert (disable datagrid buttons: none)
    /// </summary>
    WithLabel = 1,

    /// <summary>
    /// Update (disable datagrid buttons: Add, Delete)
    /// </summary>
    WithoutLabelUpdate = 2,

    /// <summary>
    /// Add or Replace (disable datagrid buttons: Delete)
    /// </summary>
    WithoutLabelAddReplace = 3
}