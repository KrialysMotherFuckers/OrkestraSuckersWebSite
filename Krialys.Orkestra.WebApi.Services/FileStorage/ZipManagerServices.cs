using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Krialys.Common.Extensions;
using Krialys.Data.EF.Model;
using Krialys.Entities.COMMON;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Krialys.Data.EF.Model.StorageRequest;

namespace Krialys.Orkestra.WebApi.Services.FileStorage;

/// <summary>
/// In memory zip manager
/// </summary>
public interface IZipManagerServices : ITransientService, IDisposable
{
    /// <summary>
    /// In memory zip content
    /// </summary>
    byte[] ZipContent { get; set; }

    /// <summary>
    /// [Optional] Set or update compression level (only when necessary, by default the compression level is set to 7)
    /// </summary>
    /// <param name="compressionLevel"></param>
    void SetCompressionLevel(int compressionLevel = 7);

    /// <summary>
    /// Set or update metadata name
    /// </summary>
    void SetMetaDataName(string description);

    /// <summary>
    /// Reset in memory zip content
    /// </summary>
    void ResetZipContent();

    /// <summary>
    /// Create a new in memory zip
    /// </summary>
    /// <param name="dataList"></param>
    /// <returns>Number of new entries that were added</returns>
    ValueTask<int> CreateZipAsync(IEnumerable<KeyValuePair<string, byte[]>> dataList);

    /// <summary>
    /// Create a new in memory zip from a given directory
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <returns>Byte array of entries that were added</returns>
    ValueTask<bool> ZipFromDirectoryAsync(string sourceDirectory);

    /// <summary>
    /// Add new entries as in memory zip
    /// </summary>
    /// <param name="newEntries"></param>
    /// <returns>Number of new entries that were added</returns>
    ValueTask<int> AddZipEntriesAsync(IEnumerable<KeyValuePair<string, byte[]>> newEntries);

    /// <summary>
    /// Remove zip entries from in memory zip
    /// </summary>
    /// <param name="entryNamesToRemove"></param>
    /// <returns>Number of new entries that were removed</returns>
    ValueTask<int> RemoveZipEntriesAsync(IEnumerable<string> entryNamesToRemove);

    /// <summary>
    /// Get metadata from an in memory zip
    /// </summary>
    /// <param name="requestType"></param>
    /// <returns></returns>
    MetaData GetZipMetaData(StorageRequestType requestType);

    /// <summary>
    /// Get metadata from an json metadata
    /// </summary>
    /// <param name="jsonMetaData"></param>
    MetaData GetZipMetaData(string jsonMetaData);

    /// <summary>
    /// Get json from a metadata
    /// </summary>
    /// <param name="metaData"></param>
    string GetZipMetaData(MetaData metaData);

    /// <summary>
    /// Get all directory names from metaData
    /// </summary>
    /// <param name="jsonMetaData">Json string</param>
    /// <returns>A string list of all directories</returns>
    IEnumerable<string> GetZipDirNames(string jsonMetaData);

    /// <summary>
    /// Get all file names from jsonMetaData
    /// </summary>
    /// <param name="jsonMetaData">Json string</param>
    /// <returns>An enumerable string of all files</returns>
    IEnumerable<string> GetZipFileNames(string jsonMetaData);

    /// <summary>
    /// Save in memory zip as a physical zip file
    /// </summary>
    /// <param name="outputFullPathName"></param>
    ValueTask SaveAsZipFileAsync(string outputFullPathName);

    /// <summary>
    /// Unzip in memory zip as physical files
    /// </summary>
    /// <param name="outputFullPathName"></param>
    /// <param name="entryNamesToSave"></param>
    ValueTask UnZipFilesAsync(string outputFullPathName, IEnumerable<string> entryNamesToSave);
}

/// <summary>
/// In memory zip manager
/// </summary>
public sealed class ZipManagerServices : IZipManagerServices
{
    #region Ctor
    public byte[] ZipContent { get; set; }

    private const int DefaultCompressionLevel = 7;
    private const int MaxBufferSize = 4096;

    private int _compressionLevel;
    private string _metaDataPath;
    private string _metaDataName;

    /// <summary>
    /// Reset in memory zip content
    /// </summary>
    public void ResetZipContent()
    {
        ZipContent = default!;
        _metaDataPath = null;
        _metaDataName = null!;
    }

