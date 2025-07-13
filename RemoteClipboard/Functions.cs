using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;

namespace RemoteClipboard;

public static class Functions
{
    private const string fileName = "hello.txt";
    private static readonly SocketsHttpHandler httpHandler = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        SslOptions = new() { RemoteCertificateValidationCallback = (_, _, _, _) => true },
    };

    private static string? _lastKvp;
    private static AuthenticationHeaderValue _authenticationHeader = null!;

    private static AuthenticationHeaderValue GetAuthenticationHeaderValue()
    {
        var kvp =
            $"{ApplicationData.UserCredentials?.UserName}:{ApplicationData.UserCredentials?.Password}";

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

    public static async Task<bool> Login(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(1500, cancellationToken);

        ApplicationData.UserCredentials = userCredentials;

        await SecureStorage.SaveCredentials(userCredentials, cancellationToken);

        return true;
    }

    public static void Logout()
    {
        ApplicationData.UserCredentials = null;

        SecureStorage.DeleteCredentials();
    }

    public static async Task WriteToRemoteClipboard(
        string? text = null,
        CancellationToken cancellationToken = default
    )
    {
        text ??= GetClipboardText();
        await Task.Delay(1000, cancellationToken);
        await File.WriteAllTextAsync(fileName, text, cancellationToken);
    }

    public static async Task<string> ReadFromRemoteClipboard(
        CancellationToken cancellationToken = default
    )
    {
        //using var httpClient = GetHttpClient();

        //var response = await httpClient.GetAsync("/123", cancellationToken);

        await Task.Delay(1000, cancellationToken);
        var text = await File.ReadAllTextAsync(fileName, cancellationToken);
        SetClipboardText(text);
        return text;
    }

    public static string GetClipboardText()
    {
        try
        {
            return Clipboard.GetText(TextDataFormat.UnicodeText);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return string.Empty;
        }
    }

    public static void SetClipboardText(string text)
    {
        try
        {
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
