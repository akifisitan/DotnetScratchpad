using System.Net.Sockets;

namespace Scratchpad.Lib;

internal class ExampleRawClient
{
    private static readonly HttpClient _httpClient;

    static ExampleRawClient()
    {
        var socketsHttpHandler = new SocketsHttpHandler
        {
            ConnectCallback = static async (ctx, cancellationToken) =>
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.SetSocketOption(
                    SocketOptionLevel.Tcp,
                    SocketOptionName.TcpKeepAliveTime,
                    20
                );
                socket.SetSocketOption(
                    SocketOptionLevel.Tcp,
                    SocketOptionName.TcpKeepAliveInterval,
                    5
                );
                socket.SetSocketOption(
                    SocketOptionLevel.Tcp,
                    SocketOptionName.TcpKeepAliveRetryCount,
                    5
                );

                await socket.ConnectAsync(ctx.DnsEndPoint, cancellationToken);

                return new NetworkStream(socket, ownsSocket: true);
            },
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            UseCookies = false,
            SslOptions = new()
            {
                RemoteCertificateValidationCallback = static (sender, cert, chain, errors) => true,
            },
        };

        var httpClient = new HttpClient(socketsHttpHandler)
        {
            BaseAddress = new Uri("https://mywebsite.com"),
            Timeout = TimeSpan.FromSeconds(30),
        };

        httpClient.DefaultRequestHeaders.Add("my", "header");

        _httpClient = httpClient;
    }

    public async Task<string> GetSomethingAsync()
    {
        using var cts = new CancellationTokenSource();

        var response = await _httpClient.GetAsync(
            "slow-endpoint-that-needs-keep-alives-to-not-close"
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