    /// <summary>
    /// Set or update metaData path
    /// </summary>
    private string SetMetaDataPath(string metaDataPath)
    {
        _metaDataPath = (string.IsNullOrEmpty(metaDataPath) ? $"{GetShortId()}{Path.AltDirectorySeparatorChar}"
            : (metaDataPath.EndsWith(Path.AltDirectorySeparatorChar) ? metaDataPath : $"{metaDataPath}{Path.AltDirectorySeparatorChar}"))
                .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return _metaDataPath;
    }

    /// <summary>
    /// Set or update compression level
    /// </summary>
    /// <param name="compressionLevel">Value can be set from 0 to 8</param>
    public void SetCompressionLevel(int compressionLevel = DefaultCompressionLevel)
    {
        _compressionLevel = compressionLevel is < 0 or > 8
            ? DefaultCompressionLevel
            : compressionLevel;
    }

    /// <summary>
    /// Set or update metadata name
    /// </summary>
    public void SetMetaDataName(string description) => _metaDataName = description;

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose() => FreeZipMemory();
    #endregion

    //var options = new JsonSerializerOptions();
    //options.Converters.Add(new JsonStringEnumConverter());

    #region UuId
    private const int MaxRandomKeyLength = 12;
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    private static readonly StringBuilder SRandom = new StringBuilder(MaxRandomKeyLength);
    private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static string GetShortId()
    {
        var randomBytes = new byte[MaxRandomKeyLength];
        Rng.GetBytes(randomBytes);

        SRandom.Clear();
        for (var i = 0; i < MaxRandomKeyLength; i++)
            SRandom.Append(Base62Chars[randomBytes[i] % 62]);
        SRandom[0] = '$';

        return SRandom.ToString();
    }
    #endregion

    #region InMemoryZip API
    /// <summary>
    /// Create a new in memory zip
    /// </summary>
    /// <param name="dataList"></param>
    /// <returns>Number of new entries that were added</returns>
    public async ValueTask<int> CreateZipAsync(IEnumerable<KeyValuePair<string, byte[]>> dataList)
    {
        var count = 0;

        var dateTime = DateExtensions.TruncateToSecond(DateTimeOffset.Now.DateTime);

        ZipContent = default!;

        using var memoryStream = new MemoryStream();
        await using (var zipStream = new ZipOutputStream(memoryStream))
        {
            zipStream.SetLevel(_compressionLevel);

            foreach (var entry in dataList)
            {
                var entryKey = ZipEntry.CleanName(entry.Key);

                var zipEntry = new ZipEntry(entryKey)
                {
                    Size = entry.Value.Length,
                    DateTime = dateTime,
                    CompressionMethod = entryKey.EndsWith(Path.AltDirectorySeparatorChar)
                        ? CompressionMethod.Stored
                        : GetCompressionMethod(entryKey)
                };
                await zipStream.PutNextEntryAsync(zipEntry);
                await zipStream.WriteAsync(entry.Value.AsMemory(0, entry.Value.Length));
                await zipStream.CloseEntryAsync(CancellationToken.None);
                count++;
            }

            await zipStream.FinishAsync(CancellationToken.None);

            memoryStream.Seek(0, SeekOrigin.Begin);
            ZipContent = memoryStream.ToArray();

            zipStream.Close();
        }

        FreeZipMemory(false);

        return count;
    }

