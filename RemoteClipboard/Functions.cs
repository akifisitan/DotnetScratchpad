using System.IO;
using System.Net.Http;
using System.Windows;

namespace RemoteClipboard;

public static class Functions
{
    private const string fileName = "hello.txt";
    private static readonly SocketsHttpHandler httpHandler = new();

    private static HttpClient GetHttpClient() =>
        new(httpHandler) { BaseAddress = new Uri("http://localhost:5173") };

    public static async Task<bool> Login(UserCredentials userCredentials)
    {
        await Task.Delay(1500);

        ApplicationData.UserCredentials = userCredentials;

        await SecureStorage.SaveCredentials(userCredentials);

        return true;
    }

    public static void Logout()
    {
        ApplicationData.UserCredentials = null;

        SecureStorage.DeleteCredentials();
    }

    public static async Task WriteToRemoteClipboard(string? text = null)
    {
        text ??= GetClipboardText();
        await Task.Delay(1000);
        await File.WriteAllTextAsync(fileName, text);
    }

    public static async Task<string> ReadFromRemoteClipboard(
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(1000);

        var httpClient = GetHttpClient();

        var response = await httpClient.GetAsync("/123", cancellationToken);

        var text = await File.ReadAllTextAsync(fileName, cancellationToken);
        SetClipboardText(text);
        return text;
    }

    private static string GetClipboardText()
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

    private static void SetClipboardText(string text)
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
