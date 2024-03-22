using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Shared;

public partial class OrderDocuments_DialogComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Selected order.
    /// </summary>
    [Parameter]
    public TCMD_COMMANDES Order { get; set; }
    #endregion

    #region Dialog
    /// <summary>
    /// Close the dialog.
    /// </summary>
    private Task CloseDialogAsync()
    {
        // Close catalog.
        IsVisible = false;

        // Update parent with new value.
        return IsVisibleChanged.InvokeAsync(IsVisible);
    }
    #endregion

    #region Download
    /// <summary>
    /// Get URL of the document to download.
    /// </summary>
    /// <param name="fileName">Name of the downloaded file.</param>
    /// <returns>URL of the document to download.</returns>
    private string GetDownloadDocumentUrl(string fileName)
        => $"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}" +
            $"/FILE/DownloadFile?fromPath={{ParallelU:PathCommande}}" +
            $"&fileName={Order?.TCMD_COMMANDEID}/{fileName}&downloadFileName={fileName}";
    #endregion
}
