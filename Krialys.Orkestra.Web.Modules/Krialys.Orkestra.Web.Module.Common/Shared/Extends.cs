using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.Common.Shared;

public static class Extends
{
    private static DialogSettings _gridEditSettingDialog;

    // Configuration of the edit dialog
    public static DialogSettings GridEditSettingDialog
    {
        get
        {
            return _gridEditSettingDialog ??= new DialogSettings
            {
                Width = "480px",
                ShowCloseIcon = true,
                CloseOnEscape = true,
                AllowDragging = false,
                XValue = "center",
                AnimationEffect = Syncfusion.Blazor.Popups.DialogEffect.None,
                AnimationDelay = 0,
                AnimationDuration = 0,
            };
        }
    }
}