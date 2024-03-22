using Krialys.Orkestra.Common.Models.Notifications;

namespace Krialys.Orkestra.Web.Infrastructure.Preferences;

public class OrkaTablePreference : INotificationMessage
{
    public bool IsDense { get; set; }
    public bool IsStriped { get; set; }
    public bool HasBorder { get; set; }
    public bool IsHoverable { get; set; }
}