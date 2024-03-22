using Krialys.Common.Literals;
using Krialys.Orkestra.Common;
using Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;
using Krialys.Orkestra.WebApi.Services.EJ2FileManager.PhysicalFileProvider;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Krialys.Orkestra.WebApi.Controllers.Common;

/// <summary>
/// Implements dedicated controller for FILEMANAGER
/// </summary>
[ApiExplorerSettings]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly PhysicalFileProvider _operation = new();
    private readonly string _srcBasePath;
    private readonly IConfiguration _configuration;
    private string _rootFolder;
    private readonly string _envId;
    private readonly IFileServices _iFileServices;
    private readonly Encoding _encoding;

    public FileController(IConfiguration configuration, IHttpContextAccessor accessor, IFileServices iFileServices)
    {
        _configuration = configuration;
        _iFileServices = iFileServices;

        _srcBasePath = configuration["ParallelU:PathEnvVierge"]; //root;
        _envId = accessor.HttpContext!.Request.Headers["EnvId"];

        // How to manage accents issue within zip files: https://stackoverflow.com/questions/59734984/char-extraction-issue-with-zipfile-system-io-compression-c-sharp-wpf
        _encoding = Encoding.GetEncoding(850);

        if (!_srcBasePath!.EndsWith(@"/")) // potential bug when basePath does not ends with /, then add /
            _srcBasePath += @"/";

        _rootFolder = $"{_srcBasePath}{_envId}";
        if (!_rootFolder.EndsWith("/")) // potential bug when _rootFolder does not ends with /, then add /
            _rootFolder += "/";
        _operation.RootFolder(_rootFolder);
    }

    # region SfFileManager WebApi Methods

    // Processing the File Manager operations https://www.talkingdotnet.com/how-to-increase-file-upload-size-asp-net-core/
    [Route("FileOperations")]
    [HttpPost]
    [DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public string FileOperations([FromBody] FileManagerDirectoryContent args)
    {
        var values = args.Action switch
        {
            "read" => PhysicalFileProvider.ToCamelCase(_operation.GetFiles(args.Path, args.ShowHiddenItems)),// reads the file(s) or folder(s) from the given path.
            "delete" => PhysicalFileProvider.ToCamelCase(_operation.Delete(args.Path, args.Names)),// deletes the selected file(s) or folder(s) from the given path.
            "copy" => PhysicalFileProvider.ToCamelCase(_operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)),// copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
            "move" => PhysicalFileProvider.ToCamelCase(_operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)),// cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
            "details" => PhysicalFileProvider.ToCamelCase(_operation.Details(args.Path, args.Names, args.Data)),// gets the details of the selected file(s) or folder(s).
            "create" => PhysicalFileProvider.ToCamelCase(_operation.Create(args.Path, args.Name)),// creates a new folder in a given path.
            "search" => PhysicalFileProvider.ToCamelCase(_operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive)),// gets the list of file(s) or folder(s) from a given path based on the searched key string.
            "rename" => PhysicalFileProvider.ToCamelCase(_operation.Rename(args.Path, args.Name, args.NewName, replace: true)),// renames a file or folder.
            _ => string.Empty,
        };

        return values;
    }

    // Processing the GetImage operation
    [Route("GetImage")]
    [HttpGet]
    [Produces(Litterals.ApplicationJson)]
    public IActionResult GetImage([FromForm] FileManagerDirectoryContent args, [FromQuery(Name = "envId")] string envId)
    {
        _operation.RootFolder($"{_srcBasePath}{envId.Split('?')[0]}");

        args.Path ??= envId.Split('=')[1];

        var path = args.Path.Length == 0 ? "/" : args.Path;

        //Invoking GetImage operation with the required paramaters
        // path - Current path of the image file; Id - Image file id;
        return _operation.GetImage(path, args.Id, false, null, null);
    }

    // Processing the Upload operation https://www.talkingdotnet.com/how-to-increase-file-upload-size-asp-net-core/
    [Route("Upload")]
    [HttpPost]
    [DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    public IActionResult Upload([FromForm] string path, [FromForm] IList<IFormFile> uploadFiles, [FromForm] string action)
    {
        if (!string.IsNullOrEmpty(path))
        {
            //Invoking upload operation with the required paramaters
            // path - Current path where the file is to uploaded; uploadFiles - Files to be uploaded; verb - name of the operation(upload)
            var uploadResponse = _operation.Upload(path, uploadFiles, action, null);

            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }
        }

        return Content("");
    }

    // Processing the Download operation https://www.talkingdotnet.com/how-to-increase-file-upload-size-asp-net-core/
    [Route("Download")]
    [HttpPost]
    [DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    public IActionResult Download([FromForm] string downloadInput, [FromQuery(Name = "envId")] string envId)
    {
        //Invoking download operation with the required paramaters
        // path - Current path where the file is downloaded; Names - Files to be downloaded;
        //var args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput); // NewtonSoft
        var args = JsonSerializer.Deserialize<FileManagerDirectoryContent>(downloadInput,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _operation.RootFolder($"{_srcBasePath}{envId}");

        var path = args.Path.Length == 0 ? "/" : args.Path;
        var names = args.Path.Length == 0 ? new[] { "/" } : args.Names;

        //return operation.Download(path, names);
        return _operation.Download(path, names, args.Data);
    }

    #endregion

    // Download zip file from Server http://localhost:8000/api/univers/v1/FILE/DownloadEnvironmentZip?envId=E000005-Test
    [Route("DownloadEnvironmentZip")]
    [HttpGet]
    [DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    public IActionResult DownloadEnvironmentZip([FromQuery(Name = "envId")] string envId)
    {
        var fileName = $"{envId}.zip";
        var fullPathFile = $"{_srcBasePath}{fileName}";

        // Case file not found, return "NotFound.txt" file.
        if (!System.IO.File.Exists(fullPathFile))
        {
            // Define file name.
            fileName = "NotFound.txt";

            // Define file path.
            fullPathFile = $"{_srcBasePath}{fileName}";

            // Create directory if doesn't exist.
            if (!Directory.Exists(_srcBasePath))
                Directory.CreateDirectory(_srcBasePath);

            // Create file if doesn't exist.
            if (!System.IO.File.Exists(fullPathFile))
            {
                using var sw = System.IO.File.CreateText(fullPathFile);
            }
        }

        FileManagerDirectoryContent args = new()
        {
            Action = "download",
            Path = "/",
            Names = new[] { fileName },
            Data = new FileManagerDirectoryContent[] {
                    new() {
                        Name = fileName, Size = new FileInfo(fullPathFile).Length
                    }
                }
        };

        return _operation.Download(args.Path, args.Names, args.Data);
    }

    // Download any file from Server http://localhost:8000/api/univers/v1/FILE/DownloadFile?fromPath={ParallelU:PathDoc}subdir/&fileName=Test.docx
    // Use: NavigationManager.NavigateTo($"{_configuration[Litterals.ProxyUrl]}{Litterals.UniversRootPathV1}/FILE/DownloadFile?fromPath={{ParallelU:PathDoc}}subdir/&fileName=Test.docx");
    /// <summary>
    /// Download file.
    /// </summary>
    /// <param name="fromPath">Path on server, can have appsettings parameters between {}.</param>
    /// <param name="fileName">Name of the file on server.</param>
    /// <param name="downloadFileName">Name of the file when downloaded (optional)</param>
    /// <returns></returns>
    [Route("DownloadFile")]
    [HttpGet]
    [DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    public Task DownloadFileAsync([FromQuery(Name = "fromPath")] string fromPath,
                                  [FromQuery(Name = "fileName")] string fileName,
                                  [FromQuery(Name = "downloadFileName")] string downloadFileName)
    {
        // Look for {xxx:yyy} using ApiUnivers appsettings, when found then path is revealed
        var rx = new Regex(@"(?<=\{)[^]]+(?=\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var matches = rx.Matches(fromPath);
        if (matches.Count > 0)
        {
            var name = matches.FirstOrDefault()!.Groups[0].Value;
            var value = _configuration[matches.FirstOrDefault()!.Value];
            if (!string.IsNullOrEmpty(value))
                fromPath = fromPath.Replace("{" + name + "}", value);
        }

        // Reveal full path filename 
        var fullPathFile = $"{fromPath}{fileName}";

        // Name of the downloaded file.
        string outputFileName = string.IsNullOrEmpty(downloadFileName) ? fileName : downloadFileName;

        // Assign root folder to target path
        _operation.RootFolder(fromPath);

        // If file not found, download a dummy NotFound.txt file.
        if (!System.IO.File.Exists(fullPathFile))
        {
            // Define file name.
            outputFileName = "NotFound.txt";

            // Define file path.
            fullPathFile = $"{_srcBasePath}{outputFileName}";

            // Create directory if doesn't exist.
            if (!Directory.Exists(_srcBasePath))
                Directory.CreateDirectory(_srcBasePath);

            // Create file if doesn't exist.
            if (!System.IO.File.Exists(fullPathFile))
            {
                using var sw = System.IO.File.CreateText(fullPathFile);
            }
        }

        // Prepare response
        // Define content type based on file type.
        new FileExtensionContentTypeProvider().TryGetContentType(outputFileName, out var contentType);
        Response.ContentType = contentType!;

        // Specify that the response should treated as an attachment rather than content which results in the download dialog.
        Response.Headers.Add("Content-Disposition", $"attachment; filename={outputFileName}");

        // Send file.
        return Response.SendFileAsync(fullPathFile);
    }

    /// <summary>
    /// Implement rules for checking environment directory based on EnvId
    /// Implement insert/update TEB_ETAT_BATCHS
    /// Implement environment zip
    /// Implement update TE_ETATS
    /// </summary>
    /// <param name="envId"></param>
    /// <returns>Report status</returns>
    [Route("GetEnvironmentToc")] // http://localhost:8000/api/univers/v1/FILE/GetEnvironmentToc?envId=E000005-Test
    [HttpGet]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult))]
    public async Task<ApiResult> GetEnvironmentToc([FromQuery(Name = "envId")] string envId, [FromQuery(Name = "checkOnly")] bool checkOnly)
    {
        var report = string.Empty;
        var folderEnv = $"{_srcBasePath}{envId}/";
        var dir = new DirectoryInfo(folderEnv);

        var structEnv = _iFileServices.GetStructureUploadRules(envId);

        try
        {
            // Check if directories are present (D)
            var structDir = structEnv.Where(x => x.TLEM_FILE_TYPE == "D").ToList();
            var dirList = dir.GetDirectories();
            foreach (var d in structDir)
            {
                bool found = false;

                // Check if directory exists
                if (d.TLEM_ACTION.Equals("CHK", StringComparison.InvariantCultureIgnoreCase))
                {
                    found = dirList.Any(e => e.Name.Equals(d.TLEM_PATH_NAME, StringComparison.InvariantCultureIgnoreCase));
                }
                // Delete directory if exists
                else if (d.TLEM_ACTION.Equals("DEL", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (dirList.Any(e => e.Name.Equals(d.TLEM_PATH_NAME, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Directory.Delete($"{folderEnv}{d.TLEM_PATH_NAME}", recursive: true);
                    }
                    found = true;
                }

                // Report errors
                if (!found)
                    report += $"Directory missing: <b>{d.TLEM_PATH_NAME.ToUpper()}</b></br>";
            }
        }
        catch (Exception ex)
        {
            report += $"Directory error: <b>{(ex.InnerException?.Message is not null ? ex.InnerException.Message : ex.Message)}</b></br>";
        }

        // Check if files are present (F)
        var structFile = structEnv.Where(x => x.TLEM_FILE_TYPE == "F").ToList();
        foreach (var f in structFile)
        {
            bool found = false;

            // Check if file exists
            if (f.TLEM_ACTION.Equals("CHK", StringComparison.InvariantCultureIgnoreCase))
            {
                found = dir.GetFiles($"{(f.TLEM_PATH_NAME != "" ? f.TLEM_PATH_NAME + "/" : "")}{f.TLEM_FILENAME_PATTERN}").Any();
            }
            // Delete file if exists
            else if (f.TLEM_ACTION.Equals("DEL", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Directory.Exists($"{folderEnv}{f.TLEM_PATH_NAME}"))
                {
                    try
                    {
                        foreach (var file in dir.GetFiles($"{(f.TLEM_PATH_NAME != "" ? f.TLEM_PATH_NAME + "/" : "")}{f.TLEM_FILENAME_PATTERN}"))
                        {
                            if (System.IO.File.Exists(file.FullName))
                            {
                                System.IO.File.Delete(file.FullName);
                            }
                        }
                        found = true;
                    }
                    catch (Exception ex)
                    {
                        report += $"File error: <b>{(ex.InnerException?.Message is not null ? ex.InnerException.Message : ex.Message)}</b></br>";
                    }
                }
            }

            // Report errors
            if (!found)
                report += $"File missing: <b>{(f.TLEM_PATH_NAME != "" ? f.TLEM_PATH_NAME.ToUpper() + "/" : "")}{f.TLEM_FILENAME_PATTERN}</b></br>";
        }

        // Get batchs list from the environment
        var batchDir = $"{folderEnv}Batch/";
        if (Directory.Exists(batchDir) && !checkOnly) // Only when we are saving the environment, in this case checkOnly should be equal to false
        {
            var batches = new DirectoryInfo(batchDir).GetFiles("*.bat").OrderBy(f => f.Name).ToList();

            // Add or Update TEB_ETAT_BATCHS versus physically batchs that are present within /Batch folder
            var updated = await _iFileServices.AddOrUpdateTEB_ETAT_BATCHS(envId, batches);
            if (updated == -1)
            {
                await _iFileServices.UpdateTE_ETATS(envId, StatusLiteral.No, 0);

                // Error occurred
                report += "Error while updating <b>TEB_ETAT_BATCHS</b></br>";
            }
        }

        // Look for forbidden files like .EXE and .DLL
        IList<string> forbidden = new List<string>();
        SearchForbiddenFiles(folderEnv, envId, forbidden);
        if (forbidden.Count > 0)
            report += $"Banned files found:</br><ul><li>{(string.Join(string.Empty, forbidden))}</li></ul>";

        return string.IsNullOrEmpty(report)
            ? new ApiResult(HttpStatusCode.OK.ToString(), 1, $"<h6 style='color: darkblue;'><b>All operations succeeded</b> for {envId}</h6>")
            : new ApiResult(HttpStatusCode.NotFound.ToString(), 0, $"<h6 style='color: yellow;'>Errors found for {envId}</h6>{report}");
    }

    static void SearchForbiddenFiles(string directoryPath, string envId, IList<string> files)
    {
        try
        {
            // Get all files with .exe and .dll extensions in the current directory
            string[] exeFiles = Directory.GetFiles(directoryPath, "*.exe");
            string[] dllFiles = Directory.GetFiles(directoryPath, "*.dll");
            string file = string.Empty;

            // Display the found files (added a white list for SNCF specific tools)
            foreach (string exeFile in Globals.FilterAllowedExecutables(exeFiles))
            {
                file = exeFile[exeFile.IndexOf(envId)..].Replace('\\', '/');
                files.Add($"<li><strong>{file}</strong></li>");
            }

            // Display the found files (added a white list for SNCF specific tools)
            foreach (string dllFile in Globals.FilterAllowedExecutables(dllFiles))
            {
                file = dllFile[dllFile.IndexOf(envId)..].Replace('\\', '/');
                files.Add($"<li><strong>{file}</strong></li>");
            }

            // Recursively search subdirectories
            string[] subdirectories = Directory.GetDirectories(directoryPath);
            foreach (string subdirectory in subdirectories)
                SearchForbiddenFiles(subdirectory, envId, files);
        }
        catch // Ignore
        { }
    }

    /// <summary>
    /// OB-453 adding reason details when error occurred
    /// </summary>
    /// <param name="envId"></param>
    /// <returns></returns>
    private string ZipEnvironmentFile(string envId)
    {
        string startPath = $"{_srcBasePath}{envId}";
        string zipPath = $"{_srcBasePath}{envId}.zip";

        try
        {
            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, false, _encoding);

            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [Route("UploadNewEmptyEnvironmentZip")]
    [HttpPost]
    [DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    public void UploadNewEmptyEnvironmentZip(IList<IFormFile> chunkFile, IList<IFormFile> uploadFiles, [FromQuery(Name = "envId")] string envId)
        => _iFileServices.EmptyEnvironmentUpdload(chunkFile, uploadFiles, envId);

    [Route("UploadFileNew")]
    [HttpPost, DisableRequestSizeLimit]
    [Produces(Litterals.ApplicationJson)]
    public void UploadFileNew(IList<IFormFile> chunkFile, IList<IFormFile> uploadFiles,
        [FromQuery(Name = "fileNameId")] string fileNameId, [FromQuery(Name = "targetPath")] string targetPath, [FromQuery(Name = "etatId")] string etatId)
    {
        try
        {
            var file = uploadFiles.FirstOrDefault();
            var etatPath = Path.Combine(_configuration[targetPath] ?? targetPath, etatId); // either give symbolic variable, else give path
            var etatFilePath = fileNameId is null ? Path.Combine(etatPath, file?.FileName!) : Path.Combine(etatPath, fileNameId);

            // Step 1: create directory targetPath\etatId
            if (!Directory.Exists(etatPath))
                Directory.CreateDirectory(etatPath);

            // Step 2: save file to the target directory using fileNameId
            using var fs = System.IO.File.Create(etatFilePath);
            file?.CopyTo(fs);
            fs.Flush();
        }
        catch (Exception ex)
        {
            Response.Clear();
            Response.StatusCode = 204;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File failed to upload";
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = ex.Message;
        }
    }

    /// <summary>
    /// Remove files.
    /// This endpoint is used by SfUploader component.
    /// </summary>
    /// <param name="uploadFiles">List of files to delete.
    ///     ("UploadFiles" must be the ID of SfUploader).</param>
    /// <param name="basePath">Prefix to file path, should be a field from appsettings.json.</param>
    /// <param name="filePath">Suffix to file path.</param>
    [HttpPost("[action]")]
    [Produces(Litterals.ApplicationJson)]
    public void RemoveFiles(IList<IFormFile> uploadFiles,
        [FromQuery(Name = "basePath")] string basePath,
        [FromQuery(Name = "filePath")] string filePath)
    {
        // Test request parameters.
        if (uploadFiles is null || !uploadFiles.Any())
        {
            // Incorrect parameters.
            // Http response: Bad request.
            Response.Clear();
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Bad parameter: UploadFiles is null";
            return;
        }

        // Are any files to delete missing on server?
        bool areFilesMissing = false;

        foreach (var file in uploadFiles)
        {
            int statusCode = DeleteFile(file.FileName, basePath, filePath);

            // Case where an error happen.
            if (statusCode.Equals((int)HttpStatusCode.BadRequest))
                return;

            // Case where the file was not found on server.
            if (statusCode.Equals((int)HttpStatusCode.Accepted))
            {
                areFilesMissing = true;
            }
        }

        if (areFilesMissing)
        {
            // File not found on server.
            // Http response: Accepted => Request processed, but no guarantee of result.
            Response.Clear();
            Response.StatusCode = (int)HttpStatusCode.Accepted;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File not found";
        }
    }

    /// <summary>
    /// Remove file based on file name.
    /// This endpoint is used by SfUploader component when postRawFile parameter equals false.
    /// </summary>
    /// <param name="uploadFiles">Deleted file name.
    ///     ("UploadFiles" must be the ID of SfUploader).</param>
    /// <param name="basePath">Prefix to file path, should be a field from appsettings.json.</param>
    /// <param name="filePath">Suffix to file path.</param>
    [HttpPost("[action]")]
    [Produces(Litterals.ApplicationJson)]
    public void RemoveRawFile([FromForm] string uploadFiles,
        [FromQuery(Name = "basePath")] string basePath,
        [FromQuery(Name = "filePath")] string filePath)
    {
        // Test request parameters.
        if (uploadFiles is null)
        {
            // Incorrect parameters.
            // Http response: Bad request.
            Response.Clear();
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Bad parameter: UploadFiles is null";
            return;
        }

        DeleteFile(uploadFiles, basePath, filePath);
    }

    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="fileName">Name of the file to delete.</param>
    /// <param name="basePath">File path prefix, can be a field from appsettings.json.</param>
    /// <param name="filePath">File path suffix.</param>
    /// <returns>Http status code.</returns>
    private int DeleteFile(string fileName, string basePath, string filePath)
    {
        try
        {
            var fullFilename = Path.Combine(_configuration[basePath] ?? basePath, filePath, fileName);

            if (System.IO.File.Exists(fullFilename))
            {
                System.IO.File.Delete(fullFilename);
            }
            else
            {
                // File not found on server.
                // Http response: Accepted => Request processed, but no guarantee of result.
                Response.Clear();
                Response.StatusCode = (int)HttpStatusCode.Accepted;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File not found";
                return Response.StatusCode;
            }
        }
        catch (Exception e)
        {
            // Http response: Internal server error.
            Response.Clear();
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            return Response.StatusCode;
        }

        return (int)HttpStatusCode.OK;
    }

    [Authorize]
    [Route("DeleteDirectoryFiles")]
    [HttpPost, DisableRequestSizeLimit] // [Query] string folderName, [Query] string fileName, [Query] bool deleteAll
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(KeyValuePair<string, string>))]
    public KeyValuePair<string, string> DeleteDirectoryFiles([FromQuery(Name = "folderName")] string folderName, [FromQuery(Name = "fileNames")] string fileNames, [FromQuery(Name = "deleteFolder")] bool deleteFolder)
    {
        // Look for {xxx:yyy} using ApiUnivers appsettings, when found then path is revealed
        var rx = new Regex(@"(?<=\{)[^]]+(?=\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var matches = rx.Matches(folderName);
        if (matches.Count > 0)
        {
            var name = matches.FirstOrDefault()!.Groups[0].Value;
            var value = _configuration[matches.FirstOrDefault()?.Value!];
            if (!string.IsNullOrEmpty(value))
                folderName = folderName.Replace("{" + name + "}", value);
        }

        try
        {
            // Delete directory content
            if (deleteFolder.Equals(true))
            {
                if (Directory.Exists(folderName))
                {
                    Directory.Delete(folderName, true);
                    Directory.CreateDirectory(folderName);

                    return new KeyValuePair<string, string>("OK", $"Folder content deleted: {folderName}");
                }

                return new KeyValuePair<string, string>("KO", $"Folder not found: {folderName}");
            }
            // Delete file/s

            List<string> filesNotFound = new();
            List<string> filesLookup = new();
            filesLookup.AddRange(fileNames.Split(','));

            foreach (string file in filesLookup)
            {
                string filePath = Path.Combine(folderName, file);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                else
                {
                    filesNotFound.Add(file);
                }
            }

            return filesNotFound.Any()
                ? new KeyValuePair<string, string>("KO", $"Files not found: {folderName} => {string.Join(",", filesNotFound)}")
                : new KeyValuePair<string, string>("OK", $"All files deleted: {folderName} => {fileNames}");
        }
        catch (Exception ex)
        {
            return new KeyValuePair<string, string>("KO", $"Error: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("RemoveEmptyEnvironmentZip")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public ValueTask<bool> RemoveEmptyEnvironmentZip([FromQuery] string envId) => _iFileServices.RemoveEmptyEnvironmentZip(envId);

    [Authorize]
    [HttpPost("EmptyEnvironmentInit")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public ValueTask<bool> EmptyEnvironmentInit([FromQuery] string envId) => _iFileServices.EmptyEnvironmentInit(envId);

    [HttpPost("EmptyEnvironmentValidation")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public ValueTask<bool> EmptyEnvironmentValidation([FromQuery] string envId) => _iFileServices.EmptyEnvironmentValidation(envId);
}