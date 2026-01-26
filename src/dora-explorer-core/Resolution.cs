namespace DoraExplorer.Core;

/// <summary>
/// Represents the resolution state of a Jira issue
/// </summary>
public class Resolution
{
    /// <summary>
    /// Resolution identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Resolution name (Fixed, Won't Fix, Duplicate, Won't Do, Done, etc.)
    /// </summary>
    public string? Name { get; set; }
}
