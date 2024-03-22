namespace Krialys.Orkestra.Common.Models.Email;

/// <summary>
/// An enumeration of message importance values.
/// </summary>
public enum Importance
{
    Low,
    Normal,
    High
}

/// <summary>
/// Indicates the priority of a message.
/// </summary>
public enum Priority
{
    Highest = 1,
    High,
    Normal,
    Low,
    Lowest
}

/// <summary>
/// EmailTemplate
/// </summary>
public class EmailTemplate
{
    /// <summary>
    /// Must be unique email address
    /// </summary>
    public string From { get; init; }

    /// <summary>
    /// Can be 1 or more recipients addresses, separated by ;
    /// </summary>
    public string To { get; init; }

    /// <summary>
    /// Email title
    /// </summary>
    public string Subject { get; init; }

    /// <summary>
    /// Email content
    /// </summary>
    public string Body { get; init; }

    /// <summary>
    /// Email content type (HTML is the default, else will be sent in plain text)
    /// </summary>
    public bool TextFormatHtml { get; init; } = true;

    /// <summary>
    /// Importance flag
    /// </summary>
    public Importance Importance { get; init; } = Importance.Normal;

    /// <summary>
    /// Priority flag
    /// </summary>
    public Priority Priority { get; init; } = Priority.Normal;

    /// <summary>
    /// Culture
    /// </summary>
    public string CultureInfo { get; set; }

    /// <summary>
    /// Timezone
    /// </summary>
    public string TimeZone { get; set; }
}