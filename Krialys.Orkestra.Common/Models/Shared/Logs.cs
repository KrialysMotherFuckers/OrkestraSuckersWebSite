using Krialys.Orkestra.Common.Exceptions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Krialys.Orkestra.Common.Shared;

public static class Logs
{
    public class LogException
    {
        /// <summary>
        /// Version of the assembly.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// UTC DateTime format: yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string TimeUtc => DateTimeOffset.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// Exception message, usually corresponds to ex.Message
        /// </summary>
        public string Message { get; init; }

        /// <summary>
        /// Appliction name: MSO, DTF... or sub module name MSO/LOGS...
        /// </summary>
        public string Source { get; init; }

        /// <summary>
        /// Function name: method where exception occurred.
        /// </summary>
        public string Action { get; init; }

        /// <summary>
        /// Line number: line where the log is written.
        /// </summary>
        public int AtLine { get; init; }

        /// <summary>
        /// FileName: place where exception occurred.
        /// </summary>
        public string FileName { get; init; }

        /// <summary>
        /// Stack trace, usually corresponds to ex.StackTrace
        /// </summary>
        public string StackTrace { get; init; }

        /// <summary>
        /// Incrimined datas to be traced if necessary.
        /// </summary>
        public object Data { get; init; }

        /// <summary>
        /// Ctor (MUST BE PUBLIC to avoid error while serialization)
        /// </summary>
        public LogException() => Version = GetType().Assembly.GetName().Version?.ToString();

        /// <summary>
        /// Initialize LogException with the caller namespace + data + message, method and line number.
        /// </summary>
        /// <param name="caller">Type of the calling object.</param>
        /// <param name="data">Logged data.</param>
        /// <param name="message">Logged message.</param>
        /// <param name="memberName">Method name of the caller.</param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber">Line in the source code.</param>
        /// <remarks>memberName and lineNumber are completed by compiler. Don't call them on dependency injection.</remarks>
        public LogException(Type caller,
            object data = null,
            string message = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            StackTrace = null;
            Source = GetOnlyNameSpace(caller.FullName);
            Source = string.IsNullOrEmpty(Source) ? Source : Source.Replace('\\', '/');
            Data = data;
            Message = message;
            Message = string.IsNullOrEmpty(Message) ? Message : Message.Replace('\\', '/');
            Action = memberName;
            FileName = filePath?.Replace('\\', '/');
            AtLine = lineNumber;
            Version = GetType().Assembly.GetName().Version?.ToString();
        }

        /// <summary>
        /// Initialize LogException with the caller exception + data + message, method and line number.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="exception"></param>
        /// <param name="data"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
        public LogException(Type caller,
            Exception exception,
            object data = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            var ex = JsonSerializer.Deserialize<ExceptionExtensions.ExceptionBase>(exception.ToJson());

            StackTrace = ex.StackTrace ?? exception.StackTrace ?? exception.InnerException?.StackTrace;
            StackTrace = string.IsNullOrEmpty(StackTrace) ? StackTrace : StackTrace.Replace('\\', '/');
            Source = GetOnlyNameSpace(caller.FullName);
            Source = string.IsNullOrEmpty(Source) ? Source : Source.Replace('\\', '/');
            Data = data;
            Message = ex.Message ?? exception.Message;
            Message = string.IsNullOrEmpty(Message) ? Message : Message.Replace('\\', '/');
            Action = memberName;
            FileName = filePath?.Replace('\\', '/');

            if (StackTrace != null)
            {
                var pos = StackTrace.LastIndexOf(".cs:line ", StringComparison.Ordinal);
                if (pos > 0)
                    AtLine = int.TryParse(StackTrace[(pos + ".cs:line ".Length)..].Trim(), out var line) ? line : lineNumber;
            }

            Version = ex.Version;
        }

        private static string GetOnlyNameSpace(string fullName)
        {
            if (!string.IsNullOrEmpty(fullName) && !string.IsNullOrWhiteSpace(fullName))
            {
                var src = fullName.Split('[');

                if (src.Length > 1)
                    return src[2].Split(',')![0];
            }

            return fullName;
        }
    }
}
