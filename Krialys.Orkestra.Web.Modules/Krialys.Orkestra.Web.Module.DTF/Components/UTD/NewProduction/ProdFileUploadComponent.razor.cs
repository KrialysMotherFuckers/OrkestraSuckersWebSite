using Krialys.Common.Literals;
using Krialys.Common.Validations;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;

namespace Krialys.Orkestra.Web.Module.DTF.Components.UTD.NewProduction;

public partial class ProdFileUploadComponent
{
    #region Parameters
    /// <summary>
    /// Id of created TD_DEMANDES.
    /// </summary>
    [Parameter]
    public int DemandeId { get; set; }

    /// <summary>
    /// Id of selected TS_SCENARIOS.
    /// </summary>
    [Parameter]
    public int? ScenarioId { get; set; }
    #endregion

    #region Public methods
    /// <summary>
    /// Checks that all required files are uploaded.
    /// </summary>
    /// <returns>True, if all required files are uploaded, false otherwise.</returns>
    public async Task<bool> AreRequiredFilesUploadedAsync()
    {
        // Returned value.
        // Default value is true for the case where no files are required.
        bool areRequiredFilesUploaded = true;

        // Get TER_ETAT_RESSOURCE from DB,
        // expanded with TRS_RESSOURCE_SCENARIOS, filtered by selected scenario id and where a file is required,
        // and expanded with TRD_RESSOURCE_DEMANDES, filtered by created demande id,
        // filtered to display only resources associated to the selected scenario and that have a required file.
        string queryOptions = $"?$expand=TRS_RESSOURCE_SCENARIOS($filter=TS_SCENARIOID eq {ScenarioId} and TRS_FICHIER_OBLIGATOIRE eq 'O')" +
            $"&$expand=TRD_RESSOURCE_DEMANDES($filter=TD_DEMANDEID eq {DemandeId})" +
            $"&$filter=TRS_RESSOURCE_SCENARIOS/any(TRS: TRS/TS_SCENARIOID eq {ScenarioId} and TRS/TRS_FICHIER_OBLIGATOIRE eq 'O')";

        var etatRessourcesWithRequiredFile = await ProxyCore.GetEnumerableAsync<TER_ETAT_RESSOURCES>(queryOptions, useCache: false);

        // For each required file.
        foreach (var etatRessource in etatRessourcesWithRequiredFile)
        {
            // Checks if at least one file was uploaded.
            areRequiredFilesUploaded &= etatRessource.TRD_RESSOURCE_DEMANDES.Any(rd => StatusLiteral.Yes.Equals(rd.TRD_FICHIER_PRESENT));

            // If one file is missing.
            if (!areRequiredFilesUploaded)
            {
                await Toast.DisplayErrorAsync($"{Trad.Keys["DTF:FileControl"]}",
                    string.Format(Trad.Keys["DTF:FileControlFailed"], etatRessource.TER_NOM_FICHIER))
                    ;

                // Stop control.
                break;
            }
        }

        return areRequiredFilesUploaded;
    }
    #endregion

    #region Files data
    /// <summary>
    /// List of displayed TER_ETAT_RESSOURCE.
    /// </summary>
    private IEnumerable<TER_ETAT_RESSOURCES> _etatRessources = Enumerable.Empty<TER_ETAT_RESSOURCES>();

    /// <summary>
    /// Get data to display from database.
    /// </summary>
    private async Task LoadFilesDataAsync()
    {
        if (!ScenarioId.Equals(default) && !DemandeId.Equals(default))
        {
            // Get TER_ETAT_RESSOURCE from DB,
            // expanded with TRS_RESSOURCE_SCENARIOS, filtered by selected scenario id
            // and filtered to display only resources associated to the selected scenario.
            string queryOptions = $"?$expand=TRS_RESSOURCE_SCENARIOS($filter=TS_SCENARIOID eq {ScenarioId})" +
                $"&$filter=TRS_RESSOURCE_SCENARIOS/any(TRS: TRS/TS_SCENARIOID eq {ScenarioId})";

            _etatRessources = await ProxyCore.GetEnumerableAsync<TER_ETAT_RESSOURCES>(queryOptions, useCache: false);
        }
    }

