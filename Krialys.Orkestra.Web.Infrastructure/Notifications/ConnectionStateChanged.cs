using Krialys.Orkestra.Common.Models.Notifications;

namespace Krialys.Orkestra.Web.Infrastructure.Notifications;

public record ConnectionStateChanged(ConnectionState State, string Message) : INotificationMessage;