    /// <summary>
    /// Create a new in memory zip from a given directory
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <returns>Byte array of entries that were added</returns>
    public async ValueTask<bool> ZipFromDirectoryAsync(string sourceDirectory)
    {
        using var memoryStream = new MemoryStream();
        await using var zipOutputStream = new ZipOutputStream(memoryStream);

        // Set compression level
        zipOutputStream.SetLevel(_compressionLevel);

        // Recursively add files and subdirectories to the zip
        await AddDirectoryToZipAsync(zipOutputStream, sourceDirectory, "");

        await zipOutputStream.FinishAsync(CancellationToken.None);
        zipOutputStream.Close();

        ZipContent = memoryStream.ToArray();

        FreeZipMemory(false);

        return ZipContent.Length > 0;

        static async Task AddDirectoryToZipAsync(ZipOutputStream zipOutputStream, string sourceDirectory, string relativePath)
        {
            //Get all files and directories within the source directory
            var files = Directory.GetFiles(sourceDirectory);
            var subDirectories = Directory.GetDirectories(sourceDirectory);
            var dateTime = DateExtensions.TruncateToSecond(DateTimeOffset.Now.DateTime);

            foreach (var file in files)
            {
                // Create a new entry for the file

                var entryKey = ZipEntry.CleanName(Path.Combine(relativePath, Path.GetFileName(file)));

                var entry = new ZipEntry(entryKey)
                {
                    DateTime = dateTime,
                    Size = new FileInfo(file).Length,
                    CompressionMethod = entryKey.EndsWith(Path.AltDirectorySeparatorChar)
                        ? CompressionMethod.Stored
                        : GetCompressionMethod(entryKey)
                };

                await zipOutputStream.PutNextEntryAsync(entry);

                // Copy the file data into the zip stream
                await using var fileStream = File.OpenRead(file);
                StreamUtils.Copy(fileStream, zipOutputStream, new byte[MaxBufferSize]);
                await zipOutputStream.CloseEntryAsync(CancellationToken.None);
            }

            // Add a placeholder file for empty directories
            if (files.Length == 0 && relativePath.Length > 0)
            {
                var entry = new ZipEntry($"{relativePath}/")
                {
                    DateTime = dateTime,
                    Size = 0, // Empty file
                    CompressionMethod = CompressionMethod.Stored
                };

                await zipOutputStream.PutNextEntryAsync(entry);
                await zipOutputStream.CloseEntryAsync(CancellationToken.None);
            }

            foreach (var subDirectory in subDirectories)
            {
                // Recursively add subdirectories
                var subDirectoryRelativePath = Path.Combine(relativePath, Path.GetFileName(subDirectory));
                await AddDirectoryToZipAsync(zipOutputStream, subDirectory, subDirectoryRelativePath);
            }
        }
    }

    /// <summary>
    /// Add new entries as in memory zip
    /// </summary>
    /// <param name="newEntries"></param>
    /// <returns>Number of new entries that were added</returns>
    public async ValueTask<int> AddZipEntriesAsync(IEnumerable<KeyValuePair<string, byte[]>> newEntries)
    {
        if (ZipContent is not { Length: > 0 })
            return 0;

        var count = 0;

        using var modifiedZipStream = new MemoryStream();
        await using (var zipOutputStream = new ZipOutputStream(modifiedZipStream))
        {
            zipOutputStream.SetLevel(_compressionLevel);

            // Create a dictionary to keep track of added entry names
            var addedEntryNames = new HashSet<string>();
            DateTime? dateTime = null;

            using var originalZipStream = new MemoryStream(ZipContent);
            await using var zipInputStream = new ZipInputStream(originalZipStream);
            var buffer = new byte[MaxBufferSize];

            while (zipInputStream.GetNextEntry() is { } entry)
            {
                var entryName = ZipEntry.CleanName(entry.Name);
                await zipOutputStream.PutNextEntryAsync(new ZipEntry(entryName)
                {
                    Size = entry.Size,
                    DateTime = entry.DateTime,
                    CompressionMethod = entry.CompressionMethod
                });

                int bytesRead;
                while ((bytesRead = await zipInputStream.ReadAsync(buffer)) > 0)
                    await zipOutputStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                await zipOutputStream.CloseEntryAsync(CancellationToken.None);

                addedEntryNames.Add(entryName);
                dateTime ??= entry.DateTime;
            }

            // Add new entries to the modified zip
            foreach (var newEntry in newEntries)
            {
                var entryKey = ZipEntry.CleanName(newEntry.Key);

                // Skip duplicate entries
                if (addedEntryNames.Contains(entryKey))
                    continue;

                await zipOutputStream.PutNextEntryAsync(new ZipEntry(entryKey)
                {
                    Size = newEntry.Value.Length,
                    DateTime = dateTime.GetValueOrDefault(),
                    CompressionMethod = entryKey.EndsWith(Path.AltDirectorySeparatorChar)
                        ? CompressionMethod.Stored
                        : GetCompressionMethod(entryKey)
                });

                await zipOutputStream.WriteAsync(newEntry.Value.AsMemory(0, newEntry.Value.Length));
                await zipOutputStream.CloseEntryAsync(CancellationToken.None);
                count++;
            }

            await zipOutputStream.FinishAsync(CancellationToken.None);
            await zipOutputStream.FlushAsync();
        }

        // Return the modified zip content as a byte array
        ZipContent = modifiedZipStream.ToArray();

        FreeZipMemory(false);

        return count;
    }

