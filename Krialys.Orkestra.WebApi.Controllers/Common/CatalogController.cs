using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Entities.COMMON;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.WebApi.Services.Auth;
using Krialys.Orkestra.WebApi.Services.Authentification;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.Data;
using Krialys.Orkestra.WebApi.Services.FileStorage;
using Krialys.Orkestra.WebApi.Services.RefManager;
using Krialys.Orkestra.WebApi.Services.System;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using static Krialys.Orkestra.Common.Shared.ETQ;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Controllers.Common;

/// <summary>
/// GLOBAL SERVICE FOR API CATALOG CORE FUNCTIONS
/// </summary>
[Area(Litterals.CatalogRootPath)]
[Route("[Area]")]
public class CatalogController : ODataController
{
    private readonly IConfiguration _configuration;
    private readonly ICommonServices _commonService;
    private readonly IHubContext<SPUHub> _hubContext;
    private readonly Serilog.ILogger _logger;
    private readonly IAuthentificationServices _credentialServices;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;
    private readonly ICpuConnectionManager _cpuManager;
    private readonly IWebHostEnvironment _hostingEnv;
    private readonly IOAuthServices _authServices;
    private readonly ICpuServices _cpuService;
    private readonly IRefManagerServices _refManagerServices;
    private readonly IEtiquettesServices _etiquettesServices;

    private readonly KrialysDbContext _dbContextUnivers;
    private readonly Krialys.Data.EF.Mso.KrialysDbContext _dbContextMso;
    private readonly Krialys.Data.EF.Etq.KrialysDbContext _dbContextEtq;
    private readonly Krialys.Data.EF.Logs.KrialysDbContext _dbContextLog;
    private readonly IZipManagerServices _zipManagerServices;

    public CatalogController(ICommonServices commonService, IConfiguration configuration,
        IHubContext<SPUHub> hubContext, Serilog.ILogger logger,
        IAuthentificationServices credentialServices, IHttpContextAccessor httpContextAccessor, IOAuthServices authServices,
        ICpuConnectionManager cpuManager, IWebHostEnvironment hostingEnv,
        ITrackedEntitiesServices trackedEntitiesServices,
        KrialysDbContext dbContextUnivers,
        Krialys.Data.EF.Mso.KrialysDbContext dbContextMso,
        Krialys.Data.EF.Etq.KrialysDbContext dbContextEtq,
        Krialys.Data.EF.Logs.KrialysDbContext dbContextLog,
        IRefManagerServices refManagerServices,
        ICpuServices cpuService,
        IEtiquettesServices etiquettesServices,
        IZipManagerServices zipManagerServices)
    {
        _dbContextUnivers =
            dbContextUnivers ?? throw new ArgumentNullException(nameof(dbContextUnivers)); // accès complet à DBUnivers
        _dbContextMso = dbContextMso ?? throw new ArgumentNullException(nameof(dbContextMso)); // accès complet à MSO
        _dbContextEtq = dbContextEtq ?? throw new ArgumentNullException(nameof(dbContextEtq)); // accès complet à ETQ
        _dbContextLog = dbContextLog ?? throw new ArgumentNullException(nameof(dbContextLog)); // accès complet à LOGS

        _refManagerServices = refManagerServices;
        _cpuService = cpuService;
        _commonService = commonService;

        _configuration = configuration;
        _hubContext = hubContext;
        _logger = logger;
        _credentialServices = credentialServices;
        _httpContextAccessor = httpContextAccessor;
        _authServices = authServices;
        _cpuManager = cpuManager;
        _hostingEnv = hostingEnv;
        _trackedEntitiesServices = trackedEntitiesServices;
        _etiquettesServices = etiquettesServices;
        _zipManagerServices = zipManagerServices;
    }

