namespace DoraExplorer.Core;

/// <summary>
/// Represents a link between Jira issues
/// </summary>
public class IssueLink
{
    /// <summary>
    /// Link identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Type of relationship (blocks, relates to, duplicates, etc.)
    /// </summary>
    public LinkType? Type { get; set; }

    /// <summary>
    /// The linked issue
    /// </summary>
    public LinkedIssue? InwardIssue { get; set; }

    /// <summary>
    /// The linked issue (outward direction)
    /// </summary>
    public LinkedIssue? OutwardIssue { get; set; }
}

/// <summary>
/// Represents the type of a link between issues
/// </summary>
public class LinkType
{
    /// <summary>
    /// Link type identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Link type name (e.g., "relates to", "blocks")
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Inward link description
    /// </summary>
    public string? InwardName { get; set; }

    /// <summary>
    /// Outward link description
    /// </summary>
    public string? OutwardName { get; set; }
}

/// <summary>
/// Represents a minimal issue reference in a link
/// </summary>
public class LinkedIssue
{
    /// <summary>
    /// Issue key (e.g., "PROJ-123")
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Issue summary
    /// </summary>
    public string? Summary { get; set; }
}
