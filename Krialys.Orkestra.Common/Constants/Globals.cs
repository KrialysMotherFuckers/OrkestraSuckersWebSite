using System.Reflection;
using System.Text;

namespace Krialys.Orkestra.Common.Constants;

/// <summary>All globals/default parameters.</summary>
public static class Globals
{
    /// <summary>Number of rows that each OData controller can emit at once.</summary>
    public const int MaxTop = 1000;
    /// <summary>Default number of items by page.</summary>
    public const int PageSize = 20;

    public static string SfLicenseKey
        //=> "Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5ad01jWnpbcXJSRGVV;MjcwNTM0MUAzMjMzMmUzMDJlMzBnam15dUN1N0M4SmMzZHVOdkhxTG16VFRMdzNlL3EvdjBITnZRMHlmeDIwPQ==";
        => "Mgo+DSMBMAY9C3t2UVhhQlVFfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9Rd01iXX9Xc3JUR2hc;Mjk4NDU1MUAzMjM0MmUzMDJlMzBiUEl2VXdVQXBSVjFEQ2ZlNlZwTXhnZ05abU9hRVhjeUZNNnpDTHoySTA0PQ==";

    /// <summary>Gets the assembly path that contains the code that is currently executing.</summary>
    public static string AssemblyDirectory
    {
        get
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

        return Convert.ToBase64String(plainTextBytes);
    }

    /// <summary>
    /// Get the list of granted executables (white list)
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> FilterAllowedExecutables(string[] exeFiles)
    {
        foreach (var exeFile in exeFiles)
        {
            if (new[] { "synchro_ldap.exe", "balance.exe", "ginkgo.exe" }
                .All(e => !e.Equals(new FileInfo(exeFile).Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                yield return exeFile;
            }
        }
    }
}