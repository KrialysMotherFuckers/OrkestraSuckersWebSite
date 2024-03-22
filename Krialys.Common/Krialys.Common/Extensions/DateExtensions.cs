using TimeZoneConverter;

namespace Krialys.Common.Extensions;

/// <summary>Generic static class extension based on <strong>PropertyInfo</strong> reflection</summary>
public static class DateExtensions
{
    /// <summary>
    /// How to truncate milli/seconds/minute off of a .NET DateTime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero)
            return dateTime; // Or could throw an ArgumentException

        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            return dateTime; // do not modify "guard" values

        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    public static DateTimeOffset Truncate(this DateTimeOffset dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero)
            return dateTime; // Or could throw an ArgumentException

        if (dateTime == DateTimeOffset.MinValue || dateTime == DateTimeOffset.MaxValue)
            return dateTime; // do not modify "guard" values

        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    /// <summary>
    /// Get a Local TimeZone Info Id from .NET Core, regardless of what OS you are running on
    /// </summary>
    public static string GetLocalTimeZoneId
        => TZConvert.GetTimeZoneInfo(TimeZoneInfo.Local.Id).Id;

    /// <summary>
    /// Get a TimeZoneInfo object from .NET Core, regardless of what OS you are running on
    /// </summary>
    /// <param name="sourceZoneIdentifier"></param>
    /// <returns></returns>
    public static TimeZoneInfo GetTimeZoneInfo(string sourceZoneIdentifier)
        => TZConvert.GetTimeZoneInfo(sourceZoneIdentifier);

    /// <summary>
    /// UTC DateTime rounded to minute, remove trailing seconds
    /// Use it ONLY for saving in database but NOT for presenting information to user, instead use GetLocalDateTimeOffset
    /// </summary>
    /// <returns>UTC DateTime</returns>
    public static DateTime GetUtcNow() => DateTimeOffset.UtcNow.UtcDateTime.Truncate(TimeSpan.FromMinutes(1));

    /// <summary>
    /// UTC DateTime rounded to seconds, remove trailing milliseconds
    /// Use it ONLY for saving in database but NOT for presenting information to user, instead use GetLocalDateTimeOffset
    /// </summary>
    /// <returns>UTC DateTime</returns>
    public static DateTime GetUtcNowSecond() => DateTimeOffset.UtcNow.UtcDateTime.Truncate(TimeSpan.FromSeconds(1));

    /// <summary>
    /// LOCAL DateTime rounded to minute, remove trailing seconds
    /// Use it ONLY for saving in database but NOT for presenting information to user, instead use GetLocalDateTimeOffset
    /// </summary>
    /// <returns>LOCAL DateTime</returns>
    public static DateTime GetLocaleNow() => DateTimeOffset.Now.LocalDateTime.Truncate(TimeSpan.FromMinutes(1));

    /// <summary>
    /// LOCAL DateTime rounded to seconds, remove trailing milliseconds
    /// Use it ONLY for saving in database but NOT for presenting information to user, instead use GetLocalDateTimeOffset
    /// </summary>
    /// <returns>LOCAL DateTime</returns>
    public static DateTime GetLocaleNowSecond() => DateTimeOffset.Now.LocalDateTime.Truncate(TimeSpan.FromSeconds(1));

    // https://stackoverflow.com/questions/54397001/converting-utc-datetime-to-local-datetime-in-c-sharp

    /// <summary>
    /// Get Local TimeZoneInfo
    /// </summary>
    public static TimeZoneInfo GetLocalZoneInfo => TZConvert.GetTimeZoneInfo(TimeZoneInfo.Local.Id);

    /// <summary>
    /// Convert a DateTime to UTC DateTime using TZConvert
    /// </summary>
    /// <param name="dateTimeToConvert"></param>
    /// <param name="sourceZoneIdentifier"></param>
    /// <returns></returns>
    public static DateTime ConvertToUTC(DateTime dateTimeToConvert, string sourceZoneIdentifier = null)
    {
        var dateTime = new DateTime(dateTimeToConvert.Year, dateTimeToConvert.Month, dateTimeToConvert.Day, dateTimeToConvert.Hour, dateTimeToConvert.Minute, dateTimeToConvert.Second, DateTimeKind.Unspecified);

        var tzi = TZConvert.GetTimeZoneInfo(sourceZoneIdentifier ?? GetLocalTimeZoneId);

        var value = TimeZoneInfo.ConvertTimeToUtc(dateTime, tzi);

        return value;
    }

