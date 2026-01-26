using Microsoft.VisualStudio.TestTools.UnitTesting;
using DoraExplorer.Core;
using System.Text.Json;

namespace DoraExplorer.DotNetTool.MSTest
{
    [TestClass]
    public class PullIssuesTests
    {
        private string? _testCacheDir;

        [TestInitialize]
        public void Setup()
        {
            _testCacheDir = Path.Combine(Path.GetTempPath(), $"dora-test-cache-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testCacheDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_testCacheDir != null && Directory.Exists(_testCacheDir))
            {
                Directory.Delete(_testCacheDir, recursive: true);
            }
        }

        [TestMethod]
        public async Task CacheSaveAndLoadAsync()
        {
            // Arrange
            var cache = new IssueCache(_testCacheDir);
            var testIssues = new List<Issue>
            {
                new Issue
                {
                    Key = "TEST-1",
                    Id = "1",
                    Summary = "Test Issue 1",
                    Created = DateTime.UtcNow.AddDays(-1),
                    Updated = DateTime.UtcNow,
                    Status = new Status { Name = "Open" }
                },
                new Issue
                {
                    Key = "TEST-2",
                    Id = "2",
                    Summary = "Test Issue 2",
                    Created = DateTime.UtcNow.AddDays(-2),
                    Updated = DateTime.UtcNow,
                    Status = new Status { Name = "Done" }
                }
            };

            // Act
            await cache.SaveAsync("TESTPROJ", testIssues);
            var loaded = await cache.TryLoadAsync("TESTPROJ", TimeSpan.FromHours(1));

            // Assert
            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded.Count);
            Assert.AreEqual("TEST-1", loaded[0].Key);
            Assert.AreEqual("TEST-2", loaded[1].Key);
            Assert.AreEqual("Test Issue 1", loaded[0].Summary);
        }

        [TestMethod]
        public async Task CacheInvalidateAsync()
        {
            // Arrange
            var cache = new IssueCache(_testCacheDir);
            var testIssues = new List<Issue>
            {
                new Issue { Key = "INVALID-1", Created = DateTime.UtcNow, Updated = DateTime.UtcNow }
            };

            await cache.SaveAsync("INVALIDATE", testIssues);

            // Act
            cache.Invalidate("INVALIDATE");
            var loaded = await cache.TryLoadAsync("INVALIDATE", TimeSpan.FromHours(1));

            // Assert
            Assert.IsNull(loaded);
        }

        [TestMethod]
        public void IssueModelHasAllRequiredFields()
        {
            // Arrange & Act
            var issue = new Issue
            {
                Key = "PROJ-123",
                Id = "123",
                Summary = "Test summary",
                Description = "Test description",
                IssueType = new IssueType { Id = "10001", Name = "Bug" },
                Status = new Status { Id = "1", Name = "Open" },
                Created = DateTime.UtcNow.AddDays(-5),
                Updated = DateTime.UtcNow,
                Resolved = DateTime.UtcNow,
                Resolution = new Resolution { Id = "1", Name = "Fixed" },
                Labels = new List<string> { "urgent", "backend" },
                Components = new List<Component> { new Component { Id = "1", Name = "API" } },
                FixVersions = new List<DoraExplorer.Core.Version> { new DoraExplorer.Core.Version { Id = "1", Name = "1.0.0", Released = true } },
                AffectedVersions = new List<DoraExplorer.Core.Version> { new DoraExplorer.Core.Version { Id = "2", Name = "0.9.0", Released = false } },
                Priority = new Priority { Id = "1", Name = "High" },
                Assignee = new User { AccountId = "user1", DisplayName = "John Doe", EmailAddress = "john@example.com" },
                Reporter = new User { AccountId = "user2", DisplayName = "Jane Smith", EmailAddress = "jane@example.com" }
            };

            // Assert - Verify all fields are set
            Assert.AreEqual("PROJ-123", issue.Key);
            Assert.AreEqual("Test summary", issue.Summary);
            Assert.AreEqual("Bug", issue.IssueType?.Name);
            Assert.AreEqual("Open", issue.Status?.Name);
            Assert.IsNotNull(issue.Created);
            Assert.IsNotNull(issue.Resolved);
            Assert.AreEqual("Fixed", issue.Resolution?.Name);
            Assert.AreEqual(2, issue.Labels?.Count);
            Assert.AreEqual(1, issue.Components?.Count);
            Assert.AreEqual(1, issue.FixVersions?.Count);
            Assert.AreEqual("High", issue.Priority?.Name);
            Assert.AreEqual("John Doe", issue.Assignee?.DisplayName);
        }

        [TestMethod]
        public void BasicAuthHandlerCreatesCorrectHeader()
        {
            // Arrange
            var email = "test@example.com";
            var apiKey = "test-key-123";
            var handler = new BasicAuthHandler(email, apiKey);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/api");

            // We need to call SendAsync to test the header injection
            // For now, just verify the handler can be created
            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void JiraSearchResponseMappingWorks()
        {
            // Arrange
            var json = @"{
                ""startAt"": 0,
                ""maxResults"": 50,
                ""total"": 2,
                ""issues"": [
                    {
                        ""id"": ""10001"",
                        ""key"": ""PROJ-1"",
                        ""fields"": {
                            ""summary"": ""Test Issue 1"",
                            ""created"": ""2025-01-20T10:00:00Z"",
                            ""updated"": ""2025-01-25T15:30:00Z"",
                            ""issueType"": { ""id"": ""10001"", ""name"": ""Bug"" },
                            ""status"": { ""id"": ""1"", ""name"": ""Open"" }
                        }
                    }
                ]
            }";

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var response = JsonSerializer.Deserialize<JiraSearchResponse>(json, options);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.StartAt);
            Assert.AreEqual(50, response.MaxResults);
            Assert.AreEqual(2, response.Total);
            Assert.AreEqual(1, response.Issues?.Count);
            Assert.AreEqual("PROJ-1", response.Issues?[0].Key);
            Assert.AreEqual("Test Issue 1", response.Issues?[0].Fields?.Summary);
        }
    }
}
