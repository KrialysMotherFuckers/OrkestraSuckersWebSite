using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components.SfToast;

public partial class SfToastComponent : IDisposable
{
    /* Toast object. */
    private Syncfusion.Blazor.Notifications.SfToast ToastObj { get; set; }

    /* Toast position. */
    /// <summary>
    /// X position (Left, Center or Right)
    /// </summary>
    [Parameter] public string ToastXPosition { get; set; } = "Right";
    /// <summary>
    /// Y position (Top or Bottom)
    /// </summary>
    [Parameter] public string ToastYPosition { get; set; } = "Top";

    /* Toast properties. */
    private string ToastTitle;
    private MarkupString ToastContent;
    private string ToastCssClass;
    private string ToastIconClass;
    private int ToastTimeOut;
    private bool ToastShowCloseButton;

    protected override Task OnInitializedAsync()
    {
        /* Register on "OnToastChange" event and handle it with "ShowToast" method. */
        Toast.OnChange += ShowToast;

        return base.OnInitializedAsync();
    }

    /* Display required toast. */
    private async Task ShowToast()
    {
        if (!string.IsNullOrEmpty(Toast.Content))
        {
            /* Update toast. */
            ToastTitle = Toast.Title;
            ToastContent = (MarkupString)Toast.Content;
            ToastCssClass = Toast.Css;
            ToastIconClass = Toast.Icon;
            ToastTimeOut = Toast.ShowCloseButton ? 120_000 : Toast.TimeOut;
            ToastShowCloseButton = Toast.ShowCloseButton;

            /* Update component state to take new toast properties. */
            await InvokeAsync(StateHasChanged);

            /* Delay mandatory to update the dynamically changed Toast properties.
             * Please read https://blazor.syncfusion.com/documentation/toast/how-to/change-toast-content-dynamically/ */
            await Task.Delay(250);
            await ToastObj.ShowAsync();
        }
    }

    public void Dispose()
    {
        Toast.OnChange -= ShowToast;
    }
}
