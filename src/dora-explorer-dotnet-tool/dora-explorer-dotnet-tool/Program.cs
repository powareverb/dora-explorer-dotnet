using System;
using System.Threading.Tasks;

namespace DoraExplorer.DotNetTool;

/// <summary>
/// CLI entry point for Dora Explorer dotnet tool
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Exit code</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await ExecuteAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled error: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> ExecuteAsync(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0];

        return command switch
        {
            "pull-issues" => await ExecutePullIssuesAsync(args),
            _ => HandleUnknownCommand(command)
        };
    }

    private static async Task<int> ExecutePullIssuesAsync(string[] args)
    {
        try
        {
            var parser = new ArgumentParser();
            var options = parser.Parse(args);

            var handler = new PullIssuesCommandHandler(options);
            return await handler.ExecuteAsync();
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid arguments: {ex.Message}");
            PrintUsage();
            return 1;
        }
    }

    private static int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Dora Explorer - DORA Metrics Calculator");
        Console.WriteLine();
        Console.WriteLine("Usage: dora-explorer-dotnet pull-issues [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  pull-issues  Fetch issues from Jira");
        Console.WriteLine();
        Console.WriteLine("Options (for pull-issues):");
        Console.WriteLine("  --jira-url <url>           Jira instance URL (required)");
        Console.WriteLine("  --jira-email <email>       Jira user email (required)");
        Console.WriteLine("  --jira-api-key <key>       Jira API key (required)");
        Console.WriteLine("  --project-keys <keys>      Comma-separated project keys (required)");
        Console.WriteLine("  --start-date <date>        Start date YYYY-MM-DD (required)");
        Console.WriteLine("  --end-date <date>          End date YYYY-MM-DD (required)");
        Console.WriteLine("  --cache-ttl-hours <hours>  Cache TTL in hours (default: 1)");
        Console.WriteLine("  --force-refresh            Bypass cache and fetch from Jira");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dora-explorer-dotnet pull-issues \\");
        Console.WriteLine("    --jira-url https://your-instance.atlassian.net \\");
        Console.WriteLine("    --jira-email user@example.com \\");
        Console.WriteLine("    --jira-api-key your-api-key \\");
        Console.WriteLine("    --project-keys PROJ1,PROJ2 \\");
        Console.WriteLine("    --start-date 2025-01-01 \\");
        Console.WriteLine("    --end-date 2026-01-31");
    }
}
