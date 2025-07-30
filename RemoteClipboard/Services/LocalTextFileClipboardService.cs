using System.IO;
using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard.Services;

internal class LocalTextFileClipboardService : IClipboardService
{
    private static string DirectoryBasePath =>
        Path.Combine(Environment.CurrentDirectory, DesktopContext.UserCredentials!.ClipboardId);

    private static TimeSpan Jitter => TimeSpan.FromMilliseconds(new Random().Next(200, 2000));

    public async Task WriteToClipboard(
        string? text = null,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(Jitter, cancellationToken);

        Directory.CreateDirectory(DirectoryBasePath);

        var fileName = $"{Guid.NewGuid()}.txt";
        await File.WriteAllTextAsync(
            Path.Combine(DirectoryBasePath, fileName),
            text,
            cancellationToken
        );
    }

    public async Task<string> ReadFromClipboard(CancellationToken cancellationToken = default)
    {
        await Task.Delay(Jitter, cancellationToken);
        var fileName = GetLastNFiles(1).FirstOrDefault();

        if (fileName is null)
        {
            return string.Empty;
        }

        return await File.ReadAllTextAsync(fileName, cancellationToken);
    }

    public async Task<List<string>> ReadLastNEntriesFromClipboard(
        int n,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(Jitter, cancellationToken);
        var filePathlist = GetLastNFiles(n);
        List<string> result = [];

        foreach (var item in filePathlist)
        {
            result.Add(await File.ReadAllTextAsync(item, cancellationToken));
        }

        return result;
    }

    private static List<string> GetLastNFiles(int n)
    {
        Directory.CreateDirectory(DirectoryBasePath);
        return Directory
            .EnumerateFiles(DirectoryBasePath)
            .OrderByDescending(File.GetLastWriteTime)
            .Take(n)
            .ToList();
    }
}
