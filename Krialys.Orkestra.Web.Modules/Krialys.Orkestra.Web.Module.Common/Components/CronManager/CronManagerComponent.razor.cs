using CronExpressionDescriptor;
using Cronos;
using Krialys.Common.Extensions;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;

namespace Krialys.Orkestra.Web.Module.Common.Components.CronManager;

/// <summary>
/// Attention please: this component MUST use LocalDateTime as we save TZID within the DB
/// </summary>
public partial class CronManagerComponent
{
    #region Parameters
    /// <summary>
    /// Is edition enabled?
    /// </summary>
    [Parameter]
    public bool EnableTextBox { get; set; } = true;

    /// <summary>
    /// CRON schedule expressions.
    /// </summary>
    [Parameter]
    public string Cron { get; set; }
    /// <summary>
    /// Callback invoked when Cron changed.
    /// </summary>
    [Parameter]
    public EventCallback<string> CronChanged { get; set; }

    /// <summary>
    /// Is the CRON schedule expressions correct?
    /// </summary>
    [Parameter]
    public bool IsCronValid { get; set; }
    /// <summary>
    /// Callback invoked when IsCronValid changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsCronValidChanged { get; set; }

    /// <summary>
    /// Schedule start date (UTC0).
    /// </summary>
    [Parameter]
    public DateTime StartDate { get; set; }
    /// <summary>
    /// Callback invoked when StartDate changed.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime> StartDateChanged { get; set; }

    /// <summary>
    /// Schedule end date (UTC0).
    /// </summary>
    [Parameter]
    public DateTime? EndDate { get; set; }
    /// <summary>
    /// Callback invoked when EndDate changed.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime?> EndDateChanged { get; set; }
    #endregion

    #region Cron slot
    /// <summary>
    /// Describe an element of the CRON.
    /// </summary>
    public class Slots
    {
        public string Value { get; set; }
        public string Description { get; set; }
        public string CssClass { get; set; } = "description-text";
        public bool HasFailed => CssClass is not "description-text";
        // Type used to get the description with ExpressionDescriptor NuGet.
        public DescriptionTypeEnum DescriptionType { get; set; }
    }

    /// <summary>
    /// All CRON elements.
    /// </summary>
    private readonly IList<Slots> _cronSlots = new List<Slots>(5);
    private readonly Slots _minutes = new()
    {
        Value = "00",
        DescriptionType = DescriptionTypeEnum.MINUTES
    };
    private readonly Slots _hours = new()
    {
        Value = "00",
        DescriptionType = DescriptionTypeEnum.HOURS
    };
    private readonly Slots _days = new()
    {
        Value = "?",
        DescriptionType = DescriptionTypeEnum.DAYOFMONTH
    };
    private readonly Slots _months = new()
    {
        Value = "1-12",
        DescriptionType = DescriptionTypeEnum.MONTH
    };
    private readonly Slots _weekDdays = new()
    {
        Value = "MON-FRI",
        DescriptionType = DescriptionTypeEnum.DAYOFWEEK
    };
    #endregion

    #region CRON manager properties
    /// <summary>
    /// CronExpressionDescriptor options.
    /// </summary>
    private Options Options => new Options
    {
        DayOfWeekStartIndexZero = true,
        Use24HourTimeFormat = true,
        Verbose = true,
        ThrowExceptionOnParseError = true,
        Locale = LocalStorage.GetItemAsString("BlazorCulture") ?? Config["CultureInfo"]
    };

    /// <summary>
    /// Description of the CRON in human language.
    /// </summary>
    private string _cronDescription = string.Empty;
    /// <summary>
    /// Color of the CRON description.
    /// </summary>
    private string _cronDescriptionCssClass = string.Empty;

    /// <summary>
    /// Next CRON occurences list.
    /// </summary>
    private IList<string> _nextCronOccurencesList = new List<string>();
    /// <summary>
    /// Next CRON occurences as text.
    /// </summary>
    private string _nextCronOccurences = string.Empty;
    /// <summary>
    /// Next CRON occurences as text for Title.
    /// </summary>
    private string _nextCronOccurencesTitle = string.Empty;
    /// <summary>
    /// Color of next CRON occurences text.
    /// </summary>
    private string _nextCronOccurencesCssClass = string.Empty;

    /// <summary>
    /// CSS class applied on CRON end date picker.
    /// </summary>
    private string _endDateCssClass = string.Empty;
    #endregion

