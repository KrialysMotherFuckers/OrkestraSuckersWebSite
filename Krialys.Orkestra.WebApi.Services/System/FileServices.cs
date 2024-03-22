using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Model;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;
using Krialys.Orkestra.WebApi.Services.FileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace Krialys.Orkestra.WebApi.Services.System;

public interface IFileServices : IScopedService
{
    IList<VSCU_CTRL_STRUCTURE_UPLOADS> GetStructureUploadRules(string envId);
    Task<int> UpdateTE_ETATS(string envId, string envUploaded, long envZipFileSize);
    Task<int> AddOrUpdateTEB_ETAT_BATCHS(string envId, List<FileInfo> batchFiles);

    ValueTask<bool> EmptyEnvironmentInit(string envId);
    bool CreateDirectoryBaseandFolders(string envId, bool unzipIfZipEnvExist, string prefixZip);
    ValueTask<bool> EmptyEnvironmentUpdload(IList<IFormFile> chunkFile, IList<IFormFile> uploadFiles, string envId);
    ValueTask<bool> RemoveEmptyEnvironmentZip(string envId);
    ValueTask<bool> EmptyEnvironmentValidation(string envId);
}

public class FileServices : IFileServices
{
    private readonly Krialys.Data.EF.Univers.KrialysDbContext _dbContext;
    private readonly Krialys.Data.EF.FileStorage.KrialysDbContext _dbFileStorageContext;
    private readonly IPhysicalFileProviderBase _iPhysicalFileProviderBase;
    private readonly IZipManagerServices _iZipManagerServices;
    private readonly IConfiguration _configuration;
    private readonly ICommonServices _commonServices;
    private readonly Serilog.ILogger _logger;

    private readonly Encoding _encoding = Encoding.GetEncoding(850);
    private const int Error = -1;
    private string _rootFolder;

    public FileServices(
        Krialys.Data.EF.Univers.KrialysDbContext dbContext,
        Krialys.Data.EF.FileStorage.KrialysDbContext dbFileStorageContext,
        IPhysicalFileProviderBase iPhysicalFileProviderBase,
        IZipManagerServices iZipManagerServices,
        IConfiguration configuration,
        ICommonServices commonServices,
        Serilog.ILogger logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbFileStorageContext = dbFileStorageContext ?? throw new ArgumentNullException(nameof(dbFileStorageContext));
        _iPhysicalFileProviderBase = iPhysicalFileProviderBase ?? throw new ArgumentNullException(nameof(iPhysicalFileProviderBase));
        _iZipManagerServices = iZipManagerServices ?? throw new ArgumentNullException(nameof(iZipManagerServices));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _commonServices = commonServices;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _rootFolder = _configuration["ParallelU:PathEnvVierge"];
    }

    /// <summary>
    /// Convert EnvironmentId into EtatId
    /// </summary>
    /// <param name="envId"></param>
    /// <returns></returns>
    private int ToEtatId(string envId) => envId.Length > 6 ? Convert.ToInt32(envId.Substring(1, 6)) : Error;

    /// <summary>
    /// Get environment structure matrix rules
    /// </summary>
    /// <param name="envId"></param>
    /// <returns></returns>
    public IList<VSCU_CTRL_STRUCTURE_UPLOADS> GetStructureUploadRules(string envId)
    {
        IList<VSCU_CTRL_STRUCTURE_UPLOADS> data = new List<VSCU_CTRL_STRUCTURE_UPLOADS>();

        int etatId = ToEtatId(envId);

        if (etatId != Error)
        {
            data = _dbContext.VSCU_CTRL_STRUCTURE_UPLOADS
                .Where(x => x.TE_ETATID == etatId)
                .OrderBy(o => o.TLEM_ACTION)
                .ToList();
        }

        return data;
    }

    /// <summary>
    /// Update TE_ETATS
    /// </summary>
    /// <param name="envId"></param>
    /// <param name="envUploaded"></param>
    /// <param name="envZipFileSize"></param>
    /// <returns></returns>
    public async Task<int> UpdateTE_ETATS(string envId, string envUploaded, long envZipFileSize)
    {
        int etatId = ToEtatId(envId);
        int count = 0;

        if (etatId != Error)
        {
            await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
            {
                await _dbContext.Database.OpenConnectionAsync();
                await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    await dbCTrans.CreateSavepointAsync("Step_001");

                    var data = _dbContext.TE_ETATS.FirstOrDefault(x => x.TE_ETATID == etatId);
                    int length = envZipFileSize < int.MaxValue ? Convert.ToInt32(envZipFileSize) : int.MaxValue;

                    if (data != null)
                    {
                        data.TE_ENV_VIERGE_UPLOADED = envUploaded ?? throw new ArgumentNullException(nameof(envUploaded));
                        data.TE_ENV_VIERGE_TAILLE = length < 1 ? null : length;
                        _dbContext.TE_ETATS.Update(data);
                    }

                    count = await _dbContext.SaveChangesAsync();

                    // Commit
                    await dbCTrans.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error: {Message}", ex.Message);
                    await dbCTrans.RollbackToSavepointAsync("Step_001");
                }
            });

