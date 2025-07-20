using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Scratchpad.Lib.Abstractions;

namespace Scratchpad.Lib;

internal class RemoteClipboardService : IClipboardService
{
    private static readonly SocketsHttpHandler httpHandler = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        SslOptions = new() { RemoteCertificateValidationCallback = (_, _, _, _) => true },
    };

    private static string? _lastKvp;
    private static AuthenticationHeaderValue _authenticationHeader = null!;

    public async Task<string> ReadFromClipboard(CancellationToken cancellationToken = default)
    {
        using var httpClient = GetHttpClient();

        var response = await httpClient.GetAsync("/123", cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task WriteToClipboard(string text, CancellationToken cancellationToken = default)
    {
        using var httpClient = GetHttpClient();

        var response = await httpClient.PostAsJsonAsync("/123", new object(), cancellationToken);
    }

    private static AuthenticationHeaderValue GetAuthenticationHeaderValue()
    {
        var userName = "";
        var password = "";
        var kvp = $"{userName}:{password}";

        if (_lastKvp == kvp)
        {
            return _authenticationHeader;
        }

        _lastKvp = kvp;
        _authenticationHeader = new(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(_lastKvp))
        );

        return _authenticationHeader;
    }

    private static HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient(httpHandler, disposeHandler: false)
        {
            BaseAddress = new Uri("http://localhost:5173"),
        };

        httpClient.DefaultRequestHeaders.Authorization = GetAuthenticationHeaderValue();

        return httpClient;
    }
}
