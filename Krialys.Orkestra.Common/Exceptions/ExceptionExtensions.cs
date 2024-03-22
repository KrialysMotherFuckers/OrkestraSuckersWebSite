using System.Text.Json;

namespace Krialys.Orkestra.Common.Exceptions;

public static class ExceptionExtensions
{
    public class ExceptionBase
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public ExceptionInfo InnerException { get; set; }
    }

    public class ExceptionInfo : ExceptionBase
    {
        public ExceptionInfo() { }

        internal ExceptionInfo(Exception exception, bool includeInnerException = true, bool includeStackTrace = false)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Type = exception.GetType().FullName;
            Version = GetType().Assembly.GetName().Version?.ToString();
            Message = exception.Message;
            Source = exception.Source;
            StackTrace = includeStackTrace ? exception.StackTrace : null;
            if (includeInnerException && exception.InnerException is not null)
            {
                InnerException = new ExceptionInfo(exception.InnerException, true, includeStackTrace);
            }
        }
    }

    /// <summary>
    /// Serialize the <see cref="Exception"/> to a JSON string.
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="includeInnerException">Control if to include inner exception</param>
    /// <param name="includeStackTrace">Control if to include stack trace</param>
    /// <returns></returns>
    public static string ToJson(
        this Exception ex,
        bool includeInnerException = true,
        bool includeStackTrace = false) //, JsonSerializerOptions options = null)
    {
        ArgumentNullException.ThrowIfNull(ex);
        var info = new ExceptionInfo(ex, includeInnerException, includeStackTrace);

        return JsonSerializer.Serialize(info);//, options ?? _defaultJsonSerializerOptions);
    }
}
