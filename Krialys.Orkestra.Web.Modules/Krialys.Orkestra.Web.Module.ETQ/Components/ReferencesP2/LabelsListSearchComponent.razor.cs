using Krialys.Common.Extensions;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Shared;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using SortOrder = Syncfusion.Blazor.DropDowns.SortOrder;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.ReferencesP2;

public partial class LabelsListSearchComponent
{
    #region Parameters
    /// <summary>
    /// Number of displayed labels (depending on applied filters and searches).
    /// </summary>
    [Parameter]
    public int LabelsCount { get; set; }

    /// <summary>
    /// Event triggers when filters change.
    /// Return new filters to apply.
    /// </summary>
    [Parameter]
    public EventCallback<EtqFilters> OnFiltersChanged { get; set; }
    #endregion

    #region Filters
    /// <summary>
    /// Filters to apply on labels.
    /// </summary>
    private EtqFilters _filters = new();
    #endregion

    #region Search TextBox
    /// <summary>
    /// Event triggered when the content of TextBox has changed (enter pressed or focus-out).
    /// </summary>
    /// <param name="args">Changed event arguments.</param>
    private Task SearchValueChangeAsync(ChangedEventArgs args)
        // Trigger event callback.
        => OnFiltersChanged.InvokeAsync(_filters);
    #endregion

    #region Creation date Range Picker
    /// <summary>
    /// Event triggered when the selected range is changed.
    /// </summary>
    /// <param name="args">Range picker event arguments.</param>
    private Task CreationDateRangeChangeAsync(RangePickerEventArgs<DateTime?> args)
    {
        // Update filters.
        if (args.StartDate.HasValue)
            _filters.CreationDateMin = args.StartDate.Value.ToUniversalTime();
        else
            _filters.CreationDateMin = null;
        if (args.EndDate.HasValue)
            _filters.CreationDateMax = args.EndDate.Value.AddDays(1).ToUniversalTime();
        else
            _filters.CreationDateMax = null;

        // Trigger event callback.
        return OnFiltersChanged.InvokeAsync(_filters);
    }

    /// <summary>
    /// This week preselect start date.
    /// </summary>
    private readonly DateTime _weekStart = DateTime.Now.GetFirstDayOfWeek();

    /// <summary>
    /// This week preselect end date.
    /// </summary>
    private readonly DateTime _weekEnd = DateTime.Now.GetLastDayOfWeek();

    /// <summary>
    /// This month preselect start date.
    /// </summary>
    private readonly DateTime _monthStart = DateTime.Now.GetFirstDayOfMonth();

    /// <summary>
    /// This month preselect end date.
    /// </summary>
    private readonly DateTime _monthEnd = DateTime.Now.GetLastDayOfMonth();

    /// <summary>
    /// Last month preselect start date.
    /// </summary>
    private readonly DateTime _lastMonthStart = DateTime.Now.GetFirstDayOfPreviousMonth();

    /// <summary>
    /// Last month preselect end date.
    /// </summary>
    private readonly DateTime _lastMonthEnd = DateTime.Now.GetLastDayOfPreviousMonth();
    #endregion

    #region Module MultiSelect
    /// <summary>
    /// Selected modules.
    /// </summary>
    private TS_SCENARIOS[] _selectedModules;

    /// <summary>
    /// OData query applied to the MultiSelect.
    /// </summary>
    private const string _moduleODataQuery = $"?$expand={nameof(TS_SCENARIOS.TE_ETAT)}";

    /// <summary>
    /// Sf query used to filter and order MultiSelect.
    /// </summary>
    private readonly Query _moduleQuery = new Query()
        .Sort(nameof(TS_SCENARIOS.TS_NOM_SCENARIO), SortOrder.Ascending.ToString())
        .Sort($"{nameof(TS_SCENARIOS.TE_ETAT)}.{nameof(TE_ETATS.TE_FULLNAME)}", SortOrder.Ascending.ToString())
        .AddParams(Litterals.OdataQueryParameters, _moduleODataQuery)
        .Take(20);

    /// <summary>
    /// Event triggered when the MultiSelect value changed.
    /// </summary>
    /// <param name="args">MultiSelectChange event arguments.</param>
    private Task ModuleChangeAsync(MultiSelectChangeEventArgs<TS_SCENARIOS[]> args)
    {
        // Update filters.
        _filters.ModuleIds = args.Value?.Select(s => s.TS_SCENARIOID).ToArray();

        // Trigger event callback.
        return OnFiltersChanged.InvokeAsync(_filters);
    }
    #endregion

    #region Order number Numeric TextBox
    /// <summary>
    /// Event triggered when the value changes or the component loses focus.
    /// </summary>
    /// <param name="args">Change avant arguments.</param>
    private Task OrderNumberChangeAsync(Syncfusion.Blazor.Inputs.ChangeEventArgs<int?> args)
        // Trigger event callback.
        => OnFiltersChanged.InvokeAsync(_filters);
    #endregion

    #region Rule value MultiSelect
    /// <summary>
    /// Selected rule values.
    /// </summary>
    private TRGLRV_REGLES_VALEURS[] _selectedRuleValues;

    /// <summary>
    /// OData query applied to the MultiSelect.
    /// </summary>
    private const string _ruleValueODataQuery = $"?$expand={nameof(TRGLRV_REGLES_VALEURS.TRGL_REGLE)}";

    /// <summary>
    /// Sf query used to filter and order MultiSelect.
    /// </summary>
    private readonly Query _ruleValueQuery = new Query()
        .Sort(nameof(TRGLRV_REGLES_VALEURS.TRGLRV_VALEUR), SortOrder.Ascending.ToString())
        .Sort($"{nameof(TRGLRV_REGLES_VALEURS.TRGL_REGLE)}.{nameof(TRGL_REGLES.TRGL_LIB_REGLE)}", SortOrder.Ascending.ToString())
        .AddParams(Litterals.OdataQueryParameters, _ruleValueODataQuery)
        .Take(20);

    /// <summary>
    /// Event triggered when the MultiSelect value changed.
    /// </summary>
    /// <param name="args">MultiSelectChange event arguments.</param>
    private Task RuleValueChangeAsync(MultiSelectChangeEventArgs<TRGLRV_REGLES_VALEURS[]> args)
    {
        // Update filters.
        _filters.RuleValueIds = args.Value?.Select(rv => rv.TRGLRV_REGLES_VALEURID).ToArray();

        // Trigger event callback.
        return OnFiltersChanged.InvokeAsync(_filters);
    }
    #endregion

    #region Action DropDownList
    /// <summary>
    /// Sf query applied on action dropdown list.
    /// </summary>
    private readonly Query _actionDropDownQuery = new Query()
        .Sort(nameof(TACT_ACTIONS.TACT_LIB), SortOrder.Ascending.ToString());

    /// <summary>
    /// Event triggered when selected value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private Task ValueChangeAsync(ChangeEventArgs<string, TACT_ACTIONS> args)
        // Trigger event callback.
        => OnFiltersChanged.InvokeAsync(_filters);
    #endregion
}
