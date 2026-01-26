namespace DoraExplorer.Core;

/// <summary>
/// Represents a Jira user (assignee, reporter, etc.)
/// </summary>
public class User
{
    /// <summary>
    /// Unique user identifier (email or account ID)
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// User's display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string? EmailAddress { get; set; }
}
