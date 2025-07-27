using RemoteClipboard.Model;

namespace RemoteClipboard.Abstractions;

public interface IAuthService
{
    Task ValidateCredentials(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    );
}
