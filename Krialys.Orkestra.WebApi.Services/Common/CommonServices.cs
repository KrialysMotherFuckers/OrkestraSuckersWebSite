using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Orkestra.Common.Exceptions;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog.Events;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Services.Common;

public interface ICommonServices : IScopedService
{
    CatalogApi HelloWorld(string apiVersion, IDictionary<string, object> parameters);

    /// <summary>
    /// Get userId and userName
    /// </summary>
    /// <returns>(string userId, string userName) tuple</returns>
    (string userId, string userName) GetUserIdAndName();

    /// <summary>
    /// Get CultureInfo + TimeZone
    /// </summary>
    /// <returns>(string cultureInfo, string timeZone) tuple</returns>
    (string cultureInfo, string timeZone) GetUserCultureInfoAndTimeZone();

    /// <summary>
    /// Trace STD exception into LogError
    /// </summary>
    /// <param name="logException"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    /// <param name="dataIn"></param>
    void StdLogException(LogException logException, string userId, string userName, object dataIn);

    /// <summary>
    /// Trace ETL exception into LogError
    /// </summary>
    /// <param name="etlLogException"></param>
    void EtlLogException(IEnumerable<EtlLogException> etlLogException);

    /// <summary>
    /// Trace CPU exception into LogError
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="demandeId"></param>
    /// <param name="errorMessage"></param>
    /// <param name="cpuLogProperty"></param>
    void CpuLogException(LogEventLevel logLevel, int? demandeId, string errorMessage, WorkerNodeLog cpuLogProperty);
}

/// <summary>
/// Extented log exception
/// </summary>
public class LogExceptionEx : LogException
{
    /// <summary>
    /// User Id
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// User Name
    /// </summary>
    public string UserName { get; set; }
}

public sealed class CommonServices : ICommonServices
{
    private readonly IConfiguration _configuration;
    private readonly Serilog.ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Krialys.Data.EF.Logs.KrialysDbContext _dbContextLog;
    private readonly Krialys.Data.EF.Univers.KrialysDbContext _dbContextUnivers;

    public CommonServices(Serilog.ILogger logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        Krialys.Data.EF.Logs.KrialysDbContext dbContextLog, Krialys.Data.EF.Univers.KrialysDbContext dbContextUnivers)
    {
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _dbContextLog = dbContextLog;
        _dbContextUnivers = dbContextUnivers;
    }

    private string GetClaimValue(string claimTypeName)
        => ((ClaimsIdentity)_httpContextAccessor.HttpContext?.User.Identity)?.Claims
        .FirstOrDefault(x => x.Type.Equals(claimTypeName, StringComparison.OrdinalIgnoreCase))?.Value;

    /// <summary>
    /// Model to be conform to ApiCatalog
    /// </summary>
    /// <param name="apiVersion">Api version to deal with</param>
    /// <param name="parameters">Dictionary</param>
    /// <returns></returns>
    public CatalogApi HelloWorld(string apiVersion, IDictionary<string, object> parameters)
    {
        switch (apiVersion)
        {
            // Only v1 of this function is available
            case "v1":
                {
                    var values = parameters.Aggregate(string.Empty, (current, e) => current + $"{e.Key}:{e.Value},");

                    return new CatalogApi
                    {
                        TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                        Function = nameof(HelloWorld),
                        Version = apiVersion,
                        Success = true,
                        StatusCode = StatusCodes.Status200OK,
                        ErrorMessage = null,
                        Value = new
                        {
                            Result = (object)new[] { values[..^1] },
                            Count = 1
                        }
                    };
                }

            default:
                return new CatalogApi
                {
                    TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                    Function = nameof(HelloWorld),
                    Version = apiVersion,
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = $"Invalid parameter: {nameof(apiVersion)}={apiVersion}",
                    Value = new
                    {
                        Result = null as object,
                        Count = 0
                    }
                };
        }
    }

    private static string _userKAdmin;

