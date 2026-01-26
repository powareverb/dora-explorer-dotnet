namespace DoraExplorer.Core;

/// <summary>
/// Represents a Jira issue type (Bug, Story, Task, etc.)
/// </summary>
public class IssueType
{
    /// <summary>
    /// Issue type identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Issue type name (Bug, Story, Task, Epic, Sub-task)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether this is a subtask type
    /// </summary>
    public bool Subtask { get; set; }
}
