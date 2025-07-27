using RemoteClipboard.Model;

namespace RemoteClipboard.Abstractions;

internal interface IAppService
{
    Task<bool> Login(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    );

    void Logout();

    string GetClipboardText();

    void SetClipboardText(string text);

    Task<UserCredentials?> GetSavedCredentials(CancellationToken cancellationToken = default);
    void DeleteSavedCredentials();
}
