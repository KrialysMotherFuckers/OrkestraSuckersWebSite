using Krialys.Common.Extensions;
using Krialys.Data.EF.Mso;
using Krialys.Common.Literals;
using Krialys.Orkestra.WebApi.Proxy;
using Syncfusion.Blazor;

namespace Krialys.Orkestra.Web.Module.MSO.DI;

public interface IRapprochementsSchedulerServices
{
    Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null);
}

/// <summary>
/// Attention ici tu utilises le cache via _proxy.GetAllAsync => implémenter les notifications pour mettre à jour le scheduler sinon tu n'auras pas les datas mises à jour
/// Rappel : toujours avoir en tête que toute classe qui sera utilisée pour agrémenter/compléter/rendre service dans un composant doit être injecté
/// Si classe est 'pure' alors on l'injecte en Singleton, sinon en Scoped, et on les range dans RazorLib/DI
/// </summary>
public class RapprochementsSchedulerServices : DataAdaptor, IRapprochementsSchedulerServices
{
    private readonly IHttpProxyCore _proxy;
    private readonly IAppointmentServices _appointments;

    /// <summary>
    /// Dictionary linking code (TRA_CODE) and applications label (TRAPL_LIB).
    /// </summary>
    private static Dictionary<string, string> CodeApplicationDico { get; } = new();

    /// <summary>
    /// Minimum duration applied to an appointment.
    /// The appointment must be long enough so that its subject is readable.
    /// </summary>
    private const double AppointmentMinDuration = 20;

    public RapprochementsSchedulerServices(IHttpProxyCore proxy, IAppointmentServices appointments)
    {
        _proxy = proxy;
        _appointments = appointments;
    }

    /// <summary>
    /// READ - Core function
    /// </summary>
    /// <param name="dataManagerRequest"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null)
        => await GetAppointments(
            dataManagerRequest.Params["StartDate"].ToString(),
            dataManagerRequest.Params["EndDate"].ToString())
        ;