    /// <summary>
    /// Get userId and userName
    /// </summary>
    /// <returns>(string userId, string userName) tuple</returns>
    public (string userId, string userName) GetUserIdAndName()
    {
        //var cli = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;

        // Identified user
        var userId = GetClaimValue(ClaimTypes.NameIdentifier);
        var userName = GetClaimValue(ClaimTypes.Name);

        // Fallback when no user has been found, then capture the User-Agent content
        if (string.IsNullOrEmpty(userName))
        {
            StringValues values = new();
            if ((_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("User-Agent", out values)).HasValue)
            {
                if (string.IsNullOrEmpty(_userKAdmin))
                {
                    var user = _dbContextUnivers.TRU_USERS.FirstOrDefault(x => x.TRU_LOGIN.ToUpper() == "KADMIN");
                    _userKAdmin = user != null ? user.TRU_USERID : "User-Agent";
                }
                userId = _userKAdmin;
                userName = values.Any() ? values.First() : null;
            }
        }

        return (userId, userName);
    }

    /// <summary>
    /// Get CultureInfo + TimeZone
    /// </summary>
    /// <returns>(string cultureInfo, string timeZone) tuple</returns>
    public (string cultureInfo, string timeZone) GetUserCultureInfoAndTimeZone()
        => (GetClaimValue(ClaimsLiterals.ClaimTypes.CultureInfo) ?? _configuration[ClaimsLiterals.ClaimTypes.CultureInfo],
            GetClaimValue(ClaimsLiterals.ClaimTypes.TimeZone) ?? _configuration[ClaimsLiterals.ClaimTypes.TimeZone]);

    /// <summary>
    /// Trace STD exception into LogError 
    /// </summary>
    /// <param name="logException"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    /// <param name="dataIn"></param>
    public void StdLogException(LogException logException, string userId, string userName, object dataIn)
        => GenericLogException("@StdLogException", logException, userId, userName, dataIn);

    /// <summary>
    /// Trace ETL exception into LogError
    /// </summary>
    /// <param name="etlLogExceptions"></param>
    public void EtlLogException(IEnumerable<EtlLogException> etlLogException)
        => _logger.Error($"{{@EtlLogException}}, DemandeId: {etlLogException.FirstOrDefault()!.DemandeId}", etlLogException);

    /// <summary>
    /// Trace CPU exception into LogError
    /// </summary>
    /// <param name="demandeId"></param>
    /// <param name="errorMessage"></param>
    /// <param name="cpuLogProperty"></param>
    public void CpuLogException(LogEventLevel logLevel, int? demandeId, string errorMessage, WorkerNodeLog cpuLogProperty)
    {
        var message = $"{{@CpuLogException}}, DemandeId: {demandeId}, Error: {errorMessage}";

        switch (logLevel)
        {
            case LogEventLevel.Information:
                _logger.Information(message, cpuLogProperty);
                break;

            case LogEventLevel.Warning:
                _logger.Warning(message, cpuLogProperty);
                break;

            case LogEventLevel.Error:
                _logger.Error(message, cpuLogProperty);
                break;
        }
    }

    /// <summary>
    /// Generic log exception
    /// </summary>
    /// <param name="logTypeName"></param>
    /// <param name="logException"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    /// <param name="dataIn"></param>
    private void GenericLogException(string logTypeName, LogException logException, string userId, string userName, object dataIn)
    {
        var stackTrace = logException.StackTrace?.Replace("\r\n", "\t").Replace("   at ", "").Trim();
        var stackEntries = stackTrace?.Split("\t", StringSplitOptions.RemoveEmptyEntries);
        object data = null;

        // Take data as it was given, don't try to encode
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (dataIn is not null)
        {
            // Can we serialize _data?
            data = dataIn is not string
                ? JsonSerializer.Serialize(dataIn, options)
                : dataIn;
        }
        else
        {
            // Is there any other data to serialize?
            if (logException.Data is not null)
                data = JsonSerializer.Serialize(logException.Data, options);
        }

        var datas = data?.ToString().TrimSpacesWithinWords();
        if (!string.IsNullOrEmpty(datas))
        {
            datas = datas.Replace("\"", "'");

            if (datas.StartsWith("'") && datas.EndsWith("'"))
                datas = datas.Replace("'", "");
        }

        var message = logException.Message;
        if (!string.IsNullOrEmpty(message))
        {
            message = message.TrimSpacesWithinWords().Replace("\"", "'");
        }

        var logType = $"{{{logTypeName}}}";

        _logger.Error(new ApplicationException(logException.Version),
            $"{logType}{(string.IsNullOrEmpty(message) ? "" : $", Reason: {message}")}",
            new LogExceptionEx
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                StackTrace = stackEntries?.Last(),
                Source = logException.Source,
                FileName = logException.FileName,
                Action = logException.Action,
                AtLine = logException.AtLine,
                Data = datas,
                Version = logException.Version
            });
    }
}