using Cronos;

namespace Krialys.Common.Extensions;

public static class CronosExtensions
{
    /// <summary>
    /// Get all next occurence/s from DateTimeOffset Dt to DateTimeOffset Dt + T
    /// var debut = GetLocalDateTime();
    /// var fin = debut.AddHours(1).Truncate(TimeSpan.FromMinutes(1));
    /// var prochains_lancements = GetOccurrences("*/15 * * * *", debut, fin, TimeZoneInfo.Local).FirstOrDefault(); // Every 15 minutes
    /// ref: https://github.com/HangfireIO/Cronos
    /// </summary>
    /// <param name="cron">Cron Expression</param>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <param name="zone">TZ info</param>
    /// <param name="fromInclusive">Including from (by default)</param>
    /// <param name="toInclusive">Excluding to (by default)</param>
    /// <returns>List of DateTimeOffset if (from > to) else null is returned</returns>
    public static IEnumerable<DateTimeOffset?> GetNextOccurrences(string cron, DateTimeOffset from, DateTimeOffset to,
        TimeZoneInfo zone, bool fromInclusive = true, bool toInclusive = false)
    {
        //            Allowed values    Allowed special characters Comment
        //┌───────────── second(optional)        0 - 59 * , - /    <--------attention semble poser pb si on positionne une valeur
        //│ ┌───────────── minute                0 - 59 * , - /
        //│ │ ┌───────────── hour                0 - 23 * , - /
        //│ │ │ ┌───────────── day of month      1 - 31 * , - / L W ?
        //│ │ │ │ ┌───────────── month           1 - 12 or JAN-DEC * , - /
        //│ │ │ │ │ ┌───────────── day of week   0 - 6  or SUN-SAT * , - / # L ?       Both 0 and 7 means SUN
        //│ │ │ │ │ │
        //* * * * * *

        if (from > to)
            yield break;

        CronExpression expr = default;
        try
        {
            // Dynamic format depending on the number of spaces within slots
            expr = CronExpression.Parse(cron, cron.Split(' ').Length < 6 ? CronFormat.Standard : CronFormat.IncludeSeconds);
        }
        catch
        {
            // Not used
        }

        var nextOccurence = expr?.GetNextOccurrence(from, zone, inclusive: false);
        if (nextOccurence != null && nextOccurence.Value <= to)
            yield return nextOccurence.Value;
    }
}