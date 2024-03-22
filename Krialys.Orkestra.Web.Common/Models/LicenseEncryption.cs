namespace Krialys.Orkestra.Web.Common.Models
{
    public class LicenseEncryption
    {
        public bool IsTrialVersion { get; set; } = false;
        public DateTime? Expirationdate { get; set; } = DateTime.UtcNow.AddYears(1);
        public string CustomerName { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
    }
}
