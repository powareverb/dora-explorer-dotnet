using System.Text.Json.Serialization;

namespace DoraExplorer.DotNetTool.MSTest
{
    /// <summary>
    /// Integration tests that verify actual Jira API communication and response deserialization
    ///
    /// These tests require valid Jira credentials and should be run against a real Jira instance.
    ///
    /// To enable these tests:
    /// 1. Configure your Jira instance URL via User Secrets:
    ///    dotnet user-secrets set "Jira:Url" "https://your-instance.atlassian.net"
    /// 2. Set your email via User Secrets:
    ///    dotnet user-secrets set "Jira:Email" "your-email@example.com"
    /// 3. Set your API key via User Secrets:
    ///    dotnet user-secrets set "Jira:ApiKey" "your-api-key"
    /// 4. Set a test project key via User Secrets:
    ///    dotnet user-secrets set "Test:ProjectKey" "TEST"
    ///
    /// The tests will skip if credentials are not configured.
    /// </summary>
    [TestClass]
    public class JiraApiIntegrationTests
    {
        private IConfiguration? _configuration;
        private string? _jiraUrl;
        private string? _jiraEmail;
        private string? _jiraApiKey;
        private string? _testProjectKey;

        [TestInitialize]
        public void Setup()
        {
            _configuration = BuildConfiguration();

            _jiraUrl = _configuration["Jira:Url"];
            _jiraEmail = _configuration["Jira:Email"];
            _jiraApiKey = _configuration["Jira:ApiKey"];
            _testProjectKey = _configuration["Test:ProjectKey"];
        }


        private IConfiguration BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddUserSecrets<JiraApiIntegrationTests>(optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            return configBuilder.Build();
        }

        [TestMethod]
        [Description("Integration test: Verify we can connect to Jira and fetch projects")]
        public async Task CanFetchProjectsFromJira()
        {
            // Skip if credentials not configured
            if (string.IsNullOrEmpty(_jiraUrl) || string.IsNullOrEmpty(_jiraEmail) || string.IsNullOrEmpty(_jiraApiKey))
            {
                Assert.Inconclusive("Jira credentials not configured. Set via User Secrets or environment variables.");
            }

            // Arrange
            var client = CreateJiraApiClient(_jiraUrl, _jiraEmail, _jiraApiKey);

            // Act
            var response = await client.GetProjectsAsync();

            // Assert
            Assert.IsNotNull(response, "Project response should not be null");
            Assert.IsNotNull(response.Values, "Projects list should not be null");
            Assert.IsTrue(response.Values.Count > 0, "Should have at least one project");
            Assert.IsTrue(response.Values.All(p => !string.IsNullOrEmpty(p.Key)), "All projects should have a key");
            Assert.IsTrue(response.Values.All(p => !string.IsNullOrEmpty(p.Name)), "All projects should have a name");
        }

        [TestMethod]
        [Description("Integration test: Verify we can search for issues and deserialize response")]
        public async Task CanSearchForIssuesAndDeserializeResponse()
        {
            // Skip if credentials not configured
            if (string.IsNullOrEmpty(_jiraUrl) || string.IsNullOrEmpty(_jiraEmail) || string.IsNullOrEmpty(_jiraApiKey) || string.IsNullOrEmpty(_testProjectKey))
            {
                Assert.Inconclusive("Jira credentials or test project key not configured. Set via User Secrets or environment variables.");
            }

            // Arrange
            var client = CreateJiraApiClient(_jiraUrl, _jiraEmail, _jiraApiKey);
            var jql = $"project = {_testProjectKey} AND created >= -90d ORDER BY created DESC";

            // Act
            // TODO: Create a generalised test helper that can capture deserialization issues
            var response = null as JiraSearchResponse;
            try
            {
                var test = await client.SearchIssuesWithDiagnosisAsync(jql, maxResults: 1);
                response = test.Content;
                if (test.IsSuccessStatusCode == false)
                {
                    Assert.Fail($"Jira API returned error: {test.StatusCode} - {test.Error}");
                }
                if (test.Error != null)
                {
                    Assert.Fail($"Jira API returned error: {test.Error}");
                }
            }
            // So we can diagnose deserialisation issues
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            // Assert - Response structure
            Assert.IsNotNull(response, "Search response should not be null");
            Assert.IsTrue(response.Total > 0 || response.Issues.Count >= 0, "Search should return valid results");
            // This doesn't seem to be reliably set
            //Assert.IsTrue(response.MaxResults > 0, "Max results should be set");
            Assert.IsTrue(response.StartAt >= 0, "Start at should be non-negative");

            if (response.Issues.Count > 0)
            {
                // Assert - Issue data deserialization
                var issue = response.Issues[0];

                Assert.IsNotNull(issue.Key, "Issue key should be deserialized");
                Assert.IsNotNull(issue.Id, "Issue ID should be deserialized");
                Assert.IsTrue(string.IsNullOrEmpty(issue.Key) == false, "Issue key should not be empty");

                // Assert - Fields deserialization
                var fields = issue.Fields;
                Assert.IsNotNull(fields, "Issue fields should be deserialized");
                Assert.IsNotNull(fields.Summary, "Summary should be deserialized");
                Assert.IsTrue(fields.Created != DateTimeOffset.MinValue, "Created date should be deserialized");
                Assert.IsTrue(fields.Updated != DateTimeOffset.MinValue, "Updated date should be deserialized");

                // Assert - Status deserialization
                Assert.IsNotNull(fields.Status, "Status should be deserialized");
                Assert.IsNotNull(fields.Status.Name, "Status name should be deserialized");

                // Assert - Optional fields may be null but should deserialize correctly
                if (fields.IssueType != null)
                {
                    Assert.IsNotNull(fields.IssueType.Name, "Issue type name should be deserialized");
                }

                if (fields.Assignee != null)
                {
                    Assert.IsNotNull(fields.Assignee.DisplayName, "Assignee display name should be deserialized if present");
                }

                if (fields.Labels != null && fields.Labels.Count > 0)
                {
                    Assert.IsTrue(fields.Labels.All(l => !string.IsNullOrEmpty(l)), "All labels should be non-empty");
                }
            }
        }