    /// <summary>
    /// Get CSS classes to apply on file name.
    /// </summary>
    /// <param name="etatRessource">Description of the file to upload.</param>
    /// <returns>Css classes.</returns>
    private string GetFileNameClass(TER_ETAT_RESSOURCES etatRessource)
        => StatusLiteral.Yes.Equals(etatRessource.TRS_RESSOURCE_SCENARIOS.FirstOrDefault()?.TRS_FICHIER_OBLIGATOIRE)
            ? "filename mandatory" : "filename";
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component has received the parameter 
    /// from parent component.
    /// </summary>
    protected override Task OnParametersSetAsync()
    {
        // Set URL of save action.
        _saveUrl = $"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}/FILE/UploadFileNew?" +
            $"targetPath={TargetPath}&etatId={DemandeId.ToString().PadLeft(6, '0')}";

        // Set URL of remove action.
        _removeUrl = $"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}/FILE/RemoveRawFile?" +
            $"basePath={TargetPath}&filePath={DemandeId.ToString().PadLeft(6, '0')}";

        // Get displayed data.
        return LoadFilesDataAsync();
    }
    #endregion

    #region Model download
    /// <summary>
    /// The path to the model will be read from ApiUnivers configuration file (appsettings.json).
    /// Here is the name of the parameter to read.
    /// </summary>
    private const string ModelPath = "ParallelU:PathRessourceModele";

    /// <summary>
    /// Download model.
    /// </summary>
    /// <param name="etatRessource">Description of the file to upload.</param>
    private void DownloadModel(TER_ETAT_RESSOURCES etatRessource)
    {
        // Path of the file to download.
        string filePath = $"{{{ModelPath}}}{etatRessource.TE_ETATID.ToString().PadLeft(6, '0')}/";
        // Name of the file to download.
        string fileName = etatRessource.TER_ETAT_RESSOURCEID.ToString();
        // Download file.
        NavigationManager.NavigateTo($"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}" +
            $"/FILE/DownloadFile?fromPath={filePath}&fileName={fileName}&downloadFileName={etatRessource.TER_NOM_MODELE}");
    }

    /// <summary>
    /// Is the button to download the model disabled?
    /// </summary>
    /// <param name="etatRessource">Description of the file to upload.</param>
    /// <returns>True if the button is disabled, false otherwise.</returns>
    private bool DownloadModelDisabled(TER_ETAT_RESSOURCES etatRessource)
        => !StatusLiteral.Yes.Equals(etatRessource.TER_MODELE_DOC);
    #endregion

    #region Uploader
    /// <summary>
    /// Path where the file will be uploaded on the server.
    /// Corresponds to a parameter from ApiUnivers configuration file (appsettings.json).
    /// </summary>
    private const string TargetPath = "ParallelU:PathRessource";

    /// <summary>
    /// Get the URL of save action (POST request) that will receive the upload files
    /// and handle the save operation in server.
    /// </summary>
    /// <note>File path on server: 
    /// {ParallelU:PathRessource}/{TD_DEMANDEID padded on 6 with 0 (ex 000005)}/{TRD_NOM_FICHIER}
    /// </note>
    private string _saveUrl;

    /// <summary>
    /// Get the URL of remove action (POST request) that receives the file information 
    /// and handle the remove operation in server.
    /// </summary>
    private string _removeUrl;

    /// <summary>
    /// Is it allowed to download multiple files?
    /// </summary>
    /// <param name="etatRessource">Description of the file to upload.</param>
    /// <returns>True if it is allowed, false otherwise.</returns>
    private bool AllowMultipleFiles(TER_ETAT_RESSOURCES etatRessource)
        // If file is a mask (contain wildcards), multiple files download is allowed.
        => StatusLiteral.Yes.Equals(etatRessource.TER_IS_PATTERN);

    /// <summary>
    /// Dictionary of the names of uploaded files before being renamed.
    /// The id is generated by SfUploader component.
    /// </summary>
    private Dictionary<string, string> _originalFilesNames = new();

