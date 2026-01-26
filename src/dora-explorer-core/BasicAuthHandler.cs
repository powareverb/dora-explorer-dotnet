namespace DoraExplorer.Core;

/// <summary>
/// HTTP message handler that injects HTTP Basic Authentication header
/// for Jira REST API requests using email and API key
/// </summary>
public class BasicAuthHandler : DelegatingHandler
{
    private readonly string _email;
    private readonly string _apiKey;

    /// <summary>
    /// Initializes a new instance of BasicAuthHandler
    /// </summary>
    /// <param name="email">Jira user email</param>
    /// <param name="apiKey">Jira API key</param>
    /// <exception cref="ArgumentNullException">Thrown when email or apiKey is null</exception>
    public BasicAuthHandler(string email, string apiKey)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        InnerHandler = new HttpClientHandler();
    }

    /// <summary>
    /// Intercepts HTTP requests and injects Basic Auth header
    /// </summary>
    /// <param name="request">HTTP request message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTTP response message</returns>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_email}:{_apiKey}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        return base.SendAsync(request, cancellationToken);
    }
}
