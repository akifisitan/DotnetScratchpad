using System.Windows;
using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard.Services;

internal sealed class AppService : IAppService
{
    private readonly IAuthService _authService;
    private readonly ISecureStorage _secureStorage;

    public AppService(IAuthService authService, ISecureStorage secureStorage)
    {
        _authService = authService;
        _secureStorage = secureStorage;
    }

    public async Task<bool> Login(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    )
    {
        await _authService.Login(userCredentials, cancellationToken);

        ApplicationData.UserCredentials = userCredentials;

        await _secureStorage.SaveCredentials(userCredentials, cancellationToken);

        return true;
    }

    public void Logout()
    {
        ApplicationData.UserCredentials = null;

        _secureStorage.DeleteCredentials();
    }

    public string GetClipboardText()
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

    public void SetClipboardText(string text)
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

    public async Task<UserCredentials?> GetSavedCredentials(
        CancellationToken cancellationToken = default
    )
    {
        return await _secureStorage.GetCredentials(cancellationToken);
    }

    public void DeleteSavedCredentials()
    {
        _secureStorage.DeleteCredentials();
    }
}
