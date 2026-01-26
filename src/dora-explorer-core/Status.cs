namespace DoraExplorer.Core;

/// <summary>
/// Represents the status of a Jira issue
/// </summary>
public class Status
{
    /// <summary>
    /// Status identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Status name (Open, In Progress, In Review, Done, Closed, etc.)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Status category (Todo, In Progress, Done)
    /// </summary>
    public StatusCategory? StatusCategory { get; set; }
}

/// <summary>
/// Represents a Jira status category
/// </summary>
public class StatusCategory
{
    /// <summary>
    /// Category key (new, indeterminate, done)
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string? Name { get; set; }
}