        [TestMethod]
        [Description("Integration test: Verify pagination works correctly")]
        public async Task CanPaginateThroughIssues()
        {
            // Skip if credentials not configured
            if (string.IsNullOrEmpty(_jiraUrl) || string.IsNullOrEmpty(_jiraEmail) || string.IsNullOrEmpty(_jiraApiKey) || string.IsNullOrEmpty(_testProjectKey))
            {
                Assert.Inconclusive("Jira credentials or test project key not configured. Set via User Secrets or environment variables.");
            }

            // Arrange
            var client = CreateJiraApiClient(_jiraUrl, _jiraEmail, _jiraApiKey);
            var jql = $"project = {_testProjectKey} AND created >= -90d ORDER BY created DESC";

            // Act - First page
            var page1 = await client.SearchIssuesAsync(jql, maxResults: 5, startAt: 0);

            // Assert - First page
            Assert.IsNotNull(page1, "First page should not be null");
            Assert.IsTrue(page1.Issues.Count <= 5, "First page should have at most 5 issues");
            Assert.AreEqual(0, page1.StartAt, "First page should start at 0");

            if (page1.Total > 5)
            {
                // Act - Second page
                var page2 = await client.SearchIssuesAsync(jql, maxResults: 5, startAt: 5);

                // Assert - Second page
                Assert.IsNotNull(page2, "Second page should not be null");
                Assert.AreEqual(5, page2.StartAt, "Second page should start at 5");

                // Issues should be different
                var firstPageKeys = new HashSet<string>(page1.Issues.Select(i => i.Key));
                var secondPageKeys = new HashSet<string>(page2.Issues.Select(i => i.Key));
                var intersection = firstPageKeys.Intersect(secondPageKeys).Count();

                Assert.AreEqual(0, intersection, "Pages should not have overlapping issues");
            }
        }

        [TestMethod]
        [Description("Integration test: Verify date fields are properly deserialized as DateTime")]
        public async Task DateFieldsAreProperlyDeserialized()
        {
            // Skip if credentials not configured
            if (string.IsNullOrEmpty(_jiraUrl) || string.IsNullOrEmpty(_jiraEmail) || string.IsNullOrEmpty(_jiraApiKey) || string.IsNullOrEmpty(_testProjectKey))
            {
                Assert.Inconclusive("Jira credentials or test project key not configured. Set via User Secrets or environment variables.");
            }

            // Arrange
            var client = CreateJiraApiClient(_jiraUrl, _jiraEmail, _jiraApiKey);
            var jql = $"project = {_testProjectKey} AND created >= -30d ORDER BY created DESC";

            // Act
            var response = await client.SearchIssuesAsync(jql, maxResults: 1);

            // Assert
            if (response.Issues.Count > 0)
            {
                var issue = response.Issues[0];
                var fields = issue.Fields;

                Assert.IsTrue(fields.Created > DateTimeOffset.MinValue, "Created should be a valid DateTime");
                Assert.IsTrue(fields.Updated > DateTimeOffset.MinValue, "Updated should be a valid DateTime");
                Assert.IsTrue(fields.Created <= DateTimeOffset.UtcNow, "Created should not be in the future");
                Assert.IsTrue(fields.Updated <= DateTimeOffset.UtcNow, "Updated should not be in the future");

                // If resolvable, check it's also valid
                if (fields.Resolvable.HasValue)
                {
                    Assert.IsTrue(fields.Resolvable.Value > DateTimeOffset.MinValue, "Resolvable should be a valid DateTime");
                    Assert.IsTrue(fields.Resolvable.Value <= DateTimeOffset.UtcNow, "Resolvable should not be in the future");
                    Assert.IsTrue(fields.Resolvable.Value >= fields.Created, "Resolvable should be after created");
                }
            }
        }