            return count;
        }

        return Error;
    }

    /// <summary>
    /// Add or Update TEB_ETAT_BATCHS
    /// </summary>
    /// <param name="envId"></param>
    /// <param name="batchFiles"></param>
    /// <returns></returns>
    public async Task<int> AddOrUpdateTEB_ETAT_BATCHS(string envId, List<FileInfo> batchFiles)
    {
        int count = Error;
        int etatId = ToEtatId(envId);
        var envDateCreation = DateExtensions.GetUtcNow();

        if (etatId != Error)
        {
            await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
            {
                await _dbContext.Database.OpenConnectionAsync();
                await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    await dbCTrans.CreateSavepointAsync("Step_001");

                    // Get DB batch list
                    var dbBatchs = _dbContext.TEB_ETAT_BATCHS.Where(x => x.TE_ETATID == etatId).ToList();

                    // Add new batch to DB, set its status to Actif
                    foreach (var batch in batchFiles.Where(batch => !dbBatchs.Any(x => x.TEB_CMD.Equals(batch.Name, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        await _dbContext.TEB_ETAT_BATCHS.AddAsync(new TEB_ETAT_BATCHS
                        {
                            TEB_CMD = batch.Name,
                            TEB_DATE_CREATION = envDateCreation,
                            //TEB_DESCR = "(new item)",
                            TEB_NOM_AFFICHAGE = batch.Name,
                            TE_ETATID = etatId,
                            TRST_STATUTID = StatusLiteral.Available
                        });
                    }

                    int batchAdded = await _dbContext.SaveChangesAsync();
                    int inactivedBatch = 0;
                    count = batchAdded != 0 ? batchAdded : 0;

                    // Update existing DB batch, set its status to deactivated, else Remove batch from table if not physically found
                    foreach (var batch in dbBatchs)
                    {
                        var item = _dbContext.TEB_ETAT_BATCHS.FirstOrDefault(x => x.TE_ETATID == etatId && x.TEB_CMD == batch.TEB_CMD);// && x.TRST_STATUTID == StatusLiteral.Available);
                        var found = batchFiles.Exists(x => x.Name.Equals(item?.TEB_CMD));

                        switch (found)
                        {
                            // Item exist, BUT can also be an old Inactived batch, then added a new batch renamed to original batch name
                            case true:
                                {
                                    if (item != null)
                                    {
                                        item.TRST_STATUTID = StatusLiteral.Available;
                                        _dbContext.TEB_ETAT_BATCHS.Update(item);
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (item != null)
                                    {
                                        item.TRST_STATUTID = StatusLiteral.Deactivated;
                                        _dbContext.TEB_ETAT_BATCHS.Update(item);
                                    }

                                    inactivedBatch += 1;
                                    break;
                                }
                        }
                        count += await _dbContext.SaveChangesAsync();
                    }

                    if (dbBatchs.Count > 0)
                        count = inactivedBatch <= dbBatchs.Count ? 1 : inactivedBatch;

                    // Commit
                    await dbCTrans.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error: {Message}", ex.Message);
                    await dbCTrans.RollbackToSavepointAsync("Step_001");
                }
            });
        }

        return count;
    }

    /// <summary>
    /// Create base paths (if folder is empty then the filemanager crashes: SF bug again)
    /// </summary>
    public bool CreateDirectoryBaseandFolders(string envId, bool unzipIfZipEnvExist, string prefixZip)
    {
        var result = false;

        try
        {
            var zipFilePath = $"{_rootFolder}{prefixZip}{envId}.zip";
            var envRootFolder = Path.Combine(_rootFolder, envId);

            if (File.Exists(zipFilePath) && unzipIfZipEnvExist && !Directory.Exists(envRootFolder))
                ZipFile.ExtractToDirectory(zipFilePath, envRootFolder, _encoding);

            if (!Directory.Exists($"{envRootFolder}/Batch")) Directory.CreateDirectory($"{envRootFolder}/Batch");
            if (!Directory.Exists($"{envRootFolder}/Documents")) Directory.CreateDirectory($"{envRootFolder}/Documents");
            if (!Directory.Exists($"{envRootFolder}/Output")) Directory.CreateDirectory($"{envRootFolder}/Output");
            if (!Directory.Exists($"{envRootFolder}/Qualif")) Directory.CreateDirectory($"{envRootFolder}/Qualif");
            if (!Directory.Exists($"{envRootFolder}/Spshare")) Directory.CreateDirectory($"{envRootFolder}/Spshare");

            // Generate zip file fitting to its directory structure
            //if (!File.Exists(zipFilePath) && unzipIfZipEnvExist)
            //    ZipFile.CreateFromDirectory(envRootFolder, zipFilePath, CompressionLevel.Fastest, false, _encoding);

            result = true;
        }
        catch { }

        return result;
    }

    public async ValueTask<bool> EmptyEnvironmentInit(string envId)
    {
        var result = false;

        // Create expected minimal structure
        CreateDirectoryBaseandFolders(envId, true, string.Empty);

        return await ValueTask.FromResult(result);
    }

    public async ValueTask<bool> EmptyEnvironmentUpdload(IList<IFormFile> chunkFile, IList<IFormFile> uploadFiles, string envId)
    {
        var result = false;
        var zipFile = string.Empty;
        var file = uploadFiles.FirstOrDefault();
        var uploadedZipFile = $"{_rootFolder}${envId}.zip";

        try
        {
            zipFile = ContentDispositionHeaderValue
                .Parse(file?.ContentDisposition)
                .FileName!
                .Trim('"');

            using (var fs = File.Create(uploadedZipFile))
            {
                file!.CopyTo(fs);
                fs.Flush();
            }

            // Environment directory 
            var targetDir = Path.Combine(_rootFolder, envId);
            // Delete env dir if exists
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);

                // Create expected minimal structure with Zip extraction set to true
                CreateDirectoryBaseandFolders(envId, true, "$");
            }
        }
        finally
        {
            if (File.Exists(uploadedZipFile))
                File.Delete(uploadedZipFile);
        }

        return await ValueTask.FromResult(result);
    }

    public async ValueTask<bool> RemoveEmptyEnvironmentZip(string envId)
    {
        var result = false;
        if (File.Exists($"{_rootFolder}{(envId)}.zip")) File.Delete($"{_rootFolder}{(envId)}.zip");

        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// Add or update empty environment data in storageFile table.
    /// <br/>Only valid empty storage environment are eligible to be saved in database.
    /// </summary>
    /// <param name="envId">Environment Id</param>
    /// <returns></returns>
    public async ValueTask<bool> EmptyEnvironmentValidation(string envId)
    {
        var result = false;

        await using (var transaction = await _dbFileStorageContext.Database.BeginTransactionAsync())
            try
            {
                var originId = Convert.ToInt32(string.IsNullOrEmpty(envId) ? 0 : envId[1..]);
                var fullName = _dbContext.TE_ETATS.FirstOrDefault(e => e.TE_ETATID == originId)?.TE_FULLNAME;
                if (string.IsNullOrEmpty(fullName))
                    throw new ArgumentNullException(nameof(fullName));

                var item = _dbFileStorageContext.TM_STF_StorageFileRequest.FirstOrDefault(x =>
                    x.stf_fk_origin_id == originId
                 && x.sct_id == Krialys.Data.EF.Model.StorageRequestType.EnvironmentEmpty
                 && x.stf_to_be_deleted == false
                 && x.stf_is_deleted == false);

                _iZipManagerServices.SetCompressionLevel(7);
                _iZipManagerServices.SetMetaDataName(fullName);

                if (!await _iZipManagerServices.ZipFromDirectoryAsync($"{_rootFolder}{(envId)}"))
                    throw new Exception("Error when zip");

                var metaJson = _iZipManagerServices.GetZipMetaData(string.IsNullOrEmpty(item?.stf_stream_list)
                    ? _iZipManagerServices.GetZipMetaData(StorageRequestType.EnvironmentEmpty)
                    : _iZipManagerServices.GetZipMetaData(item?.stf_stream_list));

                var utcNow = DateExtensions.TruncateToSecond(DateTimeOffset.Now.DateTime.ToUniversalTime());

                if (item == null)
                {
                    // ADD new item
                    await _dbFileStorageContext.TM_STF_StorageFileRequest.AddAsync(
                        new Krialys.Data.EF.FileStorage.TM_STF_StorageFileRequest()
                        {
                            stf_fk_origin_id = originId,
                            sct_id = Krialys.Data.EF.Model.StorageRequestType.EnvironmentEmpty,
                            stf_create_by = _commonServices.GetUserIdAndName().userName,
                            stf_stream_zipped = _iZipManagerServices.ZipContent,
                            stf_stream_list = metaJson,
                            stf_stream_size = _iZipManagerServices.ZipContent.Length,
                            stf_create_date = utcNow,
                        });
                }
                else
                {
                    // UPDATE existing item
                    item.stf_update_by = _commonServices.GetUserIdAndName().userName;
                    item.stf_stream_zipped = _iZipManagerServices.ZipContent;
                    item.stf_stream_list = metaJson;
                    item.stf_stream_size = _iZipManagerServices.ZipContent.Length;
                    item.stf_update_date = utcNow;

                    _dbFileStorageContext.TM_STF_StorageFileRequest.Update(item);
                }

                await UpdateTE_ETATS(envId, StatusLiteral.Yes, _iZipManagerServices.ZipContent.Length);

                // Save the compressed version (replace existing zip file)
                await _iZipManagerServices.SaveAsZipFileAsync($"{_rootFolder}{(envId)}.zip");

                // Update database
                await _dbFileStorageContext.SaveChangesAsync();
                await transaction.CommitAsync();
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error: {Message}", ex.Message);
                await transaction.RollbackAsync();
            }

        return await ValueTask.FromResult(result);
    }
}