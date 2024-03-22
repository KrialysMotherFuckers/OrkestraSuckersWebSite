using Krialys.Common.LZString;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.System;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Controllers.Common;

/// <summary>
/// GLOBAL SERVICE FOR COMMON FUNCTIONS
/// </summary>

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Litterals.CommonRootPath)]
[Route("[Area]")]
public class CommonController : ODataController
{
    private readonly ICommonServices _service;
    private readonly IConfiguration _configuration;
    private readonly ICpuConnectionManager _iNodeServices;
    private readonly IWebHostEnvironment _hostingEnv;
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;

    public CommonController(ICommonServices globalService, IConfiguration configuration,
        ICpuConnectionManager iNodeServices, IWebHostEnvironment hostingEnv, ITrackedEntitiesServices trackedEntitiesServices)
    {
        _service = globalService;
        _configuration = configuration;
        _iNodeServices = iNodeServices;
        _hostingEnv = hostingEnv;
        _trackedEntitiesServices = trackedEntitiesServices;
    }

    [ApiExplorerSettings(IgnoreApi = false)]
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    [HttpPost("GetApiUniversAppSettings")]
    public IActionResult GetApiUniversAppSettings()
        => Ok(LZString.CompressToUint8Array(System.Text.Json.JsonSerializer.Serialize(_configuration.AsEnumerable().ToDictionary(x => x.Key, x => x.Value))));

    /// <summary>
    /// Get online CPU instances
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkerNode>))]
    [HttpPost("GetOnlineCPU")]
    public IActionResult GetOnlineCPU()
    {
        IList<WorkerNode> result = new List<WorkerNode>(capacity: 0);

        foreach (var cpuName in _iNodeServices.GetOnlineInstances)
            _iNodeServices
                .GetOnlineConnections(cpuName)
                .ToList()
                .ForEach(x => result.Add(x));

        return Ok(result);
    }

    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("SetLogException")]
    public IActionResult SetLogException([FromBody] IEnumerable<LogException> logExceptions)
    {
        var exceptions = logExceptions as LogException[] ?? logExceptions.ToArray();
        if (!exceptions.Any())
        {
            return Ok();
        }

        (string userId, string userName) = _service.GetUserIdAndName();

        foreach (var ex in exceptions)
        {
            _service.StdLogException(ex, userId, userName, ex.Data);
        }

        return Ok();
    }

    #region Upload / Remove files from server

    [HttpPost("[action]")]
    public void Save(IList<IFormFile> chunkFile, IList<IFormFile> UploadFiles, [FromQuery] int id = 0)
    {
        try
        {
            if (UploadFiles is not null)
            {
                foreach (var file in UploadFiles)
                {
                    var filename = ContentDispositionHeaderValue
                            .Parse(file.ContentDisposition)
                            .FileName?.Trim('"');

                    var folders = filename?.Split('/');
                    var uploaderFilePath = _hostingEnv.ContentRootPath + @"\App_Data";

                    // for Directory upload
                    if (folders!.Length > 1)
                    {
                        for (var i = 0; i < folders.Length - 1; i++)
                        {
                            var newFolder = uploaderFilePath + $@"\{folders[i]}";
                            Directory.CreateDirectory(newFolder);
                            uploaderFilePath = newFolder;
                            filename = folders[i + 1];
                        }
                    }

                    filename = uploaderFilePath + $@"\{filename}";

                    if (!System.IO.File.Exists(filename))
                    {
                        using var fs = System.IO.File.Create(filename);
                        file.CopyTo(fs);
                        fs.Flush();

                    }
                }
            }
        }
        catch (Exception e)
        {
            Response.Clear();
            Response.StatusCode = 204;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File failed to upload";
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
        }
    }

    [HttpPost("[action]")]
    public void Remove([FromForm] IList<IFormFile> UploadFiles, [FromQuery] int id = 0)
    {
        try
        {
            if (UploadFiles is not null)
            {
                var uploaderFilePath = _hostingEnv.ContentRootPath + @"\App_Data";
                UploadFiles[0].FileName.Split('/');

                var filename = uploaderFilePath + $@"\{UploadFiles[0].FileName}";
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }

                var dirPath = Path.GetDirectoryName(filename);
                if (Directory.GetFiles(dirPath!).Length == 0 && Directory.GetDirectories(dirPath).Length == 0)
                {
                    Directory.Delete(dirPath);
                }

                // Get root sub-folder aka 1st directory created
                var folder = filename.Replace(uploaderFilePath, "").Split('/')[0];
                if (folder.Length > 0)
                {
                    var newFolder = uploaderFilePath + folder;
                    if (Directory.GetFiles(newFolder).Length == 0 && Directory.GetDirectories(newFolder).Length == 0)
                    {
                        Directory.Delete(newFolder);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Response.Clear();
            Response.StatusCode = 200;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File removed successfully";
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
        }
    }

    #endregion

    #region TRACKEDENTITIES

    /// <summary>
    /// Get latest tracked entities
    /// </summary>
    /// <param name="entityTypes">Name of an entity, else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable TrackedEntities (ordered by date descending)</returns>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("GetTrackedEntities")]
    public IEnumerable<TrackedEntity> GetTrackedEntities(string[] entityTypes, DateTime lastChecked)
        => _trackedEntitiesServices.GetTrackedEntities(entityTypes, lastChecked);

    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("GetTrackedEntities")]
    public IEnumerable<TrackedEntity> GetTrackedEntities<TEntity>(DateTime lastChecked)
        => _trackedEntitiesServices.GetTrackedEntities(new[] { typeof(TEntity).FullName }, lastChecked);

    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("GetTrackedLogger")]
    public IEnumerable<WorkerNodeLog> GetTrackedLogger(string cpuId, DateTime lastChecked)
        => _trackedEntitiesServices.GetTrackedLogger(cpuId, lastChecked);

    [Authorize]
    [HttpPost("AddTrackedEntity")]
    public Task<bool> AddTrackedEntityAsync(string fullNames, string verb, string userId, string uuid)
        => _trackedEntitiesServices.AddTrackedEntitiesAsync(null, fullNames.Split(','), verb, userId, uuid);
    #endregion
}