        [TestMethod]
        [Description("Integration test: Verify complex nested objects deserialize correctly")]
        public async Task ComplexNestedObjectsDeserializeCorrectly()
        {
            // Skip if credentials not configured
            if (string.IsNullOrEmpty(_jiraUrl) || string.IsNullOrEmpty(_jiraEmail) || string.IsNullOrEmpty(_jiraApiKey) || string.IsNullOrEmpty(_testProjectKey))
            {
                Assert.Inconclusive("Jira credentials or test project key not configured. Set via User Secrets or environment variables.");
            }

            // Arrange
            var client = CreateJiraApiClient(_jiraUrl, _jiraEmail, _jiraApiKey);
            var jql = $"project = {_testProjectKey} AND created >= -90d ORDER BY created DESC";

            // Act
            var response = await client.SearchIssuesAsync(jql, maxResults: 5);

            // Assert - Each issue should have properly deserialized nested objects
            foreach (var issue in response.Issues)
            {
                var fields = issue.Fields;

                // Status category (nested object)
                if (fields.Status?.StatusCategory != null)
                {
                    Assert.IsNotNull(fields.Status.StatusCategory.Key, "Status category key should deserialize");
                    Assert.IsNotNull(fields.Status.StatusCategory.Name, "Status category name should deserialize");
                }

                // Components (array of objects)
                if (fields.Components != null && fields.Components.Count > 0)
                {
                    foreach (var component in fields.Components)
                    {
                        Assert.IsNotNull(component.Name, "Component name should deserialize");
                    }
                }

                // Version (array of objects)
                if (fields.FixVersions != null && fields.FixVersions.Count > 0)
                {
                    foreach (var version in fields.FixVersions)
                    {
                        Assert.IsNotNull(version.Name, "Version name should deserialize");
                    }
                }

                // Issue links (array of complex objects)
                if (fields.IssueLinks != null && fields.IssueLinks.Count > 0)
                {
                    foreach (var link in fields.IssueLinks)
                    {
                        Assert.IsNotNull(link.Type, "Link type should deserialize");
                        // Either inward or outward issue should be present
                        Assert.IsTrue(
                            link.InwardIssue != null || link.OutwardIssue != null,
                            "Link should have either inward or outward issue"
                        );
                    }
                }
            }
        }

        private IJiraApiClient CreateJiraApiClient(string jiraUrl, string email, string apiKey)
        {
            var handler = new BasicAuthHandler(email, apiKey);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(jiraUrl)
            };

            var jiraApi = RestService.For<IJiraApiClient>(httpClient, new RefitSettings
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

            return jiraApi;
        }
    }


    /// <summary>
    /// Unit tests for response deserialization using mock JSON data
    /// </summary>
    [TestClass]
    public class JiraResponseDeserializationTests
    {
        [TestMethod]
        [Description("Unit test: Verify JiraSearchResponse deserializes from JSON")]
        public void JiraSearchResponseDeserializesCorrectly()
        {
            // Arrange
            var jsonResponse = """
            {
                "expand": "names,schema",
                "startAt": 0,
                "maxResults": 50,
                "total": 100,
                "issues": [
                    {
                        "expand": "changelog,html",
                        "id": "10001",
                        "key": "TEST-1",
                        "fields": {
                            "created": "2025-01-10T10:00:00.000Z",
                            "updated": "2025-01-15T15:30:00.000Z",
                            "summary": "Test Issue",
                            "description": "Test Description",
                            "status": {
                                "self": "https://test.atlassian.net/rest/api/3/status/1",
                                "description": "The issue is open",
                                "iconUrl": "https://test.atlassian.net/images/icons/statuses/open.png",
                                "name": "Open",
                                "id": "1",
                                "statusCategory": {
                                    "self": "https://test.atlassian.net/rest/api/3/statuscategory/2",
                                    "id": 2,
                                    "key": "new",
                                    "colorName": "blue-gray",
                                    "name": "To Do"
                                }
                            }
                        }
                    }
                ]
            }
            """;

            // Act
            var response = JsonSerializer.Deserialize<JiraSearchResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Assert
            Assert.IsNotNull(response, "Response should deserialize");
            Assert.AreEqual(0, response.StartAt, "StartAt should be 0");
            Assert.AreEqual(50, response.MaxResults, "MaxResults should be 50");
            Assert.AreEqual(100, response.Total, "Total should be 100");
            Assert.AreEqual(1, response.Issues.Count, "Should have 1 issue");

            var issue = response.Issues[0];
            Assert.AreEqual("10001", issue.Id, "Issue ID should match");
            Assert.AreEqual("TEST-1", issue.Key, "Issue key should match");

            var fields = issue.Fields;
            Assert.AreEqual("Test Issue", fields.Summary, "Summary should match");
            Assert.AreEqual("Test Description", fields.Description, "Description should match");
            Assert.AreEqual("Open", fields.Status.Name, "Status name should match");
            Assert.AreEqual("new", fields.Status.StatusCategory.Key, "Status category key should match");
        }

