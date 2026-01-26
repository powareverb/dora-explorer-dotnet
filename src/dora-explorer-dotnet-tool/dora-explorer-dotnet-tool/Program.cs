using System;
using DoraExplorer.Core;
using System.Linq;
using Refit;
using Polly;
using Polly.CircuitBreaker;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

namespace DoraExplorer.DotNetTool
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || args[0] != "pull-issues")
                {
                    PrintUsage();
                    Environment.Exit(1);
                }

                var options = ParseArguments(args);
                await ExecutePullIssues(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dora-explorer-dotnet pull-issues [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --jira-url <url>           Jira instance URL (e.g., https://your-instance.atlassian.net)");
            Console.WriteLine("  --jira-email <email>       Jira user email for authentication");
            Console.WriteLine("  --jira-api-key <key>       Jira API key for authentication");
            Console.WriteLine("  --project-keys <keys>      Comma-separated project keys (e.g., PROJ1,PROJ2)");
            Console.WriteLine("  --start-date <date>        Start date for issue search (YYYY-MM-DD)");
            Console.WriteLine("  --end-date <date>          End date for issue search (YYYY-MM-DD)");
            Console.WriteLine("  --cache-ttl-hours <hours>  Cache time-to-live in hours (default: 1)");
            Console.WriteLine("  --force-refresh            Bypass cache and fetch fresh data from Jira");
        }

        private static PullIssuesOptions ParseArguments(string[] args)
        {
            var options = new PullIssuesOptions();

            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("--") && i + 1 < args.Length)
                {
                    var value = args[i + 1];
                    switch (arg)
                    {
                        case "--jira-url":
                            options.JiraUrl = value;
                            i++;
                            break;
                        case "--jira-email":
                            options.JiraEmail = value;
                            i++;
                            break;
                        case "--jira-api-key":
                            options.JiraApiKey = value;
                            i++;
                            break;
                        case "--project-keys":
                            options.ProjectKeys = value.Split(',').Select(k => k.Trim()).ToList();
                            i++;
                            break;
                        case "--start-date":
                            options.StartDate = DateTime.Parse(value);
                            i++;
                            break;
                        case "--end-date":
                            options.EndDate = DateTime.Parse(value);
                            i++;
                            break;
                        case "--cache-ttl-hours":
                            if (int.TryParse(value, out var hours))
                                options.CacheTtlHours = hours;
                            i++;
                            break;
                    }
                }
                else if (arg == "--force-refresh")
                {
                    options.ForceRefresh = true;
                }
            }

            ValidateOptions(options);
            return options;
        }

        private static void ValidateOptions(PullIssuesOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.JiraUrl))
                throw new ArgumentException("--jira-url is required");
            if (string.IsNullOrWhiteSpace(options.JiraEmail))
                throw new ArgumentException("--jira-email is required");
            if (string.IsNullOrWhiteSpace(options.JiraApiKey))
                throw new ArgumentException("--jira-api-key is required");
            if (options.ProjectKeys == null || options.ProjectKeys.Count == 0)
                throw new ArgumentException("--project-keys is required");
            if (options.StartDate == default)
                throw new ArgumentException("--start-date is required");
            if (options.EndDate == default)
                throw new ArgumentException("--end-date is required");
            if (options.CacheTtlHours < 0)
                throw new ArgumentException("--cache-ttl-hours must be non-negative");
        }

        private static async Task ExecutePullIssues(PullIssuesOptions options)
        {
            // Create Jira API client with Basic Auth and retry policy
            var httpClient = new HttpClient(
                new BasicAuthHandler(options.JiraEmail, options.JiraApiKey)
                {
                    InnerHandler = new HttpClientHandler()
                })
            {
                BaseAddress = new Uri(options.JiraUrl)
            };

            // Add retry policy for transient failures
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount}: waiting {timespan.TotalSeconds}s before retry...");
                    });

            var jiraClient = RestService.For<IJiraApiClient>(httpClient);
            var cache = new IssueCache();

            var cacheTtl = TimeSpan.FromHours(options.CacheTtlHours);

            // Process each project
            var allIssues = new List<Issue>();
            foreach (var projectKey in options.ProjectKeys)
            {
                Console.WriteLine($"Processing project: {projectKey}");

                // Check cache first
                List<Issue>? cachedIssues = null;
                if (!options.ForceRefresh)
                {
                    cachedIssues = await cache.TryLoadAsync(projectKey, cacheTtl);
                    if (cachedIssues != null)
                    {
                        Console.WriteLine($"  ✓ Loaded {cachedIssues.Count} issues from cache");
                        allIssues.AddRange(cachedIssues);
                        continue;
                    }
                }

                // Fetch from Jira API
                Console.WriteLine($"  Fetching from Jira API...");
                var jql = BuildJql(projectKey, options.StartDate, options.EndDate);
                var issues = new List<Issue>();
                int startAt = 0;
                const int pageSize = 50;

                while (true)
                {
                    var response = await retryPolicy.ExecuteAsync(
                        () => jiraClient.SearchIssuesAsync(
                            jql,
                            "*all",
                            pageSize,
                            startAt));

                    if (response.Issues != null)
                    {
                        foreach (var jiraIssue in response.Issues)
                        {
                            issues.Add(MapJiraIssueToIssue(jiraIssue));
                        }
                    }

                    if (response.Issues == null || response.Issues.Count < pageSize)
                        break;

                    startAt += pageSize;
                }

                Console.WriteLine($"  ✓ Fetched {issues.Count} issues from Jira API");

                // Save to cache
                await cache.SaveAsync(projectKey, issues);
                allIssues.AddRange(issues);
            }

            Console.WriteLine();
            Console.WriteLine($"Summary: Processed {options.ProjectKeys.Count} project(s), {allIssues.Count} total issues");
        }

        private static string BuildJql(string projectKey, DateTime startDate, DateTime endDate)
        {
            return $"project = {projectKey} AND created >= {startDate:yyyy-MM-dd} AND created <= {endDate:yyyy-MM-dd}";
        }

        private static Issue MapJiraIssueToIssue(JiraIssue jiraIssue)
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

        private class PullIssuesOptions
        {
            public string? JiraUrl { get; set; }
            public string? JiraEmail { get; set; }
            public string? JiraApiKey { get; set; }
            public List<string> ProjectKeys { get; set; } = new();
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int CacheTtlHours { get; set; } = 1;
            public bool ForceRefresh { get; set; }
        }
    }
}