    /// <summary>
    /// Remove zip entries from in memory zip
    /// </summary>
    /// <param name="entryNamesToRemove"></param>
    /// <returns>Number of new entries that were removed</returns>
    public async ValueTask<int> RemoveZipEntriesAsync(IEnumerable<string> entryNamesToRemove)
    {
        if (ZipContent is not { Length: > 0 })
            return 0;

        var count = 0;

        // Create a MemoryStream from the original zip content
        using var originalZipStream = new MemoryStream(ZipContent);

        // Create a new MemoryStream for the modified zip
        using var modifiedZipStream = new MemoryStream();

        // Create ZipOutputStream with the new MemoryStream
        await using var zipOutputStream = new ZipOutputStream(modifiedZipStream);
        zipOutputStream.SetLevel(_compressionLevel);

        var namesToRemove = entryNamesToRemove.ToHashSet();

        // Iterate through the entries in the original zip
        var buffer = new byte[MaxBufferSize];
        await using var zipInputStream = new ZipInputStream(originalZipStream);

        while (zipInputStream.GetNextEntry() is { } entry)
        {
            // Skip entries to be removed
            if (namesToRemove.Contains(entry.Name))
            {
                count++;
                continue;
            }

            // Create a new entry in the modified zip
            await zipOutputStream.PutNextEntryAsync(new ZipEntry(ZipEntry.CleanName(entry.Name))
            {
                Size = entry.Size,
                DateTime = entry.DateTime,
                CompressionMethod = entry.CompressionMethod
            });

            // Copy the entry's content to the modified zip using a smaller buffer
            int bytesRead;
            while ((bytesRead = await zipInputStream.ReadAsync(buffer)) > 0)
                await zipOutputStream.WriteAsync(buffer.AsMemory(0, bytesRead));

            await zipOutputStream.CloseEntryAsync(CancellationToken.None);
        }

        await zipOutputStream.FinishAsync(CancellationToken.None);
        await zipOutputStream.FlushAsync();

        // Return the modified zip content as a byte array
        ZipContent = modifiedZipStream.ToArray();

        FreeZipMemory(false);

        return count;
    }

    /// <summary>
    /// Unzip in memory zip as physical files
    /// </summary>
    /// <param name="outputFullPathName"></param>
    /// <param name="entryNamesToSave"></param>
    public async ValueTask UnZipFilesAsync(string outputFullPathName, IEnumerable<string> entryNamesToSave)
    {
        if (ZipContent is not { Length: > 0 })
            return;

        using var memoryStream = new MemoryStream(ZipContent, false);
        await using var zipInputStream = new ZipInputStream(memoryStream);
        var namesToSave = entryNamesToSave.ToList();

        while (zipInputStream.GetNextEntry() is { } zipEntry)
        {
            var entryKey = ZipEntry.CleanName(zipEntry.Name);

            if (!namesToSave.Contains(entryKey))
                continue;

            var dir = Path.Combine(Path.GetDirectoryName(outputFullPathName)!, Path.GetDirectoryName(entryKey)!);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (entryKey.EndsWith(Path.AltDirectorySeparatorChar))
                continue;

            await using var fileStream = File.Create(Path.Combine(dir, Path.GetFileName(entryKey)));

            var buffer = new byte[MaxBufferSize];
            int bytesRead;
            while ((bytesRead = await zipInputStream.ReadAsync(buffer)) > 0)
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));