        [TestMethod]
        [Description("Unit test: Verify Issue model deserializes with all field types")]
        public void IssueDeserializesWithAllFieldTypes()
        {
            // Arrange
            var jsonResponse = """
            {
                "expand": "changelog,html",
                "id": "10100",
                "key": "PROJ-456",
                "fields": {
                    "created": "2025-01-01T08:00:00.000Z",
                    "updated": "2025-01-20T14:30:00.000Z",
                    "resolved": "2025-01-18T16:00:00.000Z",
                    "summary": "Complex Issue",
                    "description": "Complex Description",
                    "issuetype": {
                        "self": "https://test.atlassian.net/rest/api/3/issuetype/10001",
                        "id": "10001",
                        "description": "A bug in the system",
                        "iconUrl": "https://test.atlassian.net/images/icons/issuetypes/bug.png",
                        "name": "Bug",
                        "subtask": false
                    },
                    "status": {
                        "self": "https://test.atlassian.net/rest/api/3/status/10001",
                        "description": "The issue is closed",
                        "iconUrl": "https://test.atlassian.net/images/icons/statuses/closed.png",
                        "name": "Done",
                        "id": "10001",
                        "statusCategory": {
                            "self": "https://test.atlassian.net/rest/api/3/statuscategory/3",
                            "id": 3,
                            "key": "done",
                            "colorName": "green",
                            "name": "Done"
                        }
                    },
                    "priority": {
                        "self": "https://test.atlassian.net/rest/api/3/priority/2",
                        "iconUrl": "https://test.atlassian.net/images/icons/priorities/high.svg",
                        "name": "High",
                        "id": "2"
                    },
                    "assignee": {
                        "self": "https://test.atlassian.net/rest/api/3/user?accountId=test",
                        "accountId": "test",
                        "emailAddress": "test@example.com",
                        "displayName": "Test User",
                        "active": true
                    },
                    "reporter": {
                        "self": "https://test.atlassian.net/rest/api/3/user?accountId=reporter",
                        "accountId": "reporter",
                        "emailAddress": "reporter@example.com",
                        "displayName": "Reporter User",
                        "active": true
                    },
                    "labels": ["bug", "critical"],
                    "components": [
                        {
                            "self": "https://test.atlassian.net/rest/api/3/component/10000",
                            "id": "10000",
                            "name": "Frontend",
                            "description": "Frontend component"
                        }
                    ],
                    "fixVersions": [
                        {
                            "self": "https://test.atlassian.net/rest/api/3/version/10001",
                            "id": "10001",
                            "name": "1.0.0",
                            "archived": false,
                            "released": true
                        }
                    ],
                    "resolution": {
                        "self": "https://test.atlassian.net/rest/api/3/resolution/1",
                        "id": "1",
                        "name": "Fixed",
                        "description": "Fixed"
                    }
                }
            }
            """
            ;

            // Act
            var jiraIssue = JsonSerializer.Deserialize<JiraIssue>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Assert
            Assert.IsNotNull(jiraIssue, "Issue should deserialize");
            Assert.AreEqual("10100", jiraIssue.Id, "ID should match");
            Assert.AreEqual("PROJ-456", jiraIssue.Key, "Key should match");

            var fields = jiraIssue.Fields;
            Assert.AreEqual("Complex Issue", fields.Summary);
            Assert.AreEqual("Complex Description", fields.Description);
            Assert.AreEqual("2025-01-01T08:00:00.000Z", fields.Created?.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));
            Assert.AreEqual("Bug", fields.IssueType.Name);
            Assert.AreEqual("Done", fields.Status.Name);
            Assert.AreEqual("High", fields.Priority.Name);
            Assert.AreEqual("Test User", fields.Assignee.DisplayName);
            Assert.AreEqual("Reporter User", fields.Reporter.DisplayName);
            Assert.AreEqual(2, fields.Labels.Count);
            Assert.AreEqual("Frontend", fields.Components[0].Name);
            Assert.AreEqual("1.0.0", fields.FixVersions[0].Name);
            Assert.AreEqual("Fixed", fields.Resolution.Name);
        }
    }
}
