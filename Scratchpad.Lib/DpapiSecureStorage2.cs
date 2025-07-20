using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Scratchpad.Lib.Abstractions;
using Scratchpad.Lib.Model;

namespace Scratchpad.Lib;

internal sealed class DpapiSecureStorage2 : ISecureStorage
{
    private readonly IDataProtector _dataProtector;

    public DpapiSecureStorage2(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("Wowzer");
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
            var encryptedData = _dataProtector.Protect(dataToEncrypt);
        }
        catch { }
    }

    public async Task<UserCredentials?> GetCredentials(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            //var encryptedData = await File.ReadAllBytesAsync(_filePath, cancellationToken);
            byte[] encryptedData = [1, 2, 3];
            var decryptedData = _dataProtector.Unprotect(encryptedData);

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
            //File.Delete(_filePath);
        }
        catch
        {
            // Handle deletion errors
        }
    }

    public static void Register(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Environment.CurrentDirectory))
            .ProtectKeysWithDpapi();
    }
}
