using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Components.Common;

public partial class MudNavGroupExtended : ComponentBase
{
    private bool _expanded;

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public string PicturePath { get; set; }

    /// <summary>
    /// If true, expands the nav group, otherwise collapse it.
    /// Two-way bindable
    /// </summary>
    [Parameter]
    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value)
                return;
            _expanded = value;
            ExpandedChanged.InvokeAsync(_expanded);
        }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void ExpandedToggle()
    {
        _expanded = !Expanded;
        ExpandedChanged.InvokeAsync(_expanded);
    }
}
