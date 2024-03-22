using ExcelDataReader;
using System.Data;
using System.Net;
using System.Text;

namespace Krialys.Common.Excel;

/// <summary>
/// Download, parse, then returns Json and/or typed Object from an Excel (97 to 2019), or from csv file
/// Example #1 (typed result): await instance.Load PRODUCTS (@"C:\Products.xlsx", "Produits frais");
/// Example #2 (key/value result): await instance.Load(@"C:\SampleA.xls", "SheetA", new[] { "Id", "Key", "Name", "Category", "Quantity", "Comment" });
/// </summary>
public class ExcelReader
{
    private string _lastError;

    /// <summary>Gets the last error.</summary>
    /// <returns>Return <strong>error's description</strong>, else <strong>null</strong> if no error.</returns>
    public string GetLastError() => _lastError;

    public static ExcelReader CreateInstance() => new();

    private ExcelReader() => _lastError = string.Empty;

    public async Task<IList<TEntity>> Load<TEntity>(string fileName, string sheetName, bool headerRow = true, string emptyColumnNamePrefix = null, string password = null) where TEntity : class, new()
    {
        _lastError = string.Empty;

        try
        {
            return await LoadAndConvert<TEntity>(fileName, sheetName, headerRow, emptyColumnNamePrefix, password);
        }
        catch (Exception ex)
        {
            _lastError = $"FileName: {fileName}|SheetName: {sheetName}|Message: {ex.Message}";
        }

        return default;
    }

    public async Task<IList<IDictionary<string, object>>> Load(string fileName, string sheetName, string[] manualMapping, string emptyColumnNamePrefix = null, string password = null)
    {
        _lastError = string.Empty;

        try
        {
            return await LoadAndConvert(fileName, sheetName, manualMapping is null, manualMapping, emptyColumnNamePrefix, password);
        }
        catch (Exception ex)
        {
            _lastError = $"FileName: {fileName}|SheetName: {sheetName}|Message: {ex.Message}";
        }

        return default;
    }

    /// <summary>
    /// Convert Dictionary to Entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dict"></param>
    /// <param name="strict"></param>
    /// <returns></returns>
    public TEntity Map<TEntity>(IDictionary<string, object> dict, bool strict = false) where TEntity : class, new()
    {
        _lastError = string.Empty;
        var t = new TEntity();
        var properties = t.GetType().GetProperties();

        try
        {
            foreach (var property in properties)
            {
                if (strict && !dict.Any(x => x.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var item = dict.First(x => x.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

                // Find which property type (int, string, double? etc) the CURRENT property is...
                var tPropertyType = t.GetType().GetProperty(property.Name)?.PropertyType;

                if (tPropertyType is null) continue;

                // Fix nullables...
                var newT = Nullable.GetUnderlyingType(tPropertyType) ?? tPropertyType;

                // ... and change the type
                t.GetType().GetProperty(property.Name)?.SetValue(obj: t, Convert.ChangeType(item.Value, newT), null);
            }
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
        }

        return t;
    }

    private async Task<IList<TEntity>> LoadAndConvert<TEntity>(string fileName, string sheetName, bool useHeaderRow,
        string emptyColumnNamePrefix, string password) where TEntity : class, new()
    {
        var data = new List<TEntity>();
        IDictionary<string, object> row = new Dictionary<string, object>();

        try
        {
            await using MemoryStream stream = new(await DownloadFile(fileName));

            if (string.IsNullOrEmpty(_lastError))
            {
                stream.Seek(0, SeekOrigin.Begin);

                var config = new ExcelReaderConfiguration
                {
                    // Gets or sets the encoding to use when the input XLS lacks a CodePage record,
                    // or when the input CSV lacks a BOM and does not parse as UTF8.
                    // Default: cp1252 (XLS BIFF2-5 and CSV only)
                    FallbackEncoding = Encoding.GetEncoding(1252),

                    // Gets or sets the password used to open password protected workbooks.
                    Password = password,

                    // Gets or sets an array of CSV separator candidates. The reader
                    // autodetects which best fits the input data. Default: , ; TAB | #
                    // (CSV only)
                    AutodetectSeparators = new[] { ',', ';', '\t', '|', '#', '¤', '^', '$' },

                    // Gets or sets a value indicating the number of rows to analyze for
                    // encoding, separator and field count in a CSV. When set, this option
                    // causes the IExcelDataReader.RowCount property to throw an exception.
                    // Default: 0 - analyzes the entire file (CSV only, has no effect on other formats)
                    AnalyzeInitialCsvRows = 0,

                    // Gets or sets a value indicating whether to leave the stream open after the IExcelDataReader object is disposed.
                    LeaveOpen = false
                };

                // XLS, XLSX or CSV/TSV/TXT ?
                var ext = Path.GetExtension(fileName).ToLower();

                using var reader = ext switch
                {
                    ".xls" or ".xlsx" or ".xlsb" => ExcelReaderFactory.CreateReader(stream, config),
                    ".csv" or ".tsv" or ".txt" => ExcelReaderFactory.CreateCsvReader(stream, config),
                    _ => throw new NotSupportedException($"Error: {ext} type/extension not supported!"),
                };

                // Get Dataset
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    UseColumnDataType = true,
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = useHeaderRow,
                        EmptyColumnNamePrefix = emptyColumnNamePrefix
                    }
                });

                // Get Table
                using var dataTable = ext switch
                {
                    ".xls" or ".xlsx" or ".xlsb" => ds.Tables[sheetName ?? ""],
                    ".csv" or ".tsv" or ".txt" => ds.Tables[0],
                    _ => throw new NotSupportedException($"Error: {sheetName ?? "Sheet"} not found!"),
                };

                // Transpose data's to typed object
                if (dataTable is not null)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            row.Add(col.ColumnName, dr[col]);
                        }
                        data.Add(Map<TEntity>(row));
                        row.Clear();
                    }

                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
        }

