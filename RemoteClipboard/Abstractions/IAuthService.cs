using RemoteClipboard.Model;

namespace RemoteClipboard.Abstractions;

public interface IAuthService
{
    Task Login(UserCredentials userCredentials, CancellationToken cancellationToken = default);
    void Logout();
}
