using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Action = Syncfusion.Blazor.Grids.Action;
using FailureEventArgs = Syncfusion.Blazor.Inputs.FailureEventArgs;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd.Components;

public partial class TER_ETAT_RESSOURCES_GridComponent
{
    #region Parameters
    /// <summary>
    /// Sf query applied to the grid.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    /// <summary>
    /// Id of selected TE_ETAT.
    /// </summary>
    [Parameter] public int EtatId { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    #region Properties
    /// <summary>
    /// List of TRS_RESSOURCE_SCENARIOS (read from DB).
    /// </summary>
    private IEnumerable<TRS_RESSOURCE_SCENARIOS> ResourceScenarios = Enumerable.Empty<TRS_RESSOURCE_SCENARIOS>();

    /// <summary>
    /// Data of the row where the command was launched.
    /// </summary>
    private TER_ETAT_RESSOURCES CommandData;

    /// <summary>
    /// Data of the selected row.
    /// </summary>
    private TER_ETAT_RESSOURCES SelectedRecord;
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        _proxyUrl = Config["ProxyUrl"];

        // Get link table between TER_ETAT_RESSOURCE and TS_SCENARIO from DB.
        ResourceScenarios = await ProxyCore.GetEnumerableAsync<TRS_RESSOURCE_SCENARIOS>(useCache: false);
    }
    #endregion

    #region Grid
    /// <summary>
    /// Reference to the grid component.
    /// </summary>
    private OrkaGenericGridComponent<TER_ETAT_RESSOURCES> Ref_TER_ETAT_RESSOURCES;

    /// <summary>
    /// List of custom displayed fields in grid columns.
    /// </summary>
    private readonly string[] CustomDisplayedFields = {
        nameof(TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID),
        nameof(TER_ETAT_RESSOURCES.TER_NOM_FICHIER),
        nameof(TER_ETAT_RESSOURCES.TER_IS_PATTERN),
        nameof(TER_ETAT_RESSOURCES.TER_PATH_RELATIF),
        "Modele",
        nameof(TER_ETAT_RESSOURCES.TER_MODELE_DATE),
        nameof(TER_ETAT_RESSOURCES.TER_MODELE_TAILLE),
        nameof(TER_ETAT_RESSOURCES.TER_COMMENTAIRE)
    };

    /// <summary>
    /// Table of hidden fields when adding.
    /// </summary>
    private readonly string[] AddingHiddenFields = {
        nameof(TER_ETAT_RESSOURCES.TER_MODELE_DATE),
        nameof(TER_ETAT_RESSOURCES.TER_MODELE_TAILLE)
    };

    /// <summary>
    /// Table of hidden fields when editing.
    /// </summary>
    private readonly string[] EditingHiddenFields = {
        nameof(TER_ETAT_RESSOURCES.TER_MODELE_DATE),
        nameof(TER_ETAT_RESSOURCES.TER_MODELE_TAILLE)
    };

    /// <summary>
    /// Get header for grid edit template.
    /// </summary>
    /// <param name="data">Edited item.</param>
    /// <returns>Edit header text.</returns>
    private string GetEditHeader(TER_ETAT_RESSOURCES data)
    {
        return data.TER_ETAT_RESSOURCEID == 0
            ? Trad.Keys["DTM:UTDCatalogResourcesGridAddHeader"]
            : $"{Trad.Keys["DTM:UTDCatalogResourcesGridEditHeader"]}{data.TER_NOM_FICHIER}";
    }

    /// <summary>
    /// Event triggers when DataGrid actions starts.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    private async Task OnActionBeginAsync(ActionEventArgs<TER_ETAT_RESSOURCES> args)
    {
        switch (args.RequestType)
        {
            case Action.BeginEdit:
                // Hide columns that can't be edited.
                await Ref_TER_ETAT_RESSOURCES.DataGrid.HideColumnsAsync(EditingHiddenFields, hideBy: "Field");
                break;

            case Action.Add:
                // Hide columns that can't be added.
                await Ref_TER_ETAT_RESSOURCES.DataGrid.HideColumnsAsync(AddingHiddenFields, hideBy: "Field");
                break;

            case Action.Delete:
                // Test if there is a model file to erase.
                if (StatusLiteral.Yes.Equals(SelectedRecord.TER_MODELE_DOC))
                {
                    await EraseModelFileAsync(SelectedRecord);
                }
                break;
        }

        // Before data adaptor call.
        if (args.RequestType == Action.Save)
        {
            // Set TE_ETAT id.
            args.Data.TE_ETATID = EtatId;
        }
    }