        throw new ArgumentNullException(string.IsNullOrEmpty(_lastError) ? $"Could not find sheet '{sheetName ?? "Sheet"}'." : _lastError);
    }

    private async Task<IList<IDictionary<string, object>>> LoadAndConvert(string fileName, string sheetName, bool useHeaderRow,
        string[] manualMapping, string emptyColumnNamePrefix, string password)
    {
        IList<IDictionary<string, object>> data = new List<IDictionary<string, object>>();

        try
        {
            await using MemoryStream stream = new(await DownloadFile(fileName));

            if (string.IsNullOrEmpty(_lastError))
            {
                stream.Seek(0, SeekOrigin.Begin);

                var config = new ExcelReaderConfiguration
                {
                    // Gets or sets the encoding to use when the input XLS lacks a CodePage record,
                    // or when the input CSV lacks a BOM and does not parse as UTF8.
                    // Default: cp1252 (XLS BIFF2-5 and CSV only)
                    FallbackEncoding = Encoding.GetEncoding(1252),

                    // Gets or sets the password used to open password protected workbooks.
                    Password = password,

                    // Gets or sets an array of CSV separator candidates. The reader
                    // autodetects which best fits the input data. Default: , ; TAB | #
                    // (CSV only)
                    AutodetectSeparators = new[] { ',', ';', '\t', '|', '#', '¤', '^' },

                    // Gets or sets a value indicating the number of rows to analyze for
                    // encoding, separator and field count in a CSV. When set, this option
                    // causes the IExcelDataReader.RowCount property to throw an exception.
                    // Default: 0 - analyzes the entire file (CSV only, has no effect on other formats)
                    AnalyzeInitialCsvRows = 0,

                    // Gets or sets a value indicating whether to leave the stream open after the IExcelDataReader object is disposed.
                    LeaveOpen = false
                };

                // XLS, XLSX or CSV/TSV/TXT ?
                var ext = Path.GetExtension(fileName).ToLower();

                using var reader = ext switch
                {
                    ".xls" or ".xlsx" or ".xlsb" => ExcelReaderFactory.CreateReader(stream, config),
                    ".csv" or ".tsv" or ".txt" => ExcelReaderFactory.CreateCsvReader(stream, config),
                    _ => throw new NotSupportedException($"Error: {ext} type/extension not supported!"),
                };

                // Get Dataset
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    UseColumnDataType = true,
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = useHeaderRow,
                        EmptyColumnNamePrefix = emptyColumnNamePrefix
                    }
                });

                // Get Table
                using var dataTable = ext switch
                {
                    ".xls" or ".xlsx" or ".xlsb" => ds.Tables[sheetName],
                    ".csv" or ".tsv" or ".txt" => ds.Tables[0],
                    _ => throw new NotSupportedException($"Error: {sheetName} not found!"),
                };

                // Transpose data's to typed object
                if (dataTable is not null)
                {
                    var columns = dataTable.Rows[0]
                        .Table
                        .Columns
                        .Cast<DataColumn>()
                        .Select(x => x.ColumnName);

                    var mapping = manualMapping?.AsEnumerable();

                    for (int i = (useHeaderRow ? 0 : 1); i < dataTable.Rows.Count; i++)
                    {
                        var datas = dataTable.Rows[i].ItemArray
                            .Select((v, m) => (v, m))
                            .ToDictionary(
                                x => (mapping ?? columns).ElementAt(x.m)
                                , x => (x.v == DBNull.Value ? null : x.v)
                             );

                        data.Add(datas);
                    }

                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
        }

        throw new ArgumentNullException(string.IsNullOrEmpty(_lastError) ? $"Could not find sheet '{sheetName}'." : _lastError);
    }

    private async Task<byte[]> DownloadFile(string address)
    {
        // Register codepages, see: https://github.com/ExcelDataReader/ExcelDataReader
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        switch (string.IsNullOrEmpty(address))
        {
            case false:
                try
                {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    using WebClient client = new();
#pragma warning restore SYSLIB0014 // Type or member is obsolete

                    return await client.DownloadDataTaskAsync(address);
                }
                catch (Exception ex)
                {
                    _lastError = ex.Message;
                }
                break;
        }

        return Array.Empty<byte>(); // <= avoids crash within ConvertSheetToJson
    }
}