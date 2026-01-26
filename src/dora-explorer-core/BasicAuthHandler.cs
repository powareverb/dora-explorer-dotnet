using System.Text;

namespace DoraExplorer.Core;

/// <summary>
/// HTTP message handler that injects Basic Authentication header for Jira API requests
/// </summary>
public class BasicAuthHandler : DelegatingHandler
{
    private readonly string _email;
    private readonly string _apiKey;

    /// <summary>
    /// Creates a new Basic Auth handler
    /// </summary>
    /// <param name="email">Jira user email</param>
    /// <param name="apiKey">Jira API key</param>
    public BasicAuthHandler(string email, string apiKey)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        InnerHandler = new HttpClientHandler();
    }

    /// <summary>
    /// Intercepts HTTP requests and adds Basic Auth header
    /// </summary>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiKey}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        return base.SendAsync(request, cancellationToken);
    }
}