    /// <summary>
    /// Convert a DateTimeOffset to UTC DateTime using TZConvert
    /// </summary>
    /// <param name="dateTimeOffsetToConvert"></param>
    /// <param name="sourceZoneIdentifier"></param>
    /// <returns></returns>
    public static DateTime ConvertToUTC(DateTimeOffset dateTimeOffsetToConvert, string sourceZoneIdentifier = null)
    {
        var dateTime = new DateTime(dateTimeOffsetToConvert.Year, dateTimeOffsetToConvert.Month, dateTimeOffsetToConvert.Day, dateTimeOffsetToConvert.Hour, dateTimeOffsetToConvert.Minute, dateTimeOffsetToConvert.Second, DateTimeKind.Unspecified);

        var tzi = TZConvert.GetTimeZoneInfo(sourceZoneIdentifier ?? GetLocalTimeZoneId);

        var value = TimeZoneInfo.ConvertTimeToUtc(dateTime, tzi);

        return value;
    }

    /// <summary>
    /// Convert a DateTime to another DateTime from a named TZ using TZConvert
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <param name="destinationZoneIdentifier"></param>
    /// <returns></returns>
    public static DateTime ConvertToTimeZoneFromUtc(DateTime utcDateTime, string destinationZoneIdentifier = null)
    {
        var tzi = TZConvert.GetTimeZoneInfo(destinationZoneIdentifier ?? GetLocalTimeZoneId);

        var value = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tzi);