    /// <summary>
    /// Get events - Business logic
    /// </summary>
    /// <param name="startDate">Start date from scheduler.</param>
    /// <param name="endDate">End date from scheduler.</param>
    /// <returns>Appointments from StartDate to EndDate.</returns>
    private async Task<IList<AppointmentServices.Appointment>> GetAppointments(string startDate, string endDate)
    {
        if (!CodeApplicationDico.Any())
        {
            // Prepare a code-application dictionary linking a code to an application label.
            var attendus = await _proxy.GetEnumerableAsync<TRA_ATTENDUS>("?$expand=TRAPL_APPLICATION");

            foreach (var attendu in attendus)
            {
                CodeApplicationDico.TryAdd(attendu.TRA_CODE, attendu.TRAPL_APPLICATION.TRAPL_LIB);
            }
        }

        // Clear appointments list at first
        if (_appointments.List.Any())
            _appointments.List.Clear();

        // Get scheduler timespan (start and end date).
        //DateTimeOffset from = Convert.ToDateTime(StartDate);
        //DateTimeOffset to = Convert.ToDateTime(EndDate);

        // remplacé par seb suite changement DateTimeOffset en DateTime
        var from = Convert.ToDateTime(startDate);
        var to = Convert.ToDateTime(endDate);

        // Transform dates to strings (in a format supported by odata requests).
        // Odata: If the string value does not contain a time-zone offset, it is treated as UTC.

        //string fromDate = from.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        //string toDate = to.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        // remplacé par seb suite changement DateTimeOffset en DateTime

        string fromDate = from.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        string toDate = to.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

        /**** Part 1 : Get "attendus" occurences. ****/
        // Get "TRAP_ATTENDUS_PLANIFS" with active status,
        // expanded with "TRP_PLANIFS" with active status,
        // expanded with "TRA_ATTENDUS" with active status,
        // where "TRP_DATE_DEBUT_PLANIF" is before end date,
        // where "TRP_DATE_FIN_PLANIF" is after start date or null,
        // where "TRA_DEBUT_VALIDITE" is before end date,
        // where "TRA_FIN_VALIDITE" is after start date or null.
        string expand = "TRP_PLANIF, TRA_ATTENDU";

        string filter = $"(TRAP_STATUT eq 'A' and TRP_PLANIF/TRP_STATUT eq '{StatusLiteral.Available}' and TRA_ATTENDU/TRA_STATUT eq '{StatusLiteral.Available}'" +
            $" and TRP_PLANIF/TRP_DATE_DEBUT_PLANIF lt {toDate}" +
            $" and (TRP_PLANIF/TRP_DATE_FIN_PLANIF gt {fromDate} or TRP_PLANIF/TRP_DATE_FIN_PLANIF eq null)" +
            $" and TRA_ATTENDU/TRA_DEBUT_VALIDITE lt {toDate}" +
            $" and (TRA_ATTENDU/TRA_FIN_VALIDITE gt {fromDate} or TRA_ATTENDU/TRA_FIN_VALIDITE eq null))";

        var attendusPlanifs = (await _proxy.GetEnumerableAsync<TRAP_ATTENDUS_PLANIFS>($"?$expand={expand}&$filter={filter}")
            ).Where(x => x.TRA_ATTENDU is not null);

        // Apply additionnal filters on data.
        // These filters are based on selections from scheduler menu.
        if (_appointments.Filters.Attendus?.Any() == true)
        {
            attendusPlanifs = DataOperations.PerformFiltering(attendusPlanifs,
                _appointments.Filters.Attendus,
                _appointments.Filters.Attendus[0].Operator);
        }

        foreach (var attenduPlanif in attendusPlanifs)
        {
            // Get attendu min duration.
            double minDuration = attenduPlanif.TRA_ATTENDU.TRA_DUREE_TRAITEMENT_MIN ?? 0;

            // Get attendu max duration.
            double maxDuration = attenduPlanif.TRA_ATTENDU.TRA_DUREE_TRAITEMENT_MAX ?? minDuration;

            // Calculate attendu mean duration.
            double meanDuration = (minDuration + maxDuration) / 2;

            // Calculate duration displayed in the scheduler.
            // It is the minimum between attendu mean duration and a minimum displayed duration.
            double duration = meanDuration > AppointmentMinDuration ? meanDuration : AppointmentMinDuration;

            // Calculate Cron timespan.
            // Take into account timepsans ot the scheduler, of the planification and of the attendu validity.

            //DateTimeOffset fromCron = new[]
            //{
            //    from,
            //    attenduPlanif.TRP_PLANIF.TRP_DATE_DEBUT_PLANIF,
            //    attenduPlanif.TRA_ATTENDU.TRA_DEBUT_VALIDITE
            //}.Max();

            //DateTimeOffset toCron = (DateTimeOffset)new[]
            //{
            //    to,
            //    attenduPlanif.TRP_PLANIF.TRP_DATE_FIN_PLANIF,
            //    attenduPlanif.TRA_ATTENDU.TRA_FIN_VALIDITE
            //}.Where(d => d is not null).Min();

            // remplacé par seb suite changement DateTimeOffset en DateTime
            var fromCron = new[]
            {
                from,
                attenduPlanif.TRP_PLANIF.TRP_DATE_DEBUT_PLANIF,
                attenduPlanif.TRA_ATTENDU.TRA_DEBUT_VALIDITE
            }.Max();

            var toCron = new[]
            {
                to,
                attenduPlanif.TRP_PLANIF.TRP_DATE_FIN_PLANIF,
                attenduPlanif.TRA_ATTENDU.TRA_FIN_VALIDITE
            }.Where(d => d is not null).Min();


            // Get Cron occurences from the timespan.
            if (toCron == null)
                continue;

            var dates = CronosExtensions.GetNextOccurrences(attenduPlanif.TRP_PLANIF.TRP_CRON, fromCron, toCron.Value, TimeZoneInfo.Local)
                .Where(x => x.HasValue);

            foreach (var date in dates)
            {
                // Add appointment to scheduler.
                _appointments.List.Add(new()
                {
                    // Subject =  TRA_CODE (TRAPL_APPLICATION)
                    Subject = $"{attenduPlanif.TRA_ATTENDU.TRA_CODE} ({CodeApplicationDico[attenduPlanif.TRA_ATTENDU.TRA_CODE]})",
                    StartTime = date.Value.LocalDateTime,
                    EndTime = date.Value.LocalDateTime.AddMinutes(duration),
                    RealEndTime = date.Value.LocalDateTime.AddMinutes(meanDuration),
                    EntityId = 1,
                    Attendu = attenduPlanif.TRA_ATTENDU
                });
            }
        }

        /**** Part 2 : Get "logs" occurences. ****/
        // Get "TTL_LOGS" where "TTL_DATE_DEBUT" is before end date where "TTL_DATE_FIN" is after start date.
        filter = $"(TTL_DATE_DEBUT lt {toDate} and TTL_DATE_FIN gt {fromDate} and TRA_CODE ne null)";  // Will crash CodeApplicationDico when TRA_CODE is null
        var logs = (await _proxy.GetEnumerableAsync<TTL_LOGS>($"?$filter={filter}"))
            .Where(x => x.TTL_DATE_DEBUT.HasValue && x.TTL_DATE_FIN.HasValue);

        // Apply additionnal filters on data.
        // These filters are based on selections from scheduler menu.
        if (_appointments.Filters.Logs?.Any() == true)
        {
            logs = DataOperations.PerformFiltering(logs,
                _appointments.Filters.Logs,
                _appointments.Filters.Logs[0].Operator);
        }

        // Add appointments to scheduler.
        foreach (var log in logs)
        {
            if (log.TTL_DATE_FIN == null)
                continue;

            if (log.TTL_DATE_DEBUT != null)
                _appointments.List.Add(new AppointmentServices.Appointment
                {
                    // Subject =  TRA_CODE (TRAPL_APPLICATION)
                    Subject = CodeApplicationDico.TryGetValue(log.TRA_CODE, out string value)
                        ? $"{log.TRA_CODE} ({value})"
                        : $"{log.TRA_CODE}",
                    StartTime = log.TTL_DATE_DEBUT.Value, //.LocalDateTime, //seb
                    EntityId = 2,
                    Log = log,

                    // Calculate appointment EndTime:
                    // EndTime should be minimum "AppointmentMinDuration" minutes after StartTime
                    // so that the event is readable in the scheduler.
                    //EndTime = (log.TTL_DATE_FIN - log.TTL_DATE_DEBUT) < TimeSpan.FromMinutes(AppointmentMinDuration)
                    //? log.TTL_DATE_DEBUT.Value.LocalDateTime.AddMinutes(AppointmentMinDuration)
                    //: log.TTL_DATE_FIN.Value.LocalDateTime,

                    // Real end time of the log.
                    //RealEndTime = log.TTL_DATE_FIN.Value.LocalDateTime,

                    // remplacé par seb suite a modif datetimeoffseet to datetime
                    EndTime = log.TTL_DATE_FIN - log.TTL_DATE_DEBUT <
                              TimeSpan.FromMinutes(AppointmentMinDuration)
                        ? log.TTL_DATE_DEBUT.Value.AddMinutes(AppointmentMinDuration)
                        : log.TTL_DATE_FIN.Value,

                    // Real end time of the log.
                    RealEndTime = log.TTL_DATE_FIN.Value,

                    // Set resultat ID using C# 9 (much better!)
                    ResultId = log.TTL_RESULTAT switch
                    {
                        "OK" => 1,
                        "KO" => 2,
                        "NA" => 3,
                        StatusLiteral.InProgress => 4,
                        _ => 5,
                    }
                });
        }

        return _appointments.List;
    }
}
