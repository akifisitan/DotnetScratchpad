using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard.Services;

internal sealed class DummyAuthService : IAuthService
{
    public async Task Login(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(2000, cancellationToken);
    }

    public void Logout()
    {
        return;
    }
}
