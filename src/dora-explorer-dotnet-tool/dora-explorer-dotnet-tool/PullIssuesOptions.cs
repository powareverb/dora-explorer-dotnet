using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoraExplorer.Core;

namespace DoraExplorer.DotNetTool;

/// <summary>
/// Options for the pull-issues command
/// </summary>
public class PullIssuesOptions
{
    /// <summary>
    /// Jira instance URL (e.g., https://your-instance.atlassian.net)
    /// </summary>
    public string? JiraUrl { get; set; }

    /// <summary>
    /// Jira user email for Basic Auth authentication
    /// </summary>
    public string? JiraEmail { get; set; }

    /// <summary>
    /// Jira API key for authentication
    /// </summary>
    public string? JiraApiKey { get; set; }

    /// <summary>
    /// Project keys to fetch issues from
    /// </summary>
    public List<string> ProjectKeys { get; set; } = new();

    /// <summary>
    /// Start date for issue search
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for issue search
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Cache time-to-live in hours (default: 1)
    /// </summary>
    public int CacheTtlHours { get; set; } = 1;

    /// <summary>
    /// Force refresh from Jira API, bypassing cache
    /// </summary>
    public bool ForceRefresh { get; set; }
}
