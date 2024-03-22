using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Krialys.Orkestra.Web.Module.Common.DI;

public interface IDownloadServices
{
    void DownloadQualif(int demandeId);
    void DownloadResult(int demandeId);
}

/// <summary>
/// Service to download data from ApiUnivers.
/// </summary>
public class DownloadServices : IDownloadServices
{
    #region Constructor
    private readonly IConfiguration _config;

    private readonly NavigationManager _navigationManager;

    public DownloadServices(IConfiguration config, NavigationManager navigationManager)
    {
        _config = config;
        _navigationManager = navigationManager;
    }
    #endregion

    #region Download TD_DEMANDES qualifs
    /// <summary>
    /// The path to the file will be read from ApiUnivers configuration file (appsettings.json).
    /// Here is the name of the parameter to read.
    /// </summary>
    private const string QualifPath = "ParallelU:PathQualif";

    /// <summary>
    /// Download TD_DEMANDES qualification data (zip folder) .
    /// </summary>
    /// <param name="demandeId">Id of TD_DEMANDES.</param>
    public void DownloadQualif(int demandeId)
    {
        string demande = demandeId.ToString().PadLeft(6, '0');

        // Path of the file to download.
        string filePath = $"{{{QualifPath}}}";

        // Name of the file to download.
        string fileName = $"{demande}_QUALIF.zip";

        // Download file
        _navigationManager.NavigateTo($"{_config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}" +
            $"/FILE/DownloadFile?fromPath={filePath}&fileName={fileName}&downloadFileName={fileName}");
    }
    #endregion

    #region Download TD_DEMANDES results
    /// <summary>
    /// The path to the file will be read from ApiUnivers configuration file (appsettings.json).
    /// Here is the name of the parameter to read.
    /// </summary>
    private const string ResultPath = "ParallelU:PathResult";

    /// <summary>
    /// Download TD_DEMANDES results (zip folder).
    /// </summary>
    /// <param name="demandeId">Id of TD_DEMANDES.</param>
    public void DownloadResult(int demandeId)
    {
        // Path of the file to download.
        string filePath = $"{{{ResultPath}}}/";

        // Name of the file to download.
        string fileName = $"{demandeId.ToString().PadLeft(6, '0')}_RESULT.zip";

        // Download file.
        string target = $"{_config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}/FILE/DownloadFile?fromPath={filePath}&fileName={fileName}";

        _navigationManager.NavigateTo(target);
    }
    #endregion
}
