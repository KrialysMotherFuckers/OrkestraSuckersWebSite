namespace Krialys.Orkestra.Common.Models;

/// <summary>
/// API Catalog structure marked as partial because can be scalable
/// </summary>
public class CatalogApi
{
    /// <summary>
    /// Http context traceId
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// Function to invoke
    /// </summary>
    public string Function { get; set; }

    /// <summary>
    /// API version
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Http status code, 200 means OK
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Explicit error message if any
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Core values stored (can be any kind, any type, any serializable content)
    /// </summary>
    public object Value { get; set; }
}