    public async Task OnActionCompleteAsync(ActionEventArgs<TER_ETAT_RESSOURCES> args)
    {
        switch (args.RequestType)
        {
            case Action.Save:
            case Action.Cancel:
                // Show again hidden columns.
                await Ref_TER_ETAT_RESSOURCES.DataGrid.ShowColumnsAsync(EditingHiddenFields, showBy: "Field");
                break;
        }
    }

    /// <summary>
    /// Event triggers every time a request is made to access row information, element, or data 
    /// and also before the row element is appended to the DataGrid element.
    /// </summary>
    /// <param name="Args">Row data bound argument.</param>
    private void OnRowDataBound(RowDataBoundEventArgs<TER_ETAT_RESSOURCES> Args)
    {
        // If there is no model.
        if (Args.Data.TER_MODELE_DOC != StatusLiteral.Yes)
        {
            // Hide "download model" command button.
            Args.Row.AddClass(new[] { "e-removecommand" });
        }
    }

    /// <summary>
    /// Event triggers when command button is clicked.
    /// </summary>
    /// <param name="args">Command click argument.</param>
    private void CommandClicked(CommandClickEventArgs<TER_ETAT_RESSOURCES> args)
    {
        // Data of the row where command is launched.
        CommandData = args.RowData;

        // Launch command.
        switch (args.CommandColumn.ID)
        {
            case "CommandModelDownload":
                DownloadModel();
                break;

            case "CommandModelUpload":
                OpenUploadModelDialog();
                break;

            case "CommandModelDelete":
                OpenEraseModelDialog();
                break;
        }
    }

    /// <summary>
    /// Event triggers when a row is selected.
    /// </summary>
    /// <param name="args">Row select event argument.</param>
    private void RowSelected(RowSelectEventArgs<TER_ETAT_RESSOURCES> args)
    {
        // Get selected row data.
        SelectedRecord = args.Data;
    }

    /// <summary>
    /// Event triggers when a selected row is deselected.
    /// </summary>
    /// <param name="args">Row deselect event argument.</param>
    private void RowDeselected(RowDeselectEventArgs<TER_ETAT_RESSOURCES> args)
    {
        // Clear selected row data.
        SelectedRecord = null;
    }
    #endregion

    #region Download Model command button
    /// <summary>
    /// Download model command.
    /// </summary>
    private void DownloadModel()
    {
        // Path of the file to download.
        string filePath = $"{{{ModelPath}}}{EtatId.ToString().PadLeft(6, '0')}/";
        // Name of the file to download.
        string fileName = CommandData.TER_ETAT_RESSOURCEID.ToString();
        // Download file.
        NavigationManager.NavigateTo($"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}" +
            $"/FILE/DownloadFile?fromPath={filePath}&fileName={fileName}&downloadFileName={CommandData.TER_NOM_MODELE}");
    }
    #endregion

    #region Erase Model command button
    /// <summary>
    /// Is the dialog used to erase the model displayed ?
    /// </summary>
    private bool IsEraseModelDialogDisplayed;

    /// <summary>
    /// Open dialog used to erase the model.
    /// </summary>
    private void OpenEraseModelDialog() => IsEraseModelDialogDisplayed = true;

    /// <summary>
    /// Close dialog used to erase the model.
    /// </summary>
    private void CloseEraseModelDialog() => IsEraseModelDialogDisplayed = false;

