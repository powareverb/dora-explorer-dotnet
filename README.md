# Overview

Dora explorer is a DORA tool, built in dotnet 10 which will generate various DORA metrics based on a number of input sources (currently just Jira).
It makes use of labels, releases and relationships between issues to generate various DORA metrics for output.

## Features

1. Pull issues and release information from Jira, using an API key.  Cache issue metadata locally (using etags) to minimise continued queries to Jira.
2. Generate DORA metrics from issue information and timing
3. Output DORA metrics to various places
  a. Output to CSV
  b. Output to MD
  c. Output to Confluence pages based on templates

## Usage

```sh
dora-explorer-dotnet pull-issues \
  --jira-url https://your-instance.atlassian.net \
  --jira-email user@example.com \
  --jira-api-key <api-key> \
  --project-keys PROJ1,PROJ2 \
  --start-date 2025-01-01 \
  --end-date 2026-01-31 \
  --cache-ttl-hours 2
```

All of the parameters are also available using either environmental variables, or in a default configuration file.

## Technical Features

- Use Refit and direct queries rather than Jira libraries to minimise dependencies.
- Cache issue metadata locally (using etags) to minimise continued queries to Jira.

## Co-authored using Copilot

This tool is also a test of co-authoring a tool using the AI tool Copilot.
