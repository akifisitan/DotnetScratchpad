using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard.Services;

public class SecureStorage : ISecureStorage
{
    private static string GetFilePath =>
        Path.Combine(Environment.CurrentDirectory, "encryptedcredentials.dat");

    public async Task SaveCredentials(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var json = JsonSerializer.Serialize(userCredentials);
            var dataToEncrypt = Encoding.UTF8.GetBytes(json);
            var encryptedData = ProtectedData.Protect(
                dataToEncrypt,
                null,
                DataProtectionScope.CurrentUser
            );

            await File.WriteAllBytesAsync(GetFilePath, encryptedData, cancellationToken);
        }
        catch { }
    }

    public async Task<UserCredentials?> GetCredentials(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var encryptedData = await File.ReadAllBytesAsync(GetFilePath, cancellationToken);
            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser
            );

            var json = Encoding.UTF8.GetString(decryptedData);

            return JsonSerializer.Deserialize<UserCredentials>(json);
        }
        catch
        {
            return null;
        }
    }

    public void DeleteCredentials()
    {
        try
        {
            File.Delete(GetFilePath);
        }
        catch { }
    }
}
