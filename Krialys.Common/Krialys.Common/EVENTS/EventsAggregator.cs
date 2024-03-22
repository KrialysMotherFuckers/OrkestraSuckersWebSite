namespace Krialys.Common.Events;

public interface IEventsAggregator
{
    string Message { get; set; }
    string Entity { get; set; }
}

public class EventsAggregator : IEventsAggregator
{
    public EventsAggregator(string message, string entity)
    {
        Message = message;
        Entity = entity;
    }

    public string Message { get; set; }

    public string Entity { get; set; }
}