            await fileStream.DisposeAsync();
        }
    }

    /// <summary>
    /// Save in memory zip as a physical zip file
    /// </summary>
    /// <param name="outputFullPathName"></param>
    public async ValueTask SaveAsZipFileAsync(string outputFullPathName)
    {
        if (ZipContent is not { Length: > 0 })
            return;

        var dir = Path.GetDirectoryName(outputFullPathName);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await using var fileStream = new FileStream(outputFullPathName, FileMode.Create, FileAccess.Write);

        var offset = 0;
        while (offset < ZipContent.Length)
        {
            var bytesToWrite = Math.Min(MaxBufferSize, ZipContent.Length - offset);
            await fileStream.WriteAsync(ZipContent.AsMemory(offset, bytesToWrite));
            offset += bytesToWrite;
        }

        await fileStream.DisposeAsync();
    }

    /// <summary>
    /// Get metadata from an in memory zip
    /// </summary>
    /// <param name="requestType"></param>
    /// <returns></returns>
    public MetaData GetZipMetaData(StorageRequestType requestType)
    {
        if (ZipContent is not { Length: > 0 })
            return (default!);

        IList<Entry> entries = new List<Entry>(capacity: 0);

        var utcNow = DateExtensions.TruncateToSecond(DateTimeOffset.Now.DateTime.ToUniversalTime());

        using (var memoryStream = new MemoryStream(ZipContent, false))
        {
            using var zipInputStream = new ZipInputStream(memoryStream);

            while (zipInputStream.GetNextEntry() is { } zipEntry)
            {
                string entryKey = ZipEntry.CleanName(zipEntry.Name);
                string mimeType = default!;
                if (!zipEntry.IsDirectory)
                {
                    MimeTypes.TryGetValue(Path.GetExtension(entryKey).ToLower(), out mimeType);
                    mimeType ??= MimeTypes[DefaultMimeType];
                }

                entries.Add(new Entry()
                {
                    IsDirectory = zipEntry.IsDirectory,
                    Name = zipEntry.IsDirectory ? default! : Path.GetFileName(entryKey),
                    MimeType = mimeType,
                    FullName = string.IsNullOrEmpty(entryKey)
                        ? default!
                        : entryKey,
                    Length = zipEntry.Size,
                    CompressionLength = zipEntry.CompressedSize,
                }); ;
            }
        }

        var metaData = new MetaData
        {
            Type = requestType,
            Path = _metaDataPath,
            Name = _metaDataName,
            Entries = entries.OrderBy(e => e.FullName),
        };

        metaData.Path = SetMetaDataPath(metaData.Path);

        return metaData;
    }

    public MetaData GetZipMetaData(string jsonMetaData)
    {
        if (string.IsNullOrEmpty(jsonMetaData))
            return null;

        var metaData = JsonSerializer.Deserialize<MetaData>(jsonMetaData, DbFieldsExtensions.SerializerOptions());
        metaData.Name = _metaDataName;
        metaData.Path = SetMetaDataPath(metaData.Path);
        metaData.Entries = GetZipMetaData(metaData.Type).Entries;

        return metaData;
    }

    public string GetZipMetaData(MetaData metaData)
    {
        if (metaData == null)
            return null;

        metaData.Name = _metaDataName;
        metaData.Path = SetMetaDataPath(metaData.Path);

        return JsonSerializer.Serialize(metaData, DbFieldsExtensions.SerializerOptions());
    }
    /// <summary>
    /// Get all directory names from metaData
    /// </summary>
    /// <param name="jsonMetaData">Json string</param>
    /// <returns>A string list of all directories</returns>
    public IEnumerable<string> GetZipDirNames(string jsonMetaData)
    {
        var hashDir = new HashSet<string>();

        if (string.IsNullOrEmpty(jsonMetaData))
            return hashDir;

        var metaData = JsonSerializer.Deserialize<MetaData>(jsonMetaData)!;
        foreach (var entry in metaData.Entries)
        {
            if (entry is { IsDirectory: true })
            {
                hashDir.Add(ZipEntry.CleanName(entry.FullName).EndsWith(Path.AltDirectorySeparatorChar) ? entry.FullName[..^1] : entry.FullName);
            }
            else
            {
                var dir = ZipEntry.CleanName(entry.FullName.Replace(entry.Name, ""));
                if (!string.IsNullOrEmpty(dir))
                    hashDir.Add(dir.EndsWith(Path.AltDirectorySeparatorChar) ? dir[..^1] : dir);
            }
        }

        return hashDir;
    }

    /// <summary>
    /// Get all file names from jsonMetaData
    /// </summary>
    /// <param name="jsonMetaData">Json string</param>
    /// <returns>An enumerable string of all files</returns>
    public IEnumerable<string> GetZipFileNames(string jsonMetaData)
    {
        if (string.IsNullOrEmpty(jsonMetaData))
            return Enumerable.Empty<string>();

        return JsonSerializer.Deserialize<MetaData>(jsonMetaData)!
            .Entries.Select(e => ZipEntry.CleanName(e.FullName))
            .Where(e => !e.EndsWith(Path.AltDirectorySeparatorChar))
            .ToList();
    }

    /// <summary>
    /// Free memory
    /// </summary>
    /// <param name="complete"></param>
    private void FreeZipMemory(bool complete = true)
    {
        if (!complete)
            GC.Collect(GC.MaxGeneration);
        else
        {
            ZipContent = null!;
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }
    }

    /// <summary>
    /// Get compression method based on file name extension
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>CompressionMethod</returns>
    private static CompressionMethod GetCompressionMethod(string fileName)
        => Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpeg" or ".jpg" or ".png" or ".gif"
                or ".jar" or ".zip" or ".7z" or ".gz" or ".br" or ".tar" or ".rar" or ".bzip" or ".iso"
                or ".mp3" or ".flac" or ".avi" or ".mp4" => CompressionMethod.Stored,

            _ => CompressionMethod.Deflated,
        };
    #endregion
}