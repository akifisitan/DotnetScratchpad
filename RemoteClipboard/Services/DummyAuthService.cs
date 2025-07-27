using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard.Services;

internal sealed class DummyAuthService : IAuthService
{
    private static TimeSpan Jitter => TimeSpan.FromMilliseconds(new Random().Next(200, 2000));

    public async Task ValidateCredentials(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(Jitter, cancellationToken);
    }
}