    /// <summary>
    /// Generic Catalog API (main entry point)<br />
    /// [!] Errors are logged to logUnivers.
    /// </summary>
    /// <param name="payload">JSON payload (key/values)</param>
    /// <param name="functionId">Function name to invoke</param>
    /// <param name="apiVersion">Api version relative to the invoked function</param>
    /// <param name="metaData">Metadata as contract: <br/>0 means no meta, but expected output for the caller <br/>1 means metadata used as template for the caller <br/>2 means metadata helper serving as input for posting datas</param>
    /// <returns>Json flow corresponding to the expected API mapping described in Confluence</returns>
#if RELEASE
    [Authorize]
#endif
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CatalogApi))]
    [HttpPost("InvokeApiCatalog")]
    public async Task<IActionResult> InvokeApiCatalog(
        [FromBody] IDictionary<string, object> payload,
        [FromQuery(Name = "functionId")] string functionId,
        [FromQuery(Name = "apiVersion")] string apiVersion = "v1",
        [FromQuery(Name = "metaData")] int metaData = 0)
    {
        string errorMessage = null;
        CatalogApi apiCatalog = null;

        // Identified userId and userName
        var (userId, userName) = _commonService.GetUserIdAndName();

        // Which function
        var _functionId = functionId;

        // Check payload (with payload when invoking API)
        if (metaData == 0 && payload is null)
        {
            //** Avoid calling any function (no parameters passed to the function due to malformed/wrong typed Json)
            return CatalogError("-error-", apiVersion, errorMessage);
        }

        switch (_functionId)
        {
            //** HelloWorld function API call
            case nameof(_commonService.HelloWorld):
                try
                {
                    if (metaData == 0)
                        apiCatalog = _commonService.HelloWorld(apiVersion, payload);
                    else
                        return Ok(new { HelloWorld = payload });
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }
                break;

            #region ETLLOGEXCEPTION
            //** EtlLogException function API call
            case nameof(EtlLogException):
                try
                {
                    // Check all supported versions here
                    if (!apiVersion.Equals("v1"))
                        throw new ArgumentException($"Invalid parameter: {nameof(apiVersion)}={apiVersion}");

                    if (metaData == 0)
                    {
                        var output = new List<EtlLogException>();

                        // Decode "Value" from "payload"
                        if (payload != null)
                        {
                            for (int i = 0; i < payload.Count; i++)
                            {
                                var input = DictionaryExtensions.ConvertFrom<EtlLogException>(payload, i)
                                            ?? throw new InvalidDataException($"Bad JSON, expected type: {nameof(EtlLogException)}");

                                output.AddRange(input);
                            }
                        }

                        // Write each content to Log table
                        _commonService.EtlLogException(output);

                        // Return computed values
                        apiCatalog = new CatalogApi
                        {
                            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                            Function = nameof(EtlLogException),
                            Version = apiVersion,
                            Success = true,
                            StatusCode = StatusCodes.Status200OK,
                            // Specific to function
                            ErrorMessage = null,
                            Value = new
                            {
                                Result = new object[] { null }, // To be aligned with other API
                                output.Count
                            }
                        };
                    }
                    else
                    {
                        var json = JsonSerializer.SerializeToUtf8Bytes(new EtlLogException("", "", "", 0, "", "", "", "", "",
                            new DetailedProduction[] { new DetailedProduction("", "", 0, 0, 0, TimeSpan.FromSeconds(0)) },
                            new DetailedError[] { new DetailedError("", 0, "", "", "") }));

                        return Ok(new { EtlLogException = JsonSerializer.Deserialize<IDictionary<string, object>>(json) });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }

                break;
            #endregion

            #region ETIQUETTES API
            //  Calcul de 1 à N étiquette
            //  INPUT  : IEnumerable<CalculateEtqInput>
            //  OUTPUT : IEnumerable<EtqOutput>
            case nameof(_cpuService.EtqCalculate):
                try
                {
                    // Check all supported versions here
                    if (!apiVersion.Equals("v1"))
                        throw new ArgumentException($"Invalid parameter: {nameof(apiVersion)}={apiVersion}");

                    if (metaData == 0)
                    {
                        var output = new List<EtqOutput>();

                        // Decode "Value" from "payload"
                        for (int i = 0; i < payload!.Count; i++)
                        {
                            var input = DictionaryExtensions.ConvertFrom<CalculateEtqInput>(payload, i)
                                        ?? throw new InvalidDataException($"Bad JSON, expected type: {nameof(CalculateEtqInput)}");

                            // Compute etiquettes
                            foreach (var e in input)
                            {
                                output.Add(
                                    await _cpuService.EtqCalculate(e.Guid, e.CodeObjEtq, e.Version,
                                        string.IsNullOrEmpty(e.CodePerimetre) ? null : e.CodePerimetre,
                                        string.IsNullOrEmpty(e.ValDynPerimetre) ? null : e.ValDynPerimetre, e.DemandeId,
                                        nameof(_cpuService.EtqCalculate), e.Simulation));
                            }
                        }

                        // Check output
                        bool success = output.All(e => e.Success);

                        // Return computed values
                        apiCatalog = new CatalogApi
                        {
                            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                            Function = nameof(_cpuService.EtqCalculate),
                            Version = apiVersion,
                            Success = success,
                            StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                            // Specific to function
                            ErrorMessage = success ? null : $"Error(s) calling {_functionId}. Please check parameters.",
                            Value = new
                            {
                                Result = output,
                                output.Count
                            }
                        };
                    }
                    else
                    {
                        var json = JsonSerializer.SerializeToUtf8Bytes(new CalculateEtqInput("", 0, "", "", 0, Litterals.FakeId, false));

                        return Ok(new { EtqCalculate = JsonSerializer.Deserialize<IDictionary<string, object>>(json) });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }

                break;

            //  Injection des données Suivi ressource d'une étiquette
            //  INPUT  : IEnumerable<EtqSuiviRessourceFileRaw>
            //  OUTPUT : IEnumerable<EtqOutput>
            case nameof(_cpuService.EtqSuiviRessource):
                try
                {
                    // Check all supported versions here
                    if (!apiVersion.Equals("v1"))
                        throw new ArgumentException($"Invalid parameter: {nameof(apiVersion)}={apiVersion}");

                    if (metaData == 0)
                    {
                        var output = new List<EtqOutput>();

                        // Decode "Value" from "parameters"
                        for (int i = 0; i < payload!.Count; i++)
                        {
                            var input = DictionaryExtensions.ConvertFrom<EtqSuiviRessourceFileRaw>(payload, i)
                                        ?? throw new InvalidDataException($"Bad JSON, expected type: {nameof(EtqSuiviRessourceFileRaw)}");

                            // Compute resources
                            output.AddRange(await _cpuService.EtqSuiviRessource(input));
                        }

                        // Check output
                        bool success = output.All(e => e.Success);

                        // Return computed values
                        apiCatalog = new CatalogApi
                        {
                            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                            Function = nameof(_cpuService.EtqSuiviRessource),
                            Version = apiVersion,
                            Success = success,
                            StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                            // Specific to function
                            ErrorMessage = success ? null : $"Error(s) calling {_functionId}.",
                            Value = new
                            {
                                Result = output,
                                output.Count
                            }
                        };
                    }
                    else
                    {
                        var json = JsonSerializer.SerializeToUtf8Bytes(new EtqSuiviRessourceFileRaw(Litterals.FakeId, "", "", "", ""));

                        return Ok(new { EtqSuiviRessource = JsonSerializer.Deserialize<IDictionary<string, object>>(json) });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }

                break;

            //  Injection des données de libellés d'une étiquette
            //  INPUT  : IEnumerable<EtqExtraInfoAddonFileRaw>
            //  OUTPUT : IEnumerable<EtqOutput>
            case nameof(_cpuService.EtqExtraInfoAddon):
                try
                {
                    // Check all supported versions here
                    if (!apiVersion.Equals("v1"))
                        throw new ArgumentException($"Invalid parameter: {nameof(apiVersion)}={apiVersion}");

                    if (metaData == 0)
                    {
                        var output = new List<EtqOutput>();

                        // Decode "Value" from "payload"
                        for (int i = 0; i < payload!.Count; i++)
                        {
                            var input = DictionaryExtensions.ConvertFrom<EtqExtraInfoAddonFileRaw>(payload, i)
                                        ?? throw new InvalidDataException($"Bad JSON, expected type: {nameof(EtqExtraInfoAddonFileRaw)}");

                            // Compute resources
                            output.AddRange(await _cpuService.EtqExtraInfoAddon(input));
                        }

                        // Check each output
                        bool success = output.All(e => e.Success);

                        // Return computed values
                        apiCatalog = new CatalogApi
                        {
                            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                            Function = nameof(_cpuService.EtqExtraInfoAddon),
                            Version = apiVersion,
                            Success = success,
                            StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                            // Specific to function
                            ErrorMessage = success ? null : $"Error(s) calling {_functionId}.",
                            Value = new
                            {
                                Result = output,
                                output.Count
                            }
                        };
                    }
                    else
                    {
                        var json = JsonSerializer.SerializeToUtf8Bytes(new EtqExtraInfoAddonFileRaw(Litterals.FakeId, "", "", ""));

                        return Ok(new { EtqExtraInfoAddon = JsonSerializer.Deserialize<IDictionary<string, object>>(json) });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message}{ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }

                break;

            //  Recherche d'étiquettes
            //  INPUT  : IEnumerable<EtqSearchInput>
            //  OUTPUT : IEnumerable<EtqSearchOutput>
            case nameof(_etiquettesServices.EtqSearch):
                try
                {
                    if (metaData == 0)
                    {
                        var labels = DictionaryExtensions.ConvertFrom<EtqSearchInput>(payload, 0);
                        var etqSearchOutput = _etiquettesServices.EtqSearch(labels)?.ToList()
                            ?? throw new InvalidDataException($"Bad JSON, expected type: {nameof(EtqSearchInput)}");

                        var success = etqSearchOutput.All(e => e.Success);
                        var partial = etqSearchOutput.Any(e => e.Success) ^ success;
                        var errCount = etqSearchOutput.Sum(e => e.ErrorCount);
                        var errorMessages = success ? null : partial ? "Only part of the request has been transmitted" : $"{errCount} {(errCount == 1 ? "error has" : "errors have")} been found";
                        var labelCount = etqSearchOutput.Sum(e => e.Success ? 1 : 0);

                        apiCatalog = new CatalogApi
                        {
                            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                            Function = nameof(_etiquettesServices.EtqSearch),
                            Version = apiVersion,
                            Success = success,
                            StatusCode = success ? StatusCodes.Status200OK : (partial ? StatusCodes.Status206PartialContent : StatusCodes.Status400BadRequest),
                            ErrorMessage = errorMessages,
                            Value = new
                            {
                                Result = etqSearchOutput,
                                Count = labelCount
                            }
                        };

                        var jsonEtqSearchOutput = JsonSerializer.Serialize(apiCatalog, DbFieldsExtensions.SerializerOptions());
                    }
                    else
                    {
                        // TODO: ask ETLists the expected DateTime format
                        var json = JsonSerializer.SerializeToUtf8Bytes(new EtqSearchOutput("", false, "", 0, new Label[] {
                                    new Label("", new Catalog(new Base(0, "", "", "", "", DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1))),
                                    new Rule[] { new Rule("", "") },
                                    new Ress[] { new Ress("", "", "", 0, "") })                )
                        }));

                        return Ok(new { EtqSearch = JsonSerializer.Deserialize<IDictionary<string, object>>(json) });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message}{ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }

                break;
            #endregion

            #region RefManagerServices
            //  Retourne les données de type GdbRequestToHandle
            //  INPUT  : No payload, datas are stored in database
            //  OUTPUT : IEnumerable<GdbRequestToHandle>
            case nameof(_refManagerServices.GetGdbRequestTohandle):
                try
                {
                    switch (metaData)
                    {
                        case 0: // POSTed datas (OUTPUT) to the caller
                            {
                                GdbRequestToHandle output = null;
                                var success = false;
                                var value = payload?.ToList()[0].Value;

                                if (value != null && Enum.TryParse<GdbRequestAction>(value.ToString(), out var requestAction))
                                {
                                    output = await _refManagerServices.GetGdbRequestTohandle(requestAction);
                                    success = output != null;
                                }

                                // Return computed values
                                apiCatalog = new CatalogApi
                                {
                                    TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                                    Function = nameof(_refManagerServices.GetGdbRequestTohandle),
                                    Version = apiVersion,
                                    Success = success,
                                    StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                                    // Specific to function
                                    ErrorMessage = success ? null : $"Error(s) calling {_functionId}.",
                                    Value = new
                                    {
                                        Result = new[] { output },
                                        Count = success ? output.ReferentielInfos.Count() : 0
                                    }
                                };
                                break;
                            }

                        case 1: // METADATA used (when any) as template for the caller
                            return Ok(new { GdbRequestToHandle = (await _refManagerServices.GetGdbRequestTohandle(GdbRequestAction.Read)) });

                        case 2: // METADATA used (when any) as INPUT for the caller
                            return Ok(new { });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message}{ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }
                break;

            //  Injection des données de type GdbRequestHandled
            //  INPUT  : GdbRequestHandled
            //  OUTPUT : true/false
            case nameof(_refManagerServices.GdbRequestHandled):
                try
                {
                    switch (metaData)
                    {
                        case 0: // POSTed datas (OUTPUT) to the caller
                            {
                                var success = await _refManagerServices.GdbRequestHandled(
                                   JsonSerializer.Deserialize<GdbRequestHandled>(payload?.ToList()[0].Value.ToString())
                               );

                                // Return computed values
                                apiCatalog = new CatalogApi
                                {
                                    TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                                    Function = nameof(_refManagerServices.GdbRequestHandled),
                                    Version = apiVersion,
                                    Success = success,
                                    StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                                    // Specific to function
                                    ErrorMessage = success ? null : $"Error(s) calling {_functionId}.",
                                    Value = new
                                    {
                                        Result = success,
                                        Count = success ? 1 : 0
                                    }
                                };
                                break;
                            }

                        case 1: // METADATA used (when any) as template for the caller
                            return Ok(new { GdbRequestHandled = true });

                        case 2: // METADATA used (when any) as INPUT for the caller
                            return Ok(new
                            {
                                GdbRequestHandled = JsonSerializer.Deserialize<IDictionary<string, object>>(JsonSerializer.SerializeToUtf8Bytes(
                                    new GdbRequestHandled()
                                    {
                                        RequestId = Guid.NewGuid().ToString("N"),
                                        GlobalResult = true,
                                        ReferentielInfos = new[]
                                        {
                                            new ReferentielInfoHandled()
                                            {
                                                TableName = "",
                                                TableData = "VGFibGVEYXRhIQ==",
                                                TableMeta = "VGFibGVEYXRhIQ==",
                                                ErrorMessage = ""
                                            }
                                        }
                                    }))
                            });
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"{ex.Message}{ex.InnerException?.Message}";

                    // Log error
                    _commonService.StdLogException(new LogException(GetType(), ex), userId, userName, payload);

                    return CatalogError(_functionId, apiVersion, errorMessage);
                }
                break;
                #endregion
        }

        return apiCatalog is not null
            ? Ok(apiCatalog)
            : CatalogError(_functionId, apiVersion, errorMessage);
    }

    /// <summary>
    /// Default and/or error cases
    /// OUTPUT : functional errors are passed to end user, exceptions are logged
    /// </summary>
    /// <param name="functionId"></param>
    /// <param name="apiVersion"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CatalogApi))]
    private IActionResult CatalogError(string functionId, string apiVersion, string errorMessage)
    {
        return Ok(new CatalogApi
        {
            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
            Function = functionId,
            Version = apiVersion,
            Success = false,
            StatusCode = StatusCodes.Status400BadRequest,
            // Generic to any function
            ErrorMessage = errorMessage ?? "Unknown function and/or bad parameters.",
            Value = new
            {
                Result = null as object,
                Count = 0
            }
        });
    }

    /// <summary>
    /// Get authentication Bearer Token based on application name and api version<br/>
    /// </summary>
    /// <param name="applicationId">Actually type 'Krialys.Etl'</param>
    /// <param name="apiVersion">Actually type 'v1'</param>
    /// <returns>Structure made of Token and Result</returns>
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CatalogApi))]
    [HttpGet("GetAuthenticationToken")]
    public IActionResult GetAuthenticationToken(
        [FromQuery(Name = "applicationId")] string applicationId,
        [FromQuery(Name = "apiVersion")] string apiVersion)
    {
        if (!string.IsNullOrEmpty(applicationId) && applicationId.Equals(CPULitterals.KrialysEtl))
        {
            if (!string.IsNullOrEmpty(apiVersion) && apiVersion.Equals(Litterals.Version1))
            {
                return Ok(new CatalogApi
                {
                    TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
                    Function = nameof(GetAuthenticationToken),
                    Version = apiVersion,
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    ErrorMessage = null,
                    Value = new
                    {
                        Result = new[] { _authServices.GetAppAccessToken(CPULitterals.KrialysEtl) },
                        Count = 1
                    }
                });
            }
        }

        return Ok(new CatalogApi
        {
            TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier,
            Function = nameof(GetAuthenticationToken),
            Version = apiVersion,
            Success = false,
            StatusCode = StatusCodes.Status400BadRequest,
            ErrorMessage = "Invalid parameter value.",
            Value = new
            {
                Result = new[] { null as object },
                Count = 0
            }
        });
    }
}