        return value;
    }

    /// <summary>
    /// Convert a DateTimeOffset to another DateTime from a named TZ using TZConvert
    /// </summary>
    /// <param name="utcDateTimeOffset"></param>
    /// <param name="destinationZoneIdentifier"></param>
    /// <returns></returns>
    public static DateTime ConvertToTimeZoneFromUtc(DateTimeOffset utcDateTimeOffset, string destinationZoneIdentifier)
    {
        var tzi = TZConvert.GetTimeZoneInfo(destinationZoneIdentifier ?? GetLocalTimeZoneId);

        var value = TimeZoneInfo.ConvertTimeFromUtc(utcDateTimeOffset.DateTime, tzi);

        return value;
    }

    /// <summary>
    /// Get first day of week.
    /// </summary>
    /// <param name="dateTime">Extended DateTime.</param>
    /// <returns>First day of week.</returns>
    public static DateTime GetFirstDayOfWeek(this DateTime dateTime)
    {
        // Calculate the number of the day in the week.
        // Since DayOfWeek enum begin with sunday, we must shift it to monday.
        int dayNumberInWeek = (6 + (int)dateTime.DayOfWeek) % 7;
        // Shift date to monday.
        return dateTime.AddDays(-dayNumberInWeek);
    }

    /// <summary>
    /// Get last day of week.
    /// </summary>
    /// <param name="dateTime">Extended DateTime.</param>
    /// <returns>Last day of week.</returns>
    public static DateTime GetLastDayOfWeek(this DateTime dateTime)
    {
        // Calculate the number of the day in the week.
        // Since DayOfWeek enum begin with sunday, we must shift it to monday.
        int dayNumberInWeek = (6 + (int)dateTime.DayOfWeek) % 7;

        // Last day of week = first day of week + six days.
        return dateTime.AddDays(-dayNumberInWeek + 6);
    }

    /// <summary>
    /// Get first day of month.
    /// </summary>
    /// <param name="dateTime">Extended DateTime.</param>
    /// <returns>First day of month.</returns>
    public static DateTime GetFirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Get last day of month.
    /// </summary>
    /// <param name="dateTime">Extended DateTime.</param>
    /// <returns>Last day of month.</returns>
    public static DateTime GetLastDayOfMonth(this DateTime dateTime)
    {
        // Last day of month = first day of month + one month - one day.
        return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// Get first day of previous month.
    /// </summary>
    /// <param name="dateTime">Extended DateTime.</param>
    /// <returns>First day of the previous month.</returns>
    public static DateTime GetFirstDayOfPreviousMonth(this DateTime dateTime)
    {
        return dateTime.GetFirstDayOfMonth().AddMonths(-1);
    }

    /// <summary>
    /// Get last day of previous month.
    /// </summary>
    /// <param name="dateTime">Extended DateTime.</param>
    /// <returns>Last day of the previous month.</returns>
    public static DateTime GetLastDayOfPreviousMonth(this DateTime dateTime)
    {
        return dateTime.GetFirstDayOfMonth().AddDays(-1);
    }

    /// <summary>
    /// Absolute DateTime suffix for querying OData in EDM format
    /// </summary>
    private const string OdataStartDateTimeSuffix = "T00:00:00.00Z";

    /// <summary>
    /// Absolute DateTime suffix for querying OData in EDM format 
    /// </summary>
    private const string OdataEndDateTimeSuffix = "T23:59:59.00Z";

    /// <summary>
    /// Format DateTime as yyy-MM-dd format representation + apply EDM suffix or not
    /// </summary>
    /// <param name="date"></param>
    /// <param name="applyEdmFormat">When true, append EDM suffix</param>
    /// <param name="startOrEndDay">When true, apply start day, else apply end day as EDM suffix</param>
    /// <returns></returns>
    private static string FormatDateTime(DateTime date, bool applyEdmFormat, bool startOrEndDay)
        => $"{date:yyy-MM-dd}{(applyEdmFormat ? (startOrEndDay ? OdataStartDateTimeSuffix : OdataEndDateTimeSuffix) : string.Empty)}";

    /// <summary>
    /// Format DateTimeOffset as yyy-MM-dd format representation + apply EDM suffix or not
    /// </summary>
    /// <param name="startDate">Starting date</param>
    /// <param name="applyEdmFormat">Apply for EDM aka ISO expected format for ODATA dates, default value is true</param>
    /// <returns></returns>
    public static (string Start, string End) FormatDateTimes(DateTime startDate, bool applyEdmFormat = true)
        => (FormatDateTime(startDate, applyEdmFormat, startOrEndDay: true), FormatDateTime(startDate, applyEdmFormat, startOrEndDay: false));

    /// <summary>
    /// Format DateTimeOffset as yyy-MM-dd format representation + apply EDM suffix or not
    /// </summary>
    /// <param name="startDate">Starting date</param>
    /// <param name="endDate"></param>
    /// <param name="applyEdmFormat">Apply for EDM aka ISO expected format for ODATA dates, default value is true</param>
    /// <returns></returns>
    public static (string Start, string End) FormatDateTimes(DateTime startDate, DateTime endDate, bool applyEdmFormat = true)
        => (FormatDateTime(startDate, applyEdmFormat, startOrEndDay: true), FormatDateTime(endDate, applyEdmFormat, startOrEndDay: false));

    /// <summary>
    /// Format DateTimeOffset as yyy-MM-dd format representation + apply EDM suffix or not
    /// </summary>
    /// <param name="date"></param>
    /// <param name="applyEdmFormat">When true, append EDM suffix</param>
    /// <param name="startOrEndDay">When true, apply start day, else apply end day as EDM suffix</param>
    /// <returns></returns>
    private static string FormatDateTimeOffset(DateTimeOffset date, bool applyEdmFormat, bool startOrEndDay)
        => $"{date:yyy-MM-dd}{(applyEdmFormat ? (startOrEndDay ? OdataStartDateTimeSuffix : OdataEndDateTimeSuffix) : string.Empty)}";

    /// <summary>
    /// Format DateTimeOffset as yyy-MM-dd format representation + apply EDM suffix or not
    /// </summary>
    /// <param name="startDate">Starting date</param>
    /// <param name="applyEdmFormat">Apply for EDM aka ISO expected format for ODATA dates, default value is true</param>
    /// <returns></returns>
    public static (string Start, string End) FormatDateTimesOffset(DateTimeOffset startDate, bool applyEdmFormat = true)
        => (FormatDateTimeOffset(startDate, applyEdmFormat, startOrEndDay: true), FormatDateTimeOffset(startDate, applyEdmFormat, startOrEndDay: false));

    /// <summary>
    /// Format DateTimeOffset as yyy-MM-dd format representation + apply EDM suffix or not
    /// </summary>
    /// <param name="startDate">Starting date</param>
    /// <param name="endDate">Ending date</param>
    /// <param name="applyEdmFormat">Apply for EDM aka ISO expected format for ODATA dates, default value is true</param>
    /// <returns></returns>
    public static (string Start, string End) FormatDateTimesOffset(DateTimeOffset startDate, DateTimeOffset endDate, bool applyEdmFormat = true)
        => (FormatDateTimeOffset(startDate, applyEdmFormat, startOrEndDay: true), FormatDateTimeOffset(endDate, applyEdmFormat, startOrEndDay: false));

    public static DateTime TruncateToSecond(DateTime dateTime)
       => dateTime.Truncate(TimeSpan.FromMinutes(1)).Truncate(TimeSpan.FromMilliseconds(1));
}