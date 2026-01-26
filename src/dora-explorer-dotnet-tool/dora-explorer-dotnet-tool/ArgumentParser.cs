namespace DoraExplorer.DotNetTool;

/// <summary>
/// Parses command-line arguments for the pull-issues command
/// </summary>
public class ArgumentParser
{
    /// <summary>
    /// Parses command-line arguments into PullIssuesOptions
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Parsed options</returns>
    /// <exception cref="ArgumentException">Thrown when arguments are invalid</exception>
    public PullIssuesOptions Parse(string[] args)
    {
        if (args is null || args.Length == 0)
            throw new ArgumentException("No arguments provided");

        var options = new PullIssuesOptions
        {
            ProjectKeys = new List<string>()
        };

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg == "--force-refresh")
            {
                options.ForceRefresh = true;
                continue;
            }

            if (!arg.StartsWith("--") || i + 1 >= args.Length)
                continue;

            var value = args[i + 1];
            i++; // Skip next iteration since we consumed the value

            switch (arg)
            {
                case "--jira-url":
                    options.JiraUrl = value;
                    break;
                case "--jira-email":
                    options.JiraEmail = value;
                    break;
                case "--jira-api-key":
                    options.JiraApiKey = value;
                    break;
                case "--project-keys":
                    options.ProjectKeys = value.Split(',').Select(k => k.Trim()).ToList();
                    break;
                case "--start-date":
                    if (DateTime.TryParse(value, out var startDate))
                        options.StartDate = startDate;
                    break;
                case "--end-date":
                    if (DateTime.TryParse(value, out var endDate))
                        options.EndDate = endDate;
                    break;
                case "--cache-ttl-hours":
                    if (int.TryParse(value, out var hours))
                        options.CacheTtlHours = hours;
                    break;
            }
        }

        return options;
    }
}
