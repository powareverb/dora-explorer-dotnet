namespace DoraExplorer.DotNetTool;

/// <summary>
/// Parses command-line arguments for the pull-issues command
/// </summary>
public class ArgumentParser
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of ArgumentParser
    /// </summary>
    /// <param name="configuration">Configuration provider for reading settings from appsettings.json and User Secrets</param>
    public ArgumentParser(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Parses command-line arguments and merges with configuration settings
    /// Command-line arguments take precedence over configuration values.
    /// Jira API key is loaded from User Secrets if not provided via command-line.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Parsed and configured options</returns>
    /// <exception cref="ArgumentException">Thrown when arguments are invalid</exception>
    public PullIssuesOptions Parse(string[] args)
    {
        if (args is null || args.Length == 0)
            throw new ArgumentException("No arguments provided");

        // Load defaults from configuration
        var options = new PullIssuesOptions
        {
            JiraUrl = _configuration["Jira:Url"],
            JiraEmail = _configuration["Jira:Email"],
            JiraApiKey = _configuration["Jira:ApiKey"], // Load from User Secrets
            ProjectKeys = _configuration.GetSection("Jira:ProjectKeys").Get<List<string>>() ?? new(),
            CacheTtlHours = int.TryParse(_configuration["Cache:TtlHours"], out var hours) ? hours : 1
        };

        // Parse dates from config if present
        if (DateTime.TryParse(_configuration["Search:StartDate"], out var configStartDate))
            options.StartDate = configStartDate;
        if (DateTime.TryParse(_configuration["Search:EndDate"], out var configEndDate))
            options.EndDate = configEndDate;

        // Override with command-line arguments
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
                    if (int.TryParse(value, out var cacheTtl))
                        options.CacheTtlHours = cacheTtl;
                    break;
            }
        }

        return options;
    }
}
