namespace Krialys.Orkestra.Common.Exceptions;

/// <summary>
/// Represent common LOG properties from ILogger
/// </summary>
public interface ICommonLogProperties
{
    public string SourceContext { get; init; }
    public string ActionId { get; init; }
    public string ActionName { get; init; }
    public string RequestId { get; init; }
    public string RequestPath { get; init; }
}

public static class LogExceptionFrom
{
    /// <summary>
    /// Used to get the content of the "EtlLogException" from LogUnivers table
    /// </summary>
    public record Etl : ICommonLogProperties
    {
        public EtlLogException[] EtlLogException { get; init; }
        public string SourceContext { get; init; }
        public string ActionId { get; init; }
        public string ActionName { get; init; }
        public string RequestId { get; init; }
        public string RequestPath { get; init; }
    }

}