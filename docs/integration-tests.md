# Integration Tests Guide

## Overview

The Dora Explorer integration tests verify that:

- The Jira API client can successfully communicate with a real Jira instance
- Jira API responses are properly deserialized into domain models
- Complex nested objects and arrays deserialize correctly
- Date/time fields are properly handled
- Pagination works as expected

## Test Classes

### JiraApiIntegrationTests

Integration tests that execute against a **real Jira instance**. These tests require:

- Valid Jira credentials
- Access to a test Jira project
- Network connectivity to the Jira instance

**Tests included:**

- `CanFetchProjectsFromJira` - Verifies the API client can fetch and deserialize project lists
- `CanSearchForIssuesAndDeserializeResponse` - Tests issue search and response deserialization
- `CanPaginateThroughIssues` - Verifies pagination works correctly
- `DateFieldsAreProperlyDeserialized` - Checks DateTime field handling
- `ComplexNestedObjectsDeserializeCorrectly` - Verifies nested object deserialization

### JiraResponseDeserializationTests

Unit tests that use **mock JSON data** to verify deserialization without external dependencies.

**Tests included:**

- `JiraSearchResponseDeserializesCorrectly` - Verifies JiraSearchResponse model deserialization
- `IssueDeserializesWithAllFieldTypes` - Tests all Issue field types and nested objects

## Setting Up Integration Tests

### 1. Configure User Secrets (Recommended)

Store your Jira credentials securely using User Secrets:

```bash
# Navigate to the test project directory
cd src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool.MSTest

# Initialize User Secrets (first time only)
dotnet user-secrets init

# Set your credentials
dotnet user-secrets set "Jira:Url" "https://your-instance.atlassian.net"
dotnet user-secrets set "Jira:Email" "your-email@example.com"
dotnet user-secrets set "Jira:ApiKey" "your-api-key"
dotnet user-secrets set "Test:ProjectKey" "TEST"

# Verify secrets were set
dotnet user-secrets list
```

### 2. Alternative: Environment Variables

If you prefer environment variables, set them before running tests:

```bash
# PowerShell
$env:Jira__Url = "https://your-instance.atlassian.net"
$env:Jira__Email = "your-email@example.com"
$env:Jira__ApiKey = "your-api-key"
$env:Test__ProjectKey = "TEST"

# Then run tests
dotnet test
```

## Running the Tests

### Run All Tests

```bash
cd src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool.MSTest
dotnet test
```

### Run Only Integration Tests

```bash
dotnet test --filter "ClassName=JiraApiIntegrationTests"
```

### Run Only Deserialization Tests

```bash
dotnet test --filter "ClassName=JiraResponseDeserializationTests"
```

### Run with Verbose Output

```bash
dotnet test --verbosity detailed
```

## Expected Behavior

### When Credentials are Configured

All integration tests will execute against the Jira instance and:

- Connect to your Jira URL
- Search for issues in the specified project
- Verify response deserialization
- Test pagination and date handling

### When Credentials are NOT Configured

Integration tests will be marked as **Inconclusive** and skipped with a message like:

```txt
Jira credentials or test project key not configured. Set via User Secrets or environment variables.
```

Deserialization unit tests will still run since they don't require external connectivity.

## Test Project Requirements

To run integration tests successfully, your test Jira project should have:

- At least one issue created in the last 90 days (for some tests to have data to verify)
- Issues with various states (Open, In Progress, Done, etc.)
- Issues with assignees (if testing assignee deserialization)
- Issues with labels and components (optional, for testing those fields)

## Troubleshooting

### Tests Keep Being Marked Inconclusive

**Problem**: Integration tests skip with "credentials not configured"

**Solution**: Verify your User Secrets are set:

```bash
dotnet user-secrets list
```

Ensure the keys match exactly:

- `Jira:Url`
- `Jira:Email`
- `Jira:ApiKey`
- `Test:ProjectKey`

### Authentication Failures

**Problem**: HTTP 401 Unauthorized errors

**Solution**:

- Verify your email and API key are correct
- Check that your Jira API key is still valid (tokens can expire)
- Ensure your Jira user has access to the test project

### Deserialization Failures

**Problem**: Tests fail with "property cannot deserialize" errors

**Solution**:

- Check that the JSON field names match the model properties
- Verify the property casing matches the API response (usually camelCase from Jira)
- Review the IssueFields class for the correct property names

Example: Jira API returns `created` (camelCase), but it deserializes to `Created` property via JSON configuration

### Network Timeouts

**Problem**: Tests timeout when connecting to Jira

**Solution**:

- Verify your Jira URL is correct and accessible
- Check your network/firewall allows connections to your Jira instance
- Try manually visiting the URL in a browser to confirm access
- Increase test timeout in .runsettings if needed

## CI/CD Integration

For GitHub Actions or other CI pipelines, set secrets as masked environment variables:

```yaml
# GitHub Actions Example
- name: Run Integration Tests
  env:
    Jira__Url: ${{ secrets.JIRA_URL }}
    Jira__Email: ${{ secrets.JIRA_EMAIL }}
    Jira__ApiKey: ${{ secrets.JIRA_API_KEY }}
    Test__ProjectKey: ${{ secrets.TEST_PROJECT_KEY }}
  run: |
    cd src/dora-explorer-dotnet-tool/dora-explorer-dotnet-tool.MSTest
    dotnet test
```

## Best Practices

1. **Use a test project** - Create a dedicated test project in Jira with test issues
2. **Rotate API keys** - Use an API key with minimal permissions
3. **Mock for unit tests** - Use JSON mock data for fast unit tests
4. **Integration tests for validation** - Use real API tests periodically to catch breaking changes
5. **Document field mapping** - Keep notes on which Jira fields map to which model properties

## Adding New Integration Tests

When adding new integration tests:

1. Follow the existing pattern with `[TestClass]` and `[TestMethod]` attributes
2. Skip gracefully if credentials aren't configured
3. Include clear assertions with descriptive messages
4. Add `[Description]` attribute explaining what's being tested
5. Test both happy path (data exists) and edge cases (optional fields)

Example:

```csharp
[TestMethod]
[Description("Integration test: Verify new feature works")]
public async Task YourNewTest()
{
    if (string.IsNullOrEmpty(_jiraUrl) || ...)
    {
        Assert.Inconclusive("Jira credentials not configured...");
    }

    // Arrange
    var client = CreateJiraApiClient(_jiraUrl, _jiraEmail, _jiraApiKey);

    // Act
    var result = await client.YourNewMethod();

    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.SomeProperty > 0, "Property should have expected value");
}
```
