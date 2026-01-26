namespace DoraExplorer.Core;

using System.IO.Abstractions;

/// <summary>
/// Manages local filesystem caching of Jira issues with TTL-based expiration
/// </summary>
public class IssueCache
{
    private readonly string _cacheDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Creates a new instance of IssueCache
    /// </summary>
    /// <param name="cacheDirectory">Root directory for cache storage. If null, uses ~/.dora-explorer/cache/</param>
    /// <param name="fileSystem">File system abstraction for dependency injection. If null, uses real filesystem</param>
    public IssueCache(string? cacheDirectory = null, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();

        _cacheDirectory = cacheDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dora-explorer",
            "cache");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        _fileSystem.Directory.CreateDirectory(_cacheDirectory);
        Console.WriteLine($"Issue cache directory: {_cacheDirectory}");
    }

    /// <summary>
    /// Saves issues to cache with metadata
    /// </summary>
    /// <param name="projectKey">Jira project key (e.g., "PROJ")</param>
    /// <param name="issues">List of issues to cache</param>
    /// <param name="ct">Cancellation token</param>
    public async Task SaveAsync(string projectKey, List<Issue> issues, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            throw new ArgumentException("Project key cannot be empty", nameof(projectKey));

        if (issues is null)
            throw new ArgumentNullException(nameof(issues));

        var cacheEntry = new CacheEntry
        {
            ProjectKey = projectKey,
            SavedAt = DateTime.UtcNow,
            Issues = issues
        };

        var fileName = GetCacheFileName(projectKey);
        var json = JsonSerializer.Serialize(cacheEntry, _jsonOptions);

        await _fileSystem.File.WriteAllTextAsync(fileName, json, ct);
    }

    /// <summary>
    /// Attempts to load cached issues if they exist and haven't expired
    /// </summary>
    /// <param name="projectKey">Jira project key</param>
    /// <param name="cacheTtl">Maximum age of cached data before expiration</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cached issues if valid, or null if cache miss/expired</returns>
    public async Task<List<Issue>?> TryLoadAsync(
        string projectKey,
        TimeSpan? cacheTtl = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            return null;

        cacheTtl ??= TimeSpan.FromHours(1); // Default 1 hour TTL
        var fileName = GetCacheFileName(projectKey);

        if (!_fileSystem.File.Exists(fileName))
            return null;

        try
        {
            var json = await _fileSystem.File.ReadAllTextAsync(fileName, ct);
            var cacheEntry = JsonSerializer.Deserialize<CacheEntry>(json, _jsonOptions);

            if (cacheEntry is null)
                return null;

            var age = DateTime.UtcNow - cacheEntry.SavedAt;
            if (age > cacheTtl)
                return null; // Cache expired

            return cacheEntry.Issues;
        }
        catch (Exception ex)
        {
            // Log cache read errors but don't fail - treat as cache miss
            System.Diagnostics.Debug.WriteLine($"Cache read error for {projectKey}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Invalidates (deletes) cached data for a project
    /// </summary>
    /// <param name="projectKey">Jira project key</param>
    public void Invalidate(string projectKey)
    {
        if (string.IsNullOrWhiteSpace(projectKey))
            return;

        var fileName = GetCacheFileName(projectKey);
        if (_fileSystem.File.Exists(fileName))
            _fileSystem.File.Delete(fileName);
    }

    private string GetCacheFileName(string projectKey)
    {
        var safeKey = projectKey.Replace("/", "_").Replace("\\", "_");
        return Path.Combine(_cacheDirectory, $"{safeKey}.cache.json");
    }

    /// <summary>
    /// Internal cache entry with metadata
    /// </summary>
    private class CacheEntry
    {
        public string? ProjectKey { get; set; }
        public DateTime SavedAt { get; set; }
        public List<Issue>? Issues { get; set; }
    }
}
