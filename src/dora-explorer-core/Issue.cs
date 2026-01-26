namespace DoraExplorer.Core;

/// <summary>
/// Represents a Jira issue with all fields required for DORA metrics calculation
/// </summary>
public class Issue
{
    /// <summary>
    /// Issue key (e.g., "PROJ-123")
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Issue ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Issue summary/title
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Issue type (Bug, Story, Task, Epic, Sub-task)
    /// </summary>
    public IssueType? IssueType { get; set; }

    /// <summary>
    /// Current status (Open, In Progress, Done, Closed, etc.)
    /// </summary>
    public Status? Status { get; set; }

    /// <summary>
    /// DateTime when issue was created
    /// </summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>
    /// DateTime when issue was last modified
    /// </summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// DateTime when issue was resolved/closed, nullable if not resolved
    /// </summary>
    public DateTimeOffset? Resolved { get; set; }

    /// <summary>
    /// Resolution (Fixed, Won't Fix, Duplicate, etc.), nullable if not resolved
    /// </summary>
    public Resolution? Resolution { get; set; }

    /// <summary>
    /// Labels/tags associated with the issue
    /// </summary>
    public List<string>? Labels { get; set; }

    /// <summary>
    /// Components this issue affects
    /// </summary>
    public List<Component>? Components { get; set; }

    /// <summary>
    /// Sprint assignment, nullable if not in a sprint
    /// </summary>
    public Sprint? Sprint { get; set; }

    /// <summary>
    /// Target fix versions
    /// </summary>
    public List<Version>? FixVersions { get; set; }

    /// <summary>
    /// Affected versions
    /// </summary>
    public List<Version>? AffectedVersions { get; set; }

    /// <summary>
    /// Priority level
    /// </summary>
    public Priority? Priority { get; set; }

    /// <summary>
    /// Assigned user, nullable if unassigned
    /// </summary>
    public User? Assignee { get; set; }

    /// <summary>
    /// User who reported the issue
    /// </summary>
    public User? Reporter { get; set; }

    /// <summary>
    /// Links to related issues
    /// </summary>
    public List<IssueLink>? IssueLinks { get; set; }

    /// <summary>
    /// Description/details of the issue
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Represents a Jira component
/// </summary>
public class Component
{
    /// <summary>
    /// Component identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Component name
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Represents a sprint in an agile board
/// </summary>
public class Sprint
{
    /// <summary>
    /// Sprint identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Sprint name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Sprint state (Active, Future, Closed)
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Sprint start date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Sprint end date
    /// </summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Represents a version/release in Jira
/// </summary>
public class Version
{
    /// <summary>
    /// Version identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Version name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Release date
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Whether this version is released
    /// </summary>
    public bool Released { get; set; }
}

/// <summary>
/// Represents a priority level
/// </summary>
public class Priority
{
    /// <summary>
    /// Priority identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Priority name (Lowest, Low, Medium, High, Highest)
    /// </summary>
    public string? Name { get; set; }
}
