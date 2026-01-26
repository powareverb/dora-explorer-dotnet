
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoraExplorer.DotNetTool;

/// <summary>
/// Handles the pull-issues command for fetching Jira issues with caching
/// </summary>
public class PullIssuesCommandHandler
{
    private const int PageSize = 50;
    private const int MaxRetries = 3;

    private readonly PullIssuesOptions _options;

    /// <summary>
    /// Creates a new instance of PullIssuesCommandHandler
    /// </summary>
    /// <param name="options">Command options</param>
    public PullIssuesCommandHandler(PullIssuesOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions();
    }

    /// <summary>
    /// Executes the pull-issues command
    /// </summary>
    public async Task<int> ExecuteAsync()
    {
        try
        {
            using var httpClient = CreateHttpClient();
            var jiraClient = RestService.For<IJiraApiClient>(httpClient, new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new DateTimeConverterUsingDateTimeParse(),
                        new DateTimeOffsetConverterUsingDateTimeParse()
                    }
                })
            });
            var cache = new IssueCache();

            var cacheTtl = TimeSpan.FromHours(_options.CacheTtlHours);
            var allIssues = new List<Issue>();

            foreach (var projectKey in _options.ProjectKeys)
            {
                Console.WriteLine($"Processing project: {projectKey}");
                var projectIssues = await FetchProjectIssuesAsync(jiraClient, cache, projectKey, cacheTtl);
                allIssues.AddRange(projectIssues);
            }

            PrintSummary(allIssues);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new BasicAuthHandler(_options.JiraEmail, _options.JiraApiKey)
        {
            InnerHandler = new HttpClientHandler()
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(_options.JiraUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private async Task<List<Issue>> FetchProjectIssuesAsync(
        IJiraApiClient jiraClient,
        IssueCache cache,
        string projectKey,
        TimeSpan cacheTtl)
    {
        var issues = new List<Issue>();

        // Check cache first
        if (!_options.ForceRefresh)
        {
            var cachedIssues = await cache.TryLoadAsync(projectKey, cacheTtl);
            if (cachedIssues is not null)
            {
                Console.WriteLine($"  ✓ Loaded {cachedIssues.Count} issues from cache");
                return cachedIssues;
            }
        }

        // Fetch from Jira API
        Console.WriteLine("  Fetching from Jira API...");
        var jql = BuildJql(projectKey);
        int startAt = 0;

        while (true)
        {
            var response = await FetchIssuesPageAsync(jiraClient, jql, startAt);

            if (response.Issues is not null)
            {
                issues.AddRange(response.Issues.Select(MapJiraIssueToIssue));
            }

            if (response.Issues is null || response.Issues.Count < PageSize)
                break;

            startAt += PageSize;
        }

        Console.WriteLine($"  ✓ Fetched {issues.Count} issues from Jira API");

        // Save to cache
        await cache.SaveAsync(projectKey, issues);
        return issues;
    }

    private async Task<JiraSearchResponse> FetchIssuesPageAsync(
        IJiraApiClient jiraClient,
        string jql,
        int startAt)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: MaxRetries,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"  Retry {retryCount}: waiting {timespan.TotalSeconds}s before retry...");
                });

        return (await retryPolicy.ExecuteAsync(
            () => jiraClient.SearchIssuesAsync(jql, "*all", PageSize, startAt)));
    }

    private string BuildJql(string projectKey)
    {
        return $"project = {projectKey} AND created >= {_options.StartDate:yyyy-MM-dd} AND created <= {_options.EndDate:yyyy-MM-dd}";
    }

    private Issue MapJiraIssueToIssue(JiraIssue jiraIssue)
    {
        var fields = jiraIssue.Fields ?? new IssueFields();
        return new Issue
        {
            Key = jiraIssue.Key,
            Id = jiraIssue.Id,
            Summary = fields.Summary,
            Description = fields.Description,
            IssueType = fields.IssueType,
            Status = fields.Status,
            Created = fields.Created,
            Updated = fields.Updated,
            Resolved = fields.Resolvable,
            Resolution = fields.Resolution,
            Labels = fields.Labels,
            Components = fields.Components,
            Sprint = fields.Sprint?.FirstOrDefault(),
            FixVersions = fields.FixVersions,
            AffectedVersions = fields.Versions,
            Priority = fields.Priority,
            Assignee = fields.Assignee,
            Reporter = fields.Reporter,
            IssueLinks = fields.IssueLinks
        };
    }

    private void PrintSummary(List<Issue> allIssues)
    {
        Console.WriteLine();
        Console.WriteLine($"Summary: Processed {_options.ProjectKeys.Count} project(s), {allIssues.Count} total issues");
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.JiraUrl))
            throw new ArgumentException("Jira URL is required", nameof(_options.JiraUrl));
        if (string.IsNullOrWhiteSpace(_options.JiraEmail))
            throw new ArgumentException("Jira email is required", nameof(_options.JiraEmail));
        if (string.IsNullOrWhiteSpace(_options.JiraApiKey))
            throw new ArgumentException("Jira API key is required", nameof(_options.JiraApiKey));
        if (_options.ProjectKeys is null || _options.ProjectKeys.Count == 0)
            throw new ArgumentException("At least one project key is required", nameof(_options.ProjectKeys));
        if (_options.StartDate == default)
            throw new ArgumentException("Start date is required", nameof(_options.StartDate));
        if (_options.EndDate == default)
            throw new ArgumentException("End date is required", nameof(_options.EndDate));
        if (_options.CacheTtlHours < 0)
            throw new ArgumentException("Cache TTL must be non-negative", nameof(_options.CacheTtlHours));
    }
}
