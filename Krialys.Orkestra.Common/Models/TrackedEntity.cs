namespace Krialys.Orkestra.Common.Models;

/// <summary>
/// Tracked Entity
/// </summary>
public class TrackedEntity : IEquatable<TrackedEntity>
{
    /// <summary>
    /// Hash key representing the content of the TrackedEntity
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Entity type fullname, else is equivalent to CpuName(Logger)
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// UTC datetime stamp
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Action type: insert/update/patch/delete(CRUD) /error/warning(Logger)
    /// </summary>
    public string Action { get; init; }

    /// <summary>
    /// User Id who ordered the tracking, else is equivalent to CpuId(Logger)
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// Dedicated to Uuid signature, Any means receiving serialized Base64 data(Logger)
    /// </summary>
    public string UuidOrAny { get; init; }

    public bool Equals(TrackedEntity other)
    {
        if (other == null)
            return false;

        return FullName == other.FullName
            && Date == other.Date
            && Action == other.Action
            && UserId == other.UserId
            && UuidOrAny == other.UuidOrAny;
    }

    public override int GetHashCode()
        => HashCode.Combine(FullName, Date, Action, UserId, UuidOrAny);
}