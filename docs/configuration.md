# Configuration and Secrets Management

## Overview

Dora Explorer supports multiple configuration sources with a clear priority hierarchy:

1. **Command-line arguments** (highest priority - always override other sources)
2. **User Secrets** (for sensitive data like API keys)
3. **appsettings.json** (for development settings)
4. **appsettings.{Environment}.json** (environment-specific overrides)
5. **Environment variables** (lowest priority)

## Configuration Sources

### appsettings.json

Create an `appsettings.json` file in your project directory to set default values:

```json
{
  "Jira": {
    "Url": "https://your-instance.atlassian.net",
    "Email": "user@example.com",
    "ProjectKeys": [
      "PROJ1",
      "PROJ2"
    ]
  },
  "Search": {
    "StartDate": "2025-01-01",
    "EndDate": "2026-01-31"
  },
  "Cache": {
    "TtlHours": 1
  }
}
```

### User Secrets (Recommended for API Keys)

Store sensitive information like your Jira API key in User Secrets instead of plain text files:

```bash
# Initialize User Secrets (first time only)
dotnet user-secrets init

# Set your Jira API key
dotnet user-secrets set "Jira:ApiKey" "your-api-key-here"

# View all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "Jira:ApiKey"
```

User Secrets are stored in a file local to your user profile and are never checked into source control.

### Environment Variables

Set configuration via environment variables (useful in CI/CD):

```bash
# PowerShell
$env:Jira__Url = "https://your-instance.atlassian.net"
$env:Jira__Email = "user@example.com"
$env:Jira__ApiKey = "your-api-key"
$env:Jira__ProjectKeys__0 = "PROJ1"
$env:Jira__ProjectKeys__1 = "PROJ2"
$env:Search__StartDate = "2025-01-01"
$env:Search__EndDate = "2026-01-31"

# Then run the tool
dora-explorer-dotnet pull-issues
```

### Command-Line Arguments

Command-line arguments override all other configuration sources:

```bash
dora-explorer-dotnet pull-issues \
  --jira-url https://your-instance.atlassian.net \
  --jira-email user@example.com \
  --jira-api-key your-api-key \
  --project-keys PROJ1,PROJ2 \
  --start-date 2025-01-01 \
  --end-date 2026-01-31 \
  --cache-ttl-hours 2 \
  --force-refresh
```

## Configuration Keys

| Setting | Config Key | Command-Line | Default | Required |
|---------|-----------|--------------|---------|----------|
| Jira URL | `Jira:Url` | `--jira-url` | - | Yes |
| Jira Email | `Jira:Email` | `--jira-email` | - | Yes |
| Jira API Key | `Jira:ApiKey` | `--jira-api-key` | - | Yes |
| Project Keys | `Jira:ProjectKeys` | `--project-keys` | - | Yes |
| Start Date | `Search:StartDate` | `--start-date` | - | Yes |
| End Date | `Search:EndDate` | `--end-date` | - | Yes |
| Cache TTL (hours) | `Cache:TtlHours` | `--cache-ttl-hours` | 1 | No |
| Force Refresh | - | `--force-refresh` | false | No |

## Best Practices

1. **Never hardcode secrets in appsettings.json** - Use User Secrets or environment variables instead
2. **Use User Secrets in development** - Run `dotnet user-secrets set "Jira:ApiKey" "..."` before testing
3. **Use environment variables in CI/CD** - Set variables in your pipeline configuration
4. **Override only what you need** - Use configuration files for defaults, command-line args for specific runs
5. **Keep appsettings.json in source control** - This allows all developers to have consistent defaults

## Example Workflow

### Development Setup

```bash
# 1. Create appsettings.json with non-sensitive settings
# (already provided in the project)

# 2. Set API key in User Secrets
dotnet user-secrets set "Jira:ApiKey" "your-personal-api-key"

# 3. Run the tool with minimal command-line arguments
dora-explorer-dotnet pull-issues \
  --project-keys PROJ1 \
  --start-date 2025-01-01 \
  --end-date 2026-01-31
```

### CI/CD Pipeline Setup

```yaml
# Example GitHub Actions workflow
- name: Run Dora Explorer
  env:
    Jira__Url: ${{ secrets.JIRA_URL }}
    Jira__Email: ${{ secrets.JIRA_EMAIL }}
    Jira__ApiKey: ${{ secrets.JIRA_API_KEY }}
    Jira__ProjectKeys__0: "PROJ1"
    Jira__ProjectKeys__1: "PROJ2"
  run: |
    dora-explorer-dotnet pull-issues \
      --start-date 2025-01-01 \
      --end-date 2026-01-31
```

## Troubleshooting

**Problem**: "ApiKey is null"
**Solution**: Ensure you've set `Jira:ApiKey` in User Secrets or passed `--jira-api-key`

**Problem**: Configuration not loading
**Solution**: Verify `appsettings.json` exists and is valid JSON; check environment variable names use `__` not `:`

**Problem**: Settings being overridden unexpectedly
**Solution**: Remember command-line args have highest priority; remove conflicting settings from other sources
