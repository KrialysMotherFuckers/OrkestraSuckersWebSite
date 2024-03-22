namespace Krialys.Orkestra.Web.Module.Common.BusEvents;

/// <summary>
/// Manage Disabled flag for any boolean switch that want to use this event in combination with SfTab
/// </summary>
public class SfTabBusEvent
{
    /// <summary>
    /// Mark SfTab as Disabled when set to true
    /// </summary>
    public bool Disabled { get; init; }
}