    /// <summary>
    /// Event triggers after selecting or dropping the files.
    /// </summary>
    /// <param name="args">Uploader selection event arguments.</param>
    /// <param name="etatRessource">Description of the file to upload.</param>
    private async Task FileSelectedHandlerAsync(SelectedEventArgs args, TER_ETAT_RESSOURCES etatRessource)
    {
        // Browse through selected files.
        foreach (var file in args.FilesData)
        {
            try
            {
                // Store original file name (because it can possibly be renamed).
                _originalFilesNames.Add(file.Id, file.Name);
            }
            catch (Exception ex)
            {
                await ProxyCore.SetLogException(new LogException(GetType(), _originalFilesNames, ex.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["DTF:FileUploading"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

                // Cancel file upload.
                args.Cancel = true;
            }
        }

        // If file is not a mask.
        if (StatusLiteral.No.Equals(etatRessource.TER_IS_PATTERN))
        {
            // File is renamed to match TER_ETAT_RESSOURCES file name.
            args.FilesData[0].Name = etatRessource.TER_NOM_FICHIER;
            // Specifies the uploader to use modified data.
            args.IsModified = true;
            args.ModifiedFilesData = args.FilesData;
        }
        else
        {
            // Browse throught selected files.
            foreach (var file in args.FilesData)
            {
                // Control if file matches the mask.
                if (!ControlFileName.MatchesMask(file.Name, etatRessource.TER_NOM_FICHIER))
                {
                    await Toast.DisplayErrorAsync($"{Trad.Keys["DTF:FileUploading"]}",
                        string.Format(Trad.Keys["DTF:FileMaskIncorrect"],
                        file.Name));

                    // Cancel file upload.
                    args.Cancel = true;
                }
            }
        }
    }

    /// <summary>
    /// Event triggers when the uploader gets success on uploading or removing files.
    /// </summary>
    /// <param name="args">Uploader success event arguments.</param>
    /// <param name="etatRessource">Description of the file to upload.</param>
    private async Task UploadFileSuccessHandlerAsync(SuccessEventArgs args, TER_ETAT_RESSOURCES etatRessource)
    {
        // If upload was successful.
        if ("upload".Equals(args.Operation))
        {
            bool success = true;

            // If only one file can be loaded.
            if (!AllowMultipleFiles(etatRessource))
            {
                success |= await UnBindRessourceFromDemandesAsync(etatRessource,
                    removedFileName: args.File.Name);
            }

            // Get name of the file selected by the user.
            if (_originalFilesNames.TryGetValue(args.File.Id, out string originalFileName))
            {
                // We are done with the file name and we can delete it from dictionary.
                _originalFilesNames.Remove(args.File.Id);

                // Bind uploaded file (resource) to a demand.
                success = await BindRessourceToDemandesAsync(etatRessource,
                    originalFileName, uploadedFileSize: (int)args.File.Size);

                if (success)
                {
                    // Display success message.
                    await Toast.DisplaySuccessAsync($"{Trad.Keys["DTF:FileUploading"]}",
                                    string.Format(Trad.Keys["DTF:FileUploadSuccess"],
                                    args.File.Name));
                }
            }
            else
            {
                await ProxyCore.SetLogException(new LogException(GetType(), _originalFilesNames, $"An item with the key {args.File.Id} was not found."));
                await Toast.DisplayErrorAsync(Trad.Keys["DTF:FileUploading"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
            }
        }
        // If remove was successful.
        else if ("remove".Equals(args.Operation))
        {
            // Unbind removed file (resource) from demandes.
            bool success = await UnBindRessourceFromDemandesAsync(etatRessource,
                removedFileName: args.File.Name);

            if (success)
            {
                // Display success message.
                await Toast.DisplaySuccessAsync($"{Trad.Keys["DTF:FileDeleting"]}",
                string.Format(Trad.Keys["DTF:FileDeleteSuccess"], args.File.Name))
                    ;
            }
        }
    }

    /// <summary>
    /// Event triggers when uploader fails on uploading or removing files. 
    /// </summary>
    /// <param name="args">Uploader failure event arguments.</param>
    private async Task OnUploadFileFailureAsync(FailureEventArgs args)
    {
        // Display failure message.
        if ("upload".Equals(args.Operation))
        {
            await Toast.DisplayErrorAsync($"{Trad.Keys["DTF:FileUploading"]}",
                string.Format(Trad.Keys["DTF:FileUploadFailed"],
                args.File.Name));
        }
        else if ("remove".Equals(args.Operation))
        {
            await Toast.DisplayErrorAsync($"{Trad.Keys["DTF:FileDeleting"]}",
                string.Format(Trad.Keys["DTF:FileDeleteFailed"],
                args.File.Name));
        }
    }

    /// <summary>
    /// Event triggers when the uploaded file is removed. 
    /// </summary>
    /// <param name="args">Before remove event arguments.</param>
    private void UploaderBeforeRemove(BeforeRemoveEventArgs args)
        // If PostRawFile is set to false, it will post only the file name to the remove action
        // instead of the file representation (IFormFile object).
        => args.PostRawFile = false;
    #endregion

    #region Update TRD_RESSOURCE_DEMANDES table
    /// <summary>
    /// Add TRD_RESSOURCE_DEMANDES entry for the selected resource.
    /// </summary>
    /// <param name="etatRessource">Resource to bind.</param>
    /// <param name="originalFileName">Name of the file selected by the user.</param>
    /// <param name="uploadedFileSize">Size of the uploaded file.</param>
    /// <returns>True on success.</returns>
    /// <remarks>Demand id is a component parameter.</remarks>
    private async Task<bool> BindRessourceToDemandesAsync(TER_ETAT_RESSOURCES etatRessource,
        string originalFileName, int uploadedFileSize)
    {
        // Create a new TRD_RESSOURCE_DEMANDES.
        TRD_RESSOURCE_DEMANDES ressourceDemande = new()
        {
            TE_ETATID = etatRessource.TE_ETATID,
            TD_DEMANDEID = DemandeId,
            TER_ETAT_RESSOURCEID = etatRessource.TER_ETAT_RESSOURCEID,

            TRD_FICHIER_PRESENT = StatusLiteral.Yes,
            // Name of the file saved on server.
            TRD_NOM_FICHIER = etatRessource.TER_NOM_FICHIER,
            // File size.
            TRD_TAILLE_FICHIER = uploadedFileSize,
            // Name of the file selected by the user.
            TRD_NOM_FICHIER_ORIGINAL = originalFileName
        };

        var apiResult = await ProxyCore.InsertAsync(new List<TRD_RESSOURCE_DEMANDES> { ressourceDemande });

        // If the insertion failed.
        if (apiResult.Count < 1)
        {
            await ProxyCore.SetLogException(new LogException(GetType(), ressourceDemande, apiResult.Message));
            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

            return false;
        }

        // Success.
        return true;
    }

    /// <summary>
    /// Delete TRD_RESSOURCE_DEMANDES entry for the selected resource.
    /// </summary>
    /// <param name="etatRessource">Resource to unbind.</param>
    /// <param name="removedFileName">Name of the removed file.</param>
    /// <returns>True on success.</returns>
    /// <remarks>Demand id is a component parameter.</remarks>
    private async Task<bool> UnBindRessourceFromDemandesAsync(TER_ETAT_RESSOURCES etatRessource,
        string removedFileName)
    {
        // Field from TRD_RESSOURCE_DEMANDES containing the uploaded file name.
        string fileNameField = etatRessource.TER_IS_PATTERN == StatusLiteral.Yes ? "TRD_NOM_FICHIER_ORIGINAL" : "TRD_NOM_FICHIER";

        // Get link between the uploaded file (ressource) and the demand.
        string queryOptions = $"?$filter=(TD_DEMANDEID eq {DemandeId}) " +
            $"and (TER_ETAT_RESSOURCEID eq {etatRessource.TER_ETAT_RESSOURCEID}) " +
            $"and ({fileNameField} eq '{removedFileName}')";

        var ressourceDemande = (await ProxyCore.GetEnumerableAsync<TRD_RESSOURCE_DEMANDES>(queryOptions, useCache: false)).FirstOrDefault();

        if (ressourceDemande is not null)
        {
            int ressourceDemandeId = ressourceDemande.TRD_RESSOURCE_DEMANDEID;
            // Delete link between the uploaded file and the demand.
            var apiResult = await ProxyCore.DeleteAsync<TRD_RESSOURCE_DEMANDES>(new List<string> { ressourceDemandeId.ToString() });

            // Api delete failed.
            if (apiResult.Count < 1)
            {
                await ProxyCore.SetLogException(new LogException(GetType(), ressourceDemande, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

                return false;
            }
        }

        // Success.
        return true;
    }
    #endregion
}
