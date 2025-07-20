using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Scratchpad.Lib.Abstractions;
using Scratchpad.Lib.Model;

namespace Scratchpad.Lib;

internal sealed class DpapiSecureStorage : ISecureStorage
{
    //private static readonly string _filePath = Path.Combine(
    //    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    //    "RemoteClipboard",
    //    "credentials.dat"
    //);

    private static readonly string _filePath = Path.Combine(
        Environment.CurrentDirectory,
        "encryptedcredentials.dat"
    );

    public DpapiSecureStorage()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
    }

    public async Task SaveCredentials(
        UserCredentials userCredentials,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var json = JsonSerializer.Serialize(
                userCredentials,
                AppJsonSerializerContext.Default.UserCredentials
            );
            var dataToEncrypt = Encoding.UTF8.GetBytes(json);
            var encryptedData = ProtectedData.Protect(
                dataToEncrypt,
                null,
                DataProtectionScope.CurrentUser
            );

            await File.WriteAllBytesAsync(_filePath, encryptedData, cancellationToken);
        }
        catch { }
    }

    public async Task<UserCredentials?> GetCredentials(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var encryptedData = await File.ReadAllBytesAsync(_filePath, cancellationToken);
            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser
            );

            var json = Encoding.UTF8.GetString(decryptedData);

            return JsonSerializer.Deserialize(
                json,
                AppJsonSerializerContext.Default.UserCredentials
            );
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
            File.Delete(_filePath);
        }
        catch
        {
            // Handle deletion errors
        }
    }
}
