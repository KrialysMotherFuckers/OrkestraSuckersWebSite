namespace Krialys.Orkestra.Common.Models.Admin;

[Flags]
public enum LicenseType
{
    Trial = 0,
    Subscription = 2,
    LimitedTime = 4,
    Perpetual = 8
}

public class Licence
{
    public bool IsTrialVersion { get; set; } = false;
    public string CustomerName { get; set; }
    public string CustomerRefCode { get; set; }
    public string CustomerEmail { get; set; }
    public int DefaultExpirationTimeInDays { get; set; } = 91; //By default, we set the license to 3 month
    public DateTime? EndValidationDate { get; set; }
    public bool IsActive { get; set; }
    public string LicenseKey { get; set; }
    public LicenseType LicenseType { get; set; } = LicenseType.Trial;
    public string LicenseMessage { get; set; }
}

