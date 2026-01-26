namespace DoraExplorer.Core;

/// <summary>
/// Refit HTTP client interface for Jira REST API v3
/// Handles communication with Jira Cloud instances using Basic Authentication
/// </summary>
public interface IJiraApiClient
{
    /// <summary>
    /// Search for issues using JQL with pagination
    /// </summary>
    /// <param name="jql">JQL query string (e.g., "project = PROJ AND created >= -30d")</param>
    /// <param name="fields">Comma-separated field names to retrieve. Use "*all" for all fields</param>
    /// <param name="maxResults">Maximum results per request (default: 50, max: 100)</param>
    /// <param name="startAt">Starting index for pagination (default: 0)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search response containing issues and pagination info</returns>
    /// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-get
    [Get("/rest/api/3/search/jql?jql={jql}&fields={fields}&maxResults={maxResults}&startAt={startAt}")]
    Task<JiraSearchResponse> SearchIssuesAsync(
        string jql,
        string fields = "*all",
        int maxResults = 50,
        int startAt = 0,
        CancellationToken ct = default);

    /// <summary>
    /// Search for issues using JQL with pagination
    /// </summary>
    /// <param name="jql">JQL query string (e.g., "project = PROJ AND created >= -30d")</param>
    /// <param name="fields">Comma-separated field names to retrieve. Use "*all" for all fields</param>
    /// <param name="maxResults">Maximum results per request (default: 50, max: 100)</param>
    /// <param name="startAt">Starting index for pagination (default: 0)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search response containing issues and pagination info</returns>
    /// https://developer.atlassian.com/cloud/jira/platform/rest/v3/api-group-issue-search/#api-rest-api-3-search-jql-get
    [Get("/rest/api/3/search/jql?jql={jql}&fields={fields}&maxResults={maxResults}&startAt={startAt}")]
    Task<ApiResponse<JiraSearchResponse>> SearchIssuesWithDiagnosisAsync(
        string jql,
        string fields = "*all",
        int maxResults = 50,
        int startAt = 0,
        CancellationToken ct = default);

    /// <summary>
    /// Get list of all projects accessible to the current user
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of projects</returns>
    [Get("/rest/api/3/project/search")]
    Task<JiraProjectResponse> GetProjectsAsync(CancellationToken ct = default);
}

/// <summary>
/// Jira API search response envelope
/// </summary>
public class JiraSearchResponse
{
    /// <summary>
    /// Starting index of results
    /// </summary>
    public int StartAt { get; set; }

    /// <summary>
    /// Number of results returned
    /// </summary>
    public int MaxResults { get; set; }

    /// <summary>
    /// Total number of matching issues (may exceed returned results)
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// List of issues returned in this page
    /// </summary>
    public List<JiraIssue>? Issues { get; set; }
}

/// <summary>
/// Jira issue as returned by the API (maps to DoraExplorer.Core.Issue)
/// </summary>
public class JiraIssue
{
    /// <summary>
    /// Issue identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Issue key
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Issue fields
    /// </summary>
    public IssueFields? Fields { get; set; }
}

/// <summary>
/// Container for Jira issue fields
/// </summary>
public class IssueFields
{
    /// <summary>
    /// Issue summary
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Issue description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Issue type
    /// </summary>
    public IssueType? IssueType { get; set; }

    /// <summary>
    /// Current status
    /// </summary>
    public Status? Status { get; set; }

    /// <summary>
    /// Created timestamp (ISO 8601)
    /// </summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>
    /// Updated timestamp (ISO 8601)
    /// </summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// Resolved timestamp (ISO 8601), nullable
    /// </summary>
    public DateTimeOffset? Resolvable { get; set; }

    /// <summary>
    /// Resolution
    /// </summary>
    public Resolution? Resolution { get; set; }

    /// <summary>
    /// Issue labels
    /// </summary>
    public List<string>? Labels { get; set; }

    /// <summary>
    /// Components
    /// </summary>
    public List<Component>? Components { get; set; }

    /// <summary>
    /// Sprint (from agile board)
    /// </summary>
    public List<Sprint>? Sprint { get; set; }

    /// <summary>
    /// Fix versions
    /// </summary>
    public List<Version>? FixVersions { get; set; }

    /// <summary>
    /// Affected versions
    /// </summary>
    public List<Version>? Versions { get; set; }

    /// <summary>
    /// Priority
    /// </summary>
    public Priority? Priority { get; set; }

    /// <summary>
    /// Assignee user
    /// </summary>
    public User? Assignee { get; set; }

    /// <summary>
    /// Reporter user
    /// </summary>
    public User? Reporter { get; set; }

    /// <summary>
    /// Issue links
    /// </summary>
    public List<IssueLink>? IssueLinks { get; set; }
}

/// <summary>
/// Jira project response envelope
/// </summary>
public class JiraProjectResponse
{
    /// <summary>
    /// List of projects
    /// </summary>
    public List<JiraProject>? Values { get; set; }
}

/// <summary>
/// Jira project
/// </summary>
public class JiraProject
{
    /// <summary>
    /// Project key (e.g., "PROJ")
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Project identifier
    /// </summary>
    public string? Id { get; set; }
}
