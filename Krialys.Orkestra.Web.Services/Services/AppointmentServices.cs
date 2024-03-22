using Krialys.Data.EF.Mso;
using Krialys.Data.EF.Resources;
using Syncfusion.Blazor.Data;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Krialys.Orkestra.Web.Module.MSO.DI;

/// <summary>
/// Implements the strict necessary for Scheduler's appointments management, see how the class is implemented as one self contained class below
/// Best practice: when using an injected class if you have Lists, then instanciate them from each respective class
/// Only use { get; } to prevent from non attended new() from user's code (bad practice)
/// </summary>
public interface IAppointmentServices
{
    /// <summary>
    /// Appointments list { get; }
    /// </summary>
    IList<AppointmentServices.Appointment> List { get; }

    /// <summary>
    /// Filters list { get; }
    /// </summary>
    AppointmentServices.Filter Filters { get; }
}

/// <summary>
/// Appointment services
/// </summary>
public class AppointmentServices : IAppointmentServices
{
    /// <summary>
    /// Appointments list
    /// </summary>
    public IList<Appointment> List { get; } = new List<Appointment>();

    /// <summary>
    /// Where filters
    /// </summary>
    public Filter Filters { get; } = new();

    /// <summary>
    /// Where filters used for Attendus + Logs
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// Clear lists content
        /// </summary>
        public void Clear()
        {
            Attendus.Clear();
            Logs.Clear();
        }

        /// <summary>
        /// Additionnal filter applied on Attendus after Odata request { get; }
        /// </summary>
        public List<WhereFilter> Attendus { get; } = new();

        /// <summary>
        /// Additionnal filter applied on Logs after Odata request { get; }
        /// </summary>
        public List<WhereFilter> Logs { get; } = new();
    }

    /// <summary>
    /// Represents an event in the scheduler.
    /// </summary>
    public class Appointment
    {
        // Id is needed when edition is enabled.
        //public int Id { get; set; }

        /// <summary>
        /// Subject of the appointment, displayed in event cell.
        /// Subject = "TRA_CODE (TRAPL_APPLICATION)"
        /// </summary>
        [Display(Name = "Sujet")]
        public string Subject { get; set; }

        /// <summary>
        /// Start of the appointment.
        /// </summary>
        [Display(Name = "Date de début")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of the appointment as displayed in scheduler.
        /// Can be different from real end time because there is a minimal timestamp 
        /// to make the appointment visible in scheduler.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Real end time of the appointment.
        /// </summary>
        [Display(Name = "Date de fin")]
        public DateTime RealEndTime { get; set; }

        /// <summary>
        /// Attendus associated to this appointment.
        /// </summary>
        public TRA_ATTENDUS Attendu { get; set; }

        /// <summary>
        /// Logs associated to this appointment.
        /// </summary>
        public TTL_LOGS Log { get; set; }

        /// <summary>
        /// Id of the "Entity" scheduler ressource.
        /// Used to distinguish Attendus from Logs. 
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Id of the "Result" scheduler ressource.
        /// Used to distinguish Logs appointments by TRR_CODE. 
        /// </summary>
        public int ResultId { get; set; }

        /// <summary>
        /// Convert appointment to a dictionary.
        /// </summary>
        /// <returns>Dictionary with the property display name as key and the property value.</returns>
        public Dictionary<string, string> AsText(IDataAnnotations dataAnnotations)
        {
            // Dictionary to complete.
            Dictionary<string, string> appointmentDico = new();
            string key;

            // Browse through each Appointment properties.
            foreach (var prop in GetType().GetProperties())
            {
                key = prop.GetCustomAttribute<DisplayAttribute>()?.Name;

                if (!string.IsNullOrEmpty(key))
                {
                    appointmentDico.Add(key, prop.GetValue(this, null)!.ToString());
                }
            }

            // If Appointment contains Attendu object.
            if (Attendu is not null)
            {
                // Browse through each Appointment.Attendu properties.
                foreach (var prop in typeof(TRA_ATTENDUS).GetProperties())
                {
                    key = dataAnnotations.Display<TRA_ATTENDUS>(prop.Name);

                    string value = default;
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?)
                                                              || prop.PropertyType == typeof(DateTimeOffset)
                                                              || prop.PropertyType == typeof(DateTimeOffset?))
                    {
                        DateTimeOffset.TryParse(prop.GetValue(Attendu)?.ToString(), out var dateTime);
                        value = dateTime.ToString("g");
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)
                                                              || prop.PropertyType == typeof(string))
                    {
                        value = prop.GetValue(Attendu)?.ToString();
                    }

                    if (!string.IsNullOrEmpty(key) && value is not null)
                    {
                        appointmentDico.Add(key, value);
                    }
                }
            }

            // If Appointment contains Log object.
            if (Log is not null)
            {
                // Browse through each Appointment.Log properties.
                foreach (var prop in typeof(TTL_LOGS).GetProperties())
                {
                    key = dataAnnotations.Display<TTL_LOGS>(prop.Name);

                    string value;
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?)
                                                              || prop.PropertyType == typeof(DateTimeOffset) || prop.PropertyType == typeof(DateTimeOffset?))
                    {
                        DateTimeOffset.TryParse(prop.GetValue(Log)?.ToString(), out var dateTime);
                        value = dateTime.ToString("g");
                    }
                    else
                    {
                        value = prop.GetValue(Log)?.ToString();
                    }

                    if (!string.IsNullOrEmpty(key))
                    {
                        appointmentDico.Add(key, value);
                    }
                }
            }

            return appointmentDico;
        }
    }

    /// <summary>
    /// Ressources used to configure the scheduler.
    /// Appointments are grouped in ressources depending on their Entity and Result.
    /// </summary>
    public class Ressource
    {
        public int Id { get; set; }

        public string ResultText { get; set; }

        public string ResultColor { get; set; }

        public int EntityGroupId { get; set; }

        public string EntityText { get; set; }

        public string EntityColor { get; set; }
    }
}
