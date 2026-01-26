# Overview

Dora explorer is a DORA tool, built in dotnet 10 which will generate various DORA metrics based on a number of input sources (currently just Jira).
It makes use of labels, releases and relationships between issues to generate various DORA metrics for output.

## Features

- Pull issues and release information from Jira, using an API key.  Cache issue metadata locally (using etags) to minimise continued queries to Jira.
- Generate DORA metrics from issue information and timing
- Output DORA metrics to various places
  - Output to CSV
  - Output to MD
  - Output to Confluence pages based on templates

## Technical Features

- Use Refit and direct queries rather than Jira libraries to minimise dependencies.
- Cache issue metadata locally (using etags) to minimise continued queries to Jira.