    /// <summary>
    /// Erase model command.
    /// </summary>
    /// <param name="etatRessource">TER_ETAT_RESSOURCES which contains model information.</param>
    /// <param name="showSuccess">Display success message to user.</param>
    /// <returns>True if success.</returns>
    private async Task<bool> EraseModelAsync(TER_ETAT_RESSOURCES etatRessource, bool showSuccess = false)
    {
        // Returned value.
        bool isEraseSuccessful = true;

        // Close dialog.
        CloseEraseModelDialog();

        // Erase model from resource.
        isEraseSuccessful &= await DeleteModelFromResourcesAsync(etatRessource);

        if (isEraseSuccessful)
        {
            // Delete model file on server.
            isEraseSuccessful &= await EraseModelFileAsync(etatRessource);
        }

        if (showSuccess && isEraseSuccessful)
        {
            await Toast.DisplaySuccessAsync($"{Trad.Keys["DTM:ModelDeleting"]}",
                string.Format(Trad.Keys["DTM:ModelDeleteSuccess"], etatRessource.TER_NOM_MODELE));
        }

        return isEraseSuccessful;
    }
    #endregion

    #region Upload Model command button
    /// <summary>
    /// Is the dialog used to upload the model displayed ?
    /// </summary>
    private bool IsUploadModelDialogDisplayed;

    private string _proxyUrl;

    /// <summary>
    /// Open dialog used to upload the model.
    /// </summary>
    private void OpenUploadModelDialog() => IsUploadModelDialogDisplayed = true;

    /// <summary>
    /// Close dialog used to upload the model.
    /// </summary>
    private void CloseUploadModelDialog() => IsUploadModelDialogDisplayed = false;

    /// <summary>
    /// Get the URL of save action (POST request) that will receive the upload files
    /// and handle the save operation in server.
    /// </summary>
    private string SaveUrl => $"{_proxyUrl}{Litterals.UniversRootPath}/FILE/UploadFileNew?" +
        $"fileNameId={CommandData.TER_ETAT_RESSOURCEID}&targetPath={ModelPath}" +
        $"&etatId={EtatId.ToString().PadLeft(6, '0')}";

    /// <summary>
    /// SfUploader: Event triggers on removing the uploaded file.
    /// </summary>
    /// <param name="args">Removing event argument.</param>
    private async Task OnRemoveAsync(RemovingEventArgs args)
    {
        if (!await EraseModelAsync(CommandData))
        {
            // Remove failed.
            args.Cancel = true;
        }
    }

    /// <summary>
    /// SfUploader: Event triggers if action is successful.
    /// </summary>
    private async Task SuccessAsync(SuccessEventArgs args)
    {
        // If upload was successful.
        if ("upload".Equals(args.Operation))
        {
            await AddModelToResourcesAsync(CommandData, args.File.Name, (int)args.File.Size);
        }
    }

    /// <summary>
    /// SfUploader: Event triggers if action failed.
    /// </summary>
    private async Task OnFailureAsync(FailureEventArgs args)
    {
        var logException = new LogException(GetType(), args.File?.Name,
            message: $"SfUploader {args.Operation} failed. Reponse code: {args.Response.StatusCode}, Reponse text: {args.Response.StatusText}");
        await ProxyCore.SetLogException(logException);

        // Display failure message.
        if ("upload".Equals(args.Operation))
        {
            await Toast.DisplayErrorAsync($"{Trad.Keys["DTM:ModelUploading"]}",
                string.Format(Trad.Keys["DTM:ModelUploadCancelled"],
                args.File!.Name));
        }
    }
    #endregion

    #region Model
    /// <summary>
    /// The path to the model will be read from ApiUnivers configuration file (appsettings.json).
    /// Here is the name of the parameter to read.
    /// </summary>
    private const string ModelPath = "ParallelU:PathRessourceModele";