    #region Blazor Life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Initialize start date.
        // If start date is not defined.
        if (StartDate.Equals(default))
        {
            // Set start date to current time.
            StartDate = DateExtensions.GetLocaleNow();

            // Date is unspecified because offset is defined by timezone field.
            StartDate = DateTime.SpecifyKind(StartDate, DateTimeKind.Unspecified);

            // Update parent component with initialized start date.
            await StartDateChanged.InvokeAsync(StartDate);
        }

        // Initialize CRON by parsing the Cron parameter.
        // If parsing failed, default values are taken.
        if (!string.IsNullOrWhiteSpace(Cron))
        {
            IList<string> parsed = Cron.Split(' ').ToList();

            if (parsed.Count == 5)
            {
                _minutes.Value = parsed[0];
                _hours.Value = parsed[1];
                _days.Value = parsed[2];
                _months.Value = parsed[3] == "*" ? "1-12" : parsed[3];
                _weekDdays.Value = parsed[4] == "*" ? "0-6" : parsed[4];
            }
        }

        // Add all CRON slots into a list.
        _cronSlots.Add(_minutes);
        _cronSlots.Add(_hours);
        _cronSlots.Add(_days);
        _cronSlots.Add(_months);
        _cronSlots.Add(_weekDdays);

        await InitializeCron();
    }
    #endregion

    #region Helper with examples values
    /// <summary>
    /// Examples of hours values.
    /// </summary>
    private readonly string[] _examplesHours = { "*", "00", "00,15", "00-15", "0/15" };

    /// <summary>
    /// Get CRON hours description in human language.
    /// </summary>
    /// <param name="hours">CRON hours value.</param>
    /// <returns>Cron hours description</returns>
    private string GetHoursDescription(string hours)
    {
        var cron = $"00 {hours} ? 1-12 MON-FRI";
        var expression = new ExpressionDescriptor(cron, Options);
        return expression.GetDescription(DescriptionTypeEnum.HOURS);
    }

    /// <summary>
    /// Event called when Hours helper is selected.
    /// </summary>
    private async Task HoursHelperSelectedAsync(MenuEventArgs args)
    {
        if (EnableTextBox)
        {
            UpdateCronSlot(_hours, args.Item.Text);
            await OnCronChangedAsync();
        }
    }


    /// <summary>
    /// Examples of minutes values.
    /// </summary>
    private readonly string[] _examplesMinutes = { "*", "00", "00,15", "00-15", "0/15" };

    /// <summary>
    /// Get CRON minutes description in human language.
    /// </summary>
    /// <param name="minutes">CRON minutes value.</param>
    /// <returns>Cron minutes description</returns>
    private string GetMinutesDescription(string minutes)
    {
        var cron = $"{minutes} 00 ? 1-12 MON-FRI";
        var expression = new ExpressionDescriptor(cron, Options);
        return expression.GetDescription(DescriptionTypeEnum.MINUTES);
    }

    /// <summary>
    /// Event called when Minutes helper is selected.
    /// </summary>
    private async Task MinutesHelperSelectedAsync(MenuEventArgs args)
    {
        if (EnableTextBox)
        {
            UpdateCronSlot(_minutes, args.Item.Text);
            await OnCronChangedAsync();
        }
    }


    /// <summary>
    /// Examples of days values.
    /// </summary>
    private readonly string[] _examplesDays = { "*", "1", "1,15", "1-15", "1/15" };

    /// <summary>
    /// Get CRON days description in human language.
    /// </summary>
    /// <param name="days">CRON days value.</param>
    /// <returns>Cron days description</returns>
    private string GetDaysDescription(string days)
    {
        var cron = $"00 00 {days} 1-12 MON-FRI";
        var expression = new ExpressionDescriptor(cron, Options);
        return expression.GetDescription(DescriptionTypeEnum.DAYOFMONTH);
    }

    /// <summary>
    /// Event called when Days helper is selected.
    /// </summary>
    private async Task DaysHelperSelectedAsync(MenuEventArgs args)
    {
        if (EnableTextBox)
        {
            UpdateCronSlot(_days, args.Item.Text);
            await OnCronChangedAsync();
        }
    }


    /// <summary>
    /// Examples of weekdays values.
    /// </summary>
    private readonly string[] _examplesWeekdays = { "1-7", "MON", "MON,WED", "MON-FRI", "MON/2" };

    /// <summary>
    /// Get CRON weekdays description in human language.
    /// </summary>
    /// <param name="weekdays">CRON weekdays value.</param>
    /// <returns>Cron weekdays description</returns>
    private string GetWeekdaysDescription(string weekdays)
    {
        var cron = $"00 00 ? 1-12 {weekdays}";
        var expression = new ExpressionDescriptor(cron, Options);
        return expression.GetDescription(DescriptionTypeEnum.DAYOFWEEK);
    }

    /// <summary>
    /// Event called when Weekdays helper is selected.
    /// </summary>
    private async Task WeekdaysHelperSelectedAsync(MenuEventArgs args)
    {
        if (EnableTextBox)
        {
            UpdateCronSlot(_weekDdays, args.Item.Text);
            await OnCronChangedAsync();
        }
    }


    /// <summary>
    /// Examples of months values.
    /// </summary>
    private readonly string[] _examplesMonths = { "1-12", "JAN", "1,6", "JUN-DEC", "MAR/2" };

    /// <summary>
    /// Get CRON months description in human language.
    /// </summary>
    /// <param name="months">CRON months value.</param>
    /// <returns>Cron months description</returns>
    private string GetMonthsDescription(string months)
    {
        var cron = $"00 00 ? {months} MON-FRI";
        var expression = new ExpressionDescriptor(cron, Options);
        return expression.GetDescription(DescriptionTypeEnum.MONTH);
    }

    /// <summary>
    /// Event called when Months helper is selected.
    /// </summary>
    private async Task MonthsHelperSelectedAsync(MenuEventArgs args)
    {
        if (EnableTextBox)
        {
            UpdateCronSlot(_months, args.Item.Text);
            await OnCronChangedAsync();
        }
    }
    #endregion

    #region DatePicker
    /// <summary>
    /// Date returned by SfDateTimePicker component when it is empty.
    /// </summary>
    readonly DateTime DatePickerEmptyValue = new DateTime(1900, 1, 1);

    /// <summary>
    /// Event triggered when CRON start date is changed with date picker.
    /// </summary>
    /// <param name="args">Change event argument.</param>
    private async Task DatePickerValueChangeHandlerStartAsync(ChangedEventArgs<DateTime> args)
    {
        // If no date selected, set start date to now.
        if (StartDate.Equals(DatePickerEmptyValue))
        {
            // Set start date to current time.
            StartDate = DateExtensions.GetLocaleNow();
        }

        // Date is unspecified because offset is defined by timezone field.
        StartDate = DateTime.SpecifyKind(StartDate, DateTimeKind.Unspecified);

        // Send date to parent component.
        await StartDateChanged.InvokeAsync(StartDate);

        await OnCronChangedAsync();
    }

    /// <summary>
    /// Event triggered when CRON end date is changed with date picker.
    /// </summary>
    /// <param name="args">Change event argument.</param>
    private async Task DatePickerValueChangeHandlerStopAsync(ChangedEventArgs<DateTime?> args)
    {
        if (EndDate.HasValue)
        {
            // Date is unspecified because offset is defined by timezone field.
            EndDate = DateTime.SpecifyKind(EndDate.Value, DateTimeKind.Unspecified);
        }

        // Send date to parent component.
        await EndDateChanged.InvokeAsync(EndDate);

        await OnCronChangedAsync();
    }
    #endregion

    #region CRON manager Events
    private async Task InputHandlerMinutesAsync(InputEventArgs args)
    {
        UpdateCronSlot(_minutes, args.Value);
        await OnCronChangedAsync();
    }

    private async Task InputHandlerHoursAsync(InputEventArgs args)
    {
        UpdateCronSlot(_hours, args.Value);
        await OnCronChangedAsync();
    }

    private async Task InputHandlerDaysAsync(InputEventArgs args)
    {
        UpdateCronSlot(_days, args.Value);
        await OnCronChangedAsync();
    }

    private async Task InputHandlerMonthsAsync(InputEventArgs args)
    {
        UpdateCronSlot(_months, args.Value);
        await OnCronChangedAsync();
    }

    private async Task InputHandlerWeekDaysAsync(InputEventArgs args)
    {
        UpdateCronSlot(_weekDdays, args.Value);
        await OnCronChangedAsync();
    }
    #endregion

    #region Cron manager methods
    /// <summary>
    /// Initialize CRON.
    /// </summary>
    private async Task InitializeCron()
    {
        // Update CRON slots (with default values or values from parameters).
        UpdateCronSlot(_minutes, _minutes.Value);
        UpdateCronSlot(_hours, _hours.Value);
        UpdateCronSlot(_days, _days.Value);
        UpdateCronSlot(_months, _months.Value);
        UpdateCronSlot(_weekDdays, _weekDdays.Value);

        // Apply CRON.
        await OnCronChangedAsync();
    }

    /// <summary>
    /// Update CRON slot with new value.
    /// </summary>
    /// <param name="slot">CRON element being modified.</param>
    /// <param name="value">CRON value applied.</param>
    private void UpdateCronSlot(Slots slot, string value)
    {
        // Update this CRON slot value.
        slot.Value = value;

        Cron = $"{_minutes.Value} {_hours.Value} {_days.Value} {_months.Value} {_weekDdays.Value}";

        // Get description for this CRON slot.
        try
        {
            var expression = new ExpressionDescriptor(Cron, Options);
            slot.Description = expression.GetDescription(slot.DescriptionType);
            slot.CssClass = "description-text";
        }
        catch //(Exception ex)
        {
            //slot.Description = ex.InnerException?.Message is not null ? ex.InnerException?.Message : ex.Message;
            slot.Description = Trad.Keys["CronManager:InvalidCronField"];
            slot.CssClass = "e-error";
        }
    }

    /// <summary>
    /// Event called when a CRON element changed.
    /// Update desciption, next occurences, component binded parameters...
    /// </summary>
    private async Task OnCronChangedAsync()
    {
        Cron = $"{_minutes.Value} {_hours.Value} {_days.Value} {_months.Value} {_weekDdays.Value}";

        // Cron is valid if each slot is valid.
        bool isCronValid = !_cronSlots.Any(x => x.HasFailed);

        // Get CRON description.
        try
        {
            _cronDescription = ExpressionDescriptor.GetDescription(Cron, Options);
        }
        catch (Exception)
        {
            // Can't get description, CRON is not valid.
            isCronValid = false;
        }

        // If dates are incorrect.
        if (StartDate >= EndDate)
        {
            // End date error.
            _endDateCssClass = "e-error";

            // Start date can't be after end date.
            isCronValid = false;
        }
        else
        {
            // Reset end date css.
            _endDateCssClass = string.Empty;
        }

        // Reset next occurences.
        _nextCronOccurencesList.Clear();

        // If valid CRON.
        if (isCronValid)
        {
            // Get CRON next occurences.
            // Next occurences start date is the maximum between selected start date and current date.
            var from = StartDate < DateTimeOffset.Now ? DateTimeOffset.Now : StartDate;
            from = from.Truncate(TimeSpan.FromMinutes(1));

            // Next occurences end date is the selected end date or maximum value.
            var to = EndDate ?? DateTimeOffset.MaxValue;
            to = to.Truncate(TimeSpan.FromMinutes(1));

            // If dates are correct, we can get next occurences.
            if (from < to)
            {
                try
                {
                    // Get next occurences
                    _nextCronOccurencesList = CronExpression.Parse(Cron).GetOccurrences(from, to, TimeZoneInfo.Local)
                        .Take(11)
                        .Select(date => $"{date:ddd} {date:g}")
                        .ToList();
                }
                catch (Exception)
                {
                    // Can't get next occurences, CRON is not valid.
                    isCronValid = false;
                }
            }
        }

        // Update CRON description text.
        if (!isCronValid)
        {
            // Set CRON description.
            _cronDescription = Trad.Keys["CronManager:InvalidCronDescription"];
            _cronDescriptionCssClass = "e-error";
        }
        else
        {
            _cronDescriptionCssClass = "description-text";
        }

        // Update CRON next occurences text.
        if (_nextCronOccurencesList.Any())
        {
            _nextCronOccurences = string.Join(", ", _nextCronOccurencesList.Take(8)) + (_nextCronOccurencesList.Count > 8 ? ", ..." : string.Empty);
            _nextCronOccurencesTitle = _nextCronOccurences.Replace(", ", "\n");
            _nextCronOccurencesCssClass = "e-success";
        }
        else
        {
            _nextCronOccurences = Trad.Keys["CronManager:NoCronOccurencesScheduled"];
            _nextCronOccurencesTitle = _nextCronOccurences;
            _nextCronOccurencesCssClass = "e-error";

            // No next occurrences, CRON is not valid.
            isCronValid = false;
        }

        // Update parent with new value.
        await CronChanged.InvokeAsync(Cron);
        IsCronValid = isCronValid;
        await IsCronValidChanged.InvokeAsync(IsCronValid);
    }
    #endregion
}
