using Krialys.Orkestra.Common.Models.Notifications;

namespace Krialys.Orkestra.Web.Infrastructure.Notifications;

public interface INotificationPublisher
{
    Task PublishAsync(INotificationMessage notification);
}