    /// <summary>
    /// Delete model file on server.
    /// </summary>
    /// <param name="etatRessource">TER_ETAT_RESSOURCES which contains model information.</param>
    /// <returns>True if success.</returns>
    private async Task<bool> EraseModelFileAsync(TER_ETAT_RESSOURCES etatRessource)
    {
        // Returned value.
        bool isEraseSuccessful = true;

        string errorMessage = string.Empty;

        // Path of the file to remove.
        string filePath = $"{{{ModelPath}}}{EtatId.ToString().PadLeft(6, '0')}/";
        // Name of the file to remove.
        string fileName = etatRessource.TER_ETAT_RESSOURCEID.ToString();
        try
        {
            // Delete file.
            var deleteResult = await ProxyCore
                .DeleteFiles(filePath, new[] { fileName });

            // Delete result.
            if (deleteResult.Key != "OK")
            {
                isEraseSuccessful = false;
                errorMessage = deleteResult.Value;

                await ProxyCore.SetLogException(new LogException(GetType(), fileName, errorMessage));
            }
        }
        // Delete failed with error.
        catch (Exception ex)
        {
            isEraseSuccessful = false;
            errorMessage = ex.Message;

            // Log error
            await ProxyCore.SetLogException(new LogException(GetType(), ex));
        }

        if (!isEraseSuccessful)
        {
            // Display error.
            await Toast.DisplayErrorAsync($"{Trad.Keys["DTM:ModelDeleting"]}",
                $"{string.Format(Trad.Keys["DTM:ModelDeleteFailed"], etatRessource.TER_NOM_MODELE)}<br />{errorMessage}")
                ;
        }

        return isEraseSuccessful;
    }

    /// <summary>
    /// Add a model to TER_ETAT_RESSOURCES.
    /// </summary>
    /// <param name="etatRessource">Updated TER_ETAT_RESSOURCES.</param>
    /// <param name="modelName">Model name.</param>
    /// <param name="modelSize">Model size in bytes.</param>
    private async Task AddModelToResourcesAsync(TER_ETAT_RESSOURCES etatRessource, string modelName, int modelSize)
    {
        try
        {
            // Update resource with new model data.
            etatRessource.TER_NOM_MODELE = modelName;
            etatRessource.TER_MODELE_DATE = DateExtensions.GetLocaleNow();
            etatRessource.TER_MODELE_TAILLE = modelSize;
            etatRessource.TER_MODELE_DOC = StatusLiteral.Yes;
            await Ref_TER_ETAT_RESSOURCES.DataGrid.DataManager.UpdateAsync<TER_ETAT_RESSOURCES>(
                nameof(TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID), etatRessource);
        }
        catch (Exception ex)
        {
            // Log error.
            await ProxyCore.SetLogException(new LogException(GetType(), ex));

            // Display error.
            await Toast.DisplayErrorAsync($"{Trad.Keys["DTF:ModelUploading"]}",
                string.Format(Trad.Keys["DTF:ResourceUpdateFailed"]));
        }

        // Refresh grid data (to see the upload).
        await Ref_TER_ETAT_RESSOURCES.DataGrid.Refresh();
    }

    /// <summary>
    /// Delete a model from TER_ETAT_RESSOURCES.
    /// </summary>
    /// <param name="etatRessource">Updated TER_ETAT_RESSOURCES.</param>
    /// <returns>True if success.</returns>
    private async Task<bool> DeleteModelFromResourcesAsync(TER_ETAT_RESSOURCES etatRessource)
    {
        // Returned value.
        bool isDeleteSuccessful = true;

        try
        {
            // Erase model from resource.
            etatRessource.TER_NOM_MODELE = null;
            etatRessource.TER_MODELE_DATE = null;
            etatRessource.TER_MODELE_TAILLE = null;
            etatRessource.TER_MODELE_DOC = StatusLiteral.No;
            await Ref_TER_ETAT_RESSOURCES.DataGrid.DataManager
                .UpdateAsync<TER_ETAT_RESSOURCES>(nameof(TER_ETAT_RESSOURCES.TER_ETAT_RESSOURCEID), etatRessource);
        }
        catch (Exception ex)
        {
            isDeleteSuccessful = false;

            // Log error.
            await ProxyCore.SetLogException(new LogException(GetType(), ex));

            // Display error.
            await Toast.DisplayErrorAsync($"{Trad.Keys["DTF:FileDeleting"]}",
                string.Format(Trad.Keys["DTF:ResourceUpdateFailed"]));
        }

        // Refresh grid data (to see the delete).
        await Ref_TER_ETAT_RESSOURCES.DataGrid.Refresh();

        return isDeleteSuccessful;
    }
    #endregion
}
