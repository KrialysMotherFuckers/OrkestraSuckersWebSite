using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Krialys.Orkestra.Web.Module.Common.DI;

public interface ISfJsInteropServices
{
    int GetBrowserHeightMinusTop(string id);

    ValueTask DownloadFile(string filename, string contentType, byte[] content);
}

// This class provides JavaScript Interop functionality can be wrapped in a .NET class for easy consumption.
// The associated JavaScript module is loaded on demand when first needed.
// This class can be registered as scoped DI service and then injected into Blazor components for use.
// Applied advice from: https://blazor-university.com/javascript-interop/calling-dotnet-from-javascript/lifetimes-and-memory-leaks/
public class SfJsInteropServices : ISfJsInteropServices
{
    private IJSInProcessRuntime _jsInProcessRuntime;
    private readonly IServiceProvider _sp;

    public SfJsInteropServices(IServiceProvider sp)
        => _sp = sp;

    /// <summary>
    /// Browser specific functions
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int GetBrowserHeightMinusTop(string id)
    {
        int retValue = 0;

        using var scope = _sp.CreateScope();
        _jsInProcessRuntime = scope.ServiceProvider.GetRequiredService<IJSRuntime>() as IJSInProcessRuntime;

        // Get top position of the element
        int topHeight = (int)Math.Abs(_jsInProcessRuntime!.Invoke<float>(Litterals.JsGetElementPosition, id));

        if (0 == topHeight)
            return retValue;

        // Get browser height.
        int browserHeight = _jsInProcessRuntime.Invoke<int>(Litterals.JsGetBrowserHeight);
        retValue = browserHeight - topHeight > 0 ? browserHeight - topHeight : 0;

        return retValue;
    }

    /// <summary>
    /// Send the data to JS to actually download the file
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="contentType"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public ValueTask DownloadFile(string filename, string contentType, byte[] content)
        => _jsInProcessRuntime.InvokeVoidAsync(Litterals.JsDownloadFile, filename, contentType, content);
}