using Krialys.Orkestra.Common.Models.Notifications;
using MediatR;

namespace Krialys.Orkestra.Web.Infrastructure.Notifications;

public class NotificationWrapper<TNotificationMessage> : INotification
    where TNotificationMessage : INotificationMessage
{
    public NotificationWrapper(TNotificationMessage notification) => Notification = notification;

    public TNotificationMessage Notification { get; }
}