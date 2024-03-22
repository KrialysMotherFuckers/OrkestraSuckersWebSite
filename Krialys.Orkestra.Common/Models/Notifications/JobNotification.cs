﻿namespace Krialys.Orkestra.Common.Models.Notifications;

public class JobNotification : INotificationMessage
{
    public string Message { get; set; }
    public string JobId { get; set; }
    public decimal Progress { get; set; }
}