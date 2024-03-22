using System.ComponentModel.DataAnnotations;

namespace Krialys.Data.EF.Model;

/// <summary>
/// Storage request: structure, content and extensions for handling in memory Zip files
/// </summary>
public class StorageRequest
{
    #region MetaData content
    /// <summary>
    /// Storage request MetaData structure
    /// </summary>
    public class MetaData
    {
        /// <summary>
        /// Kind of storage request used
        /// </summary>
        [Required]
        public StorageRequestType Type { get; set; }

        /// <summary>
        /// Storage request name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// To be used when extracting zip content, assuming a unique name when the files are going to be extracted/updated within this folder
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// Zip content, both file and relative directory
        /// </summary>
        [Required]
        public IEnumerable<Entry> Entries { get; set; }
    }

    /// <summary>
    /// Storage request entry content
    /// </summary>
    public class Entry
    {
        public string Name { get; init; } = default!;
        public string FullName { get; init; } = default!;
        public string MimeType { get; init; }
        public bool IsDirectory { get; init; }
        public long CompressionLength { get; init; }
        public long Length { get; init; }
    }
    #endregion

    #region Mime types
    public const string DefaultMimeType = "_default_mime_type_";

    // if (MimeTypes.ContainsKey(extension)) string mimeType = MimeTypes[extension];
    public static IDictionary<string, string> MimeTypes { get; } =
        new Dictionary<string, string>
        {
            { ".bat", "application/x-bat" }, { ".cmd", "application/x-bat" },
            { ".csv", "text/csv" }, { ".tsv", "text/tab-separated-values" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".jpeg", "image/jpeg" }, { ".jpg", "image/jpeg" },
            { ".json", "application/json" },
            { ".pdf", "application/pdf" },
            { ".png", "image/png" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".rtf", "application/rtf" },
            { ".svg", "image/svg+xml" },
            { ".tif", "image/tiff" }, { ".tiff", "image/tiff" },
            { ".txt", "text/plain" }, { ".cs", "text/plain" }, { ".css", "text/css" }, { ".html", "text/html" }, { ".js", "text/javascript" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".zip", "application/zip" }, { ".7z", "application/x-7z-compressed" }, { ".jar", "application/java-archive" },
            { DefaultMimeType, "application/octet-stream" },
        };
    #endregion
}

