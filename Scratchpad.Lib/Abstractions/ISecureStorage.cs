using Scratchpad.Lib.Model;

namespace Scratchpad.Lib.Abstractions;

public interface ISecureStorage
{
    Task SaveCredentials(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    );
    Task<UserCredentials?> GetCredentials(CancellationToken cancellationToken = default);
    void DeleteCredentials();
}
