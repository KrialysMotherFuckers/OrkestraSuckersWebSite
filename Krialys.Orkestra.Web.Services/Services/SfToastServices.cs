using System.Web;

namespace Krialys.Orkestra.Web.Module.Common.DI;

public interface ISfToastServices
{
    string Content { get; }
    string Css { get; }
    string Icon { get; }

    int ExtendedTimeOut { get; }
    bool ShowCloseButton { get; }
    int TimeOut { get; }
    string Title { get; }

    event SfToastServices.EventHandler OnChange;

    Task DisplaySuccessAsync(string title, string content, bool showCloseButton = false);
    Task DisplayInfoAsync(string title, string content, bool showCloseButton = false);
    Task DisplayWarningAsync(string title, string content, bool showCloseButton = true);
    Task DisplayErrorAsync(string title, string content, bool showCloseButton = true);
}

// ----------------------------------------------------------------------------------------------------------
// How to use it:
// 1 - Add this service as singleton into Startup.cs
// 2 - Place the component SfToastComponent in your main layout.
// 3 - Inject this service and call its methods wherever you need to invoke a toast.
//
// How it works:
// The component is the UI part, it displays the toast.
// The service is the method part, it configures and triggers the display.
// 1 - Initialization: The component register OnChange event.
// 2 - Usage: Display methods triggers Onchange event, resulting in the component displaying the toast.
// 3 - Dispose: The component unregister OnChange event.
// ----------------------------------------------------------------------------------------------------------

public sealed class SfToastServices : ISfToastServices
{
    #region Syncfusion Toast properties

    public string Title { get; private set; }

    public string Content { get; private set; }

    /* Predefined styles : e-toast-success, e-toast-info, e-toast-warning, e-toast-danger. */
    public string Css { get; private set; } = "e-toast-info";

    public string Icon { get; private set; } = "e-info toast-icons";

    public int TimeOut { get; } = 5000;

    /* Determines how long the toast should be displayed after a user hovers over it. 0 means no timeout. */
    public int ExtendedTimeOut { get; } = 750;

    /* Display a cross to close the toast. */
    public bool ShowCloseButton { get; private set; } = true;

    #endregion Syncfusion Toast properties

    public delegate Task EventHandler();

    public event EventHandler OnChange;

    private Task NotifyToastChanged() => OnChange?.Invoke();

    /// <summary>
    /// Display a positive toast.
    /// </summary>
    /// <remarks>
    /// Color green, displayed 5s.
    /// </remarks>
    public Task DisplaySuccessAsync(string title, string content, bool showCloseButton = false)
        => DisplayAsync(title, content, "e-toast-success", "e-success toast-icons", showCloseButton);

    /// <summary>
    /// Display an informative toast.
    /// </summary>
    /// <remarks>
    /// Color blue, displayed 5s.
    /// </remarks>
    public Task DisplayInfoAsync(string title, string content, bool showCloseButton = false)
        => DisplayAsync(title, content, "e-toast-info", "e-info toast-icons", showCloseButton);

    /// <summary>
    /// Display a toast with caution.
    /// </summary>
    /// <remarks>
    /// Color orange, displayed until the user closes it.
    /// </remarks>
    public Task DisplayWarningAsync(string title, string content, bool showCloseButton = true)
        => DisplayAsync(title, content, "e-toast-warning", "e-warning toast-icons", showCloseButton);

    /// <summary>
    /// Display a negative toast.
    /// </summary>
    /// <remarks>
    /// Color red, displayed until the user closes it.
    /// </remarks>
    public Task DisplayErrorAsync(string title, string content, bool showCloseButton = true)
        => DisplayAsync(title, content, "e-toast-danger", "e-error toast-icons", showCloseButton);

    private async Task DisplayAsync(string title, string content, string css, string icon, bool showCloseButton)
    {
        Css = css;
        Icon = icon;
        Title = HttpUtility.HtmlDecode(title);
        Content = HttpUtility.HtmlDecode(content);
        ShowCloseButton = showCloseButton;

        await Task.Delay(100);
        await NotifyToastChanged();
    }
}