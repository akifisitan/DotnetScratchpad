using RemoteClipboard.Model;

namespace RemoteClipboard.Abstractions;

internal interface ISecureStorage
{
    Task SaveCredentials(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    );

    Task<UserCredentials?> GetCredentials(CancellationToken cancellationToken = default);

    void DeleteCredentials();
}
