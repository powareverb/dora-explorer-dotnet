# Feature 1: Pull Issues from Jira

## Overview

Establish the data foundation by building a Refit-based Jira API client and ETag-driven local cache layer. This feature fetches issue metadata from Jira, stores it locally with intelligent caching, and provides the data models that all downstream features (metrics calculation, exports) depend on.

## Requirements

### Authentication

- **Method**: HTTP Basic Auth (email + Jira API key)
- **Implementation**: Base64-encoded `Authorization: Basic` header in Refit client
- **CLI Arguments**: `--jira-url`, `--jira-email`, `--jira-api-key`

### Data Models

Issue model must capture all fields required for DORA metrics calculation:

- `Key` - Issue identifier (e.g., "PROJ-123")
- `Summary` - Issue title/description
- `IssueType` - Type (Bug, Story, Task, etc.)
- `Status` - Current status (Open, In Progress, Done, etc.)
- `Created` - DateTime when issue was created
- `Updated` - DateTime when issue was last modified
- `Resolved` - DateTime when issue was resolved/closed
- `Resolution` - Resolution type (Fixed, Won't Fix, Duplicate, etc.)
- `Labels` - Array of tags/labels for categorization
- `Components` - Array of component names
- `Sprint` - Sprint assignment (if using Jira board)
- `FixVersions` - Array of target release versions
- `AffectedVersions` - Array of affected versions
- `Priority` - Priority level
- `Assignee` - Assignee name/email
- `Reporter` - Reporter name/email
- `Links` - Related issue links (for cross-issue relationships)

### Caching Strategy

- **Storage**: Local filesystem-based cache (JSON files in `.dora-cache/` directory)
- **Cache Key**: Jira project key + date range combination
- **Invalidation Method**:
  - User-specified cache period via `--cache-ttl-hours` CLI argument (integer, default: 1 hour)
  - ETag-based conditional requests to Jira API (avoid re-downloading unchanged issues)
  - Manual refresh option via `--force-refresh` flag to bypass cache
- **Stored Data**: Full issue JSON + Last-Modified timestamp + ETag value

### Jira API Integration

- **Client**: Refit HTTP interface (`IJiraApiClient`) in `dora-explorer-core`
- **Endpoints**:
  - `GET /rest/api/3/search` - Fetch issues with JQL filtering (project, date range, etc.)
  - `GET /rest/api/3/projects` - List available projects (validation)
  - Query parameters: `jql`, `fields`, `maxResults`, `startAt` (pagination)
- **Error Handling**: Retry logic for transient failures; clear error messages for auth/permission issues
- **Rate Limiting**: Respect Jira's rate limit headers (`X-RateLimit-*`)

### CLI Arguments

```sh
dora-explorer-dotnet pull-issues
--jira-url https://your-instance.atlassian.net
--jira-email user@example.com
--jira-api-key <api-key>
--project-keys PROJ1,PROJ2
--start-date 2025-01-01
--end-date 2026-01-31
--cache-ttl-hours 1
--force-refresh
```

## Implementation Steps

### Step 1: Data Models (dora-explorer-core)

- Create `Issue.cs` with all fields listed above
- Create supporting models: `IssueType.cs`, `Status.cs`, `Resolution.cs`, `User.cs`, `IssueLink.cs`
- Use file-scoped namespaces and nullable annotations (`#nullable enable`)
- Document fields with XML comments explaining DORA relevance

### Step 2: Refit API Client (dora-explorer-core)

- Create `IJiraApiClient.cs` interface with methods:
  - `SearchIssuesAsync(string jql, int? maxResults, int? startAt)` → `List<Issue>`
  - `GetProjectsAsync()` → `List<Project>`
- Implement Basic Auth header factory
- Add retry policy via Polly for transient failures
- Configure to deserialize Jira's flat response format into nested models

### Step 3: Cache Layer (dora-explorer-core)

- Create `IssueCache.cs` class with methods:
  - `SaveAsync(string projectKey, List<Issue> issues, CancellationToken ct)` - Store to disk with timestamp
  - `TryLoadAsync(string projectKey, TimeSpan cacheTtl, CancellationToken ct)` - Load if not expired
  - `InvalidateAsync(string projectKey)` - Manual clear
- Cache directory: `~/.dora-explorer/cache/` (platform-aware paths)
- Serialize/deserialize with `System.Text.Json`

### Step 4: CLI Entry Point (dora-explorer-dotnet-tool)

- Update [Program.cs](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool/Program.cs) to parse `pull-issues` subcommand
- Instantiate Refit client + cache layer with user-provided config
- Call cache first; if miss/expired, fetch from Jira API
- Output fetched issues count + cache status

### Step 5: Integration Tests (dora-explorer-dotnet-tool.MSTest)

- Add tests in [IntegrationTests.cs](src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool.MSTest/IntegrationTests.cs):
  - Jira auth failure (401)
  - Cache hit within TTL (no API call)
  - Cache expiration after TTL
  - `--force-refresh` bypasses cache
  - Data model mapping completeness
- Mock Jira API responses with test data

### Step 6: Functional Tests (dora-explorer-dotnet-tool.MSTest)

- Test end-to-end CLI with valid credentials
- Verify `.dora-cache/` directory created with expected JSON files
- Verify CLI exits with code 0 on success

## Success Criteria

- ✅ Jira API client successfully authenticates using Basic Auth
- ✅ All issue model fields populated from Jira API response
- ✅ Cache stores issues locally; subsequent calls use cached data within TTL
- ✅ Cache invalidates after specified TTL or on `--force-refresh`
- ✅ CLI reports cache status and issue count on output
- ✅ Unit and integration tests pass with ≥85% code coverage
- ✅ Error messages clear for auth failures, network issues, permission denied

## Dependencies & Assumptions

- Jira Cloud REST API v3 available (not Server/Data Center)
- User has valid Jira API key (not legacy password)
- Internet connectivity to Jira instance
- Local filesystem writable for `.dora-cache/` directory

## Notes

- Initial version focuses on single project or comma-separated list; org-wide DORA dashboard deferred to future feature
- Cache TTL sufficient for MVP (ETag optimization deferred to Phase 2)
- Pagination handled with `maxResults=50` and `startAt` loop if result count exceeds limit
