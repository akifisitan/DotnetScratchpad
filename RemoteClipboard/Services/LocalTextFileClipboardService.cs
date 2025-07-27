using System.IO;
using Scratchpad.Lib.Clipboard;

namespace RemoteClipboard.Services;

internal class LocalTextFileClipboardService : IClipboardService
{
    private static readonly string directoryBasePath = Path.Combine(
        Environment.CurrentDirectory,
        "mydir"
    );

    private static TimeSpan Jitter => TimeSpan.FromMilliseconds(new Random().Next(200, 4000));

    static LocalTextFileClipboardService()
    {
        Directory.CreateDirectory(directoryBasePath);
    }

    public async Task WriteToClipboard(
        string? text = null,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(Jitter, cancellationToken);

        var fileName = $"{Guid.NewGuid()}.txt";
        await File.WriteAllTextAsync(
            Path.Combine(directoryBasePath, fileName),
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

    private static List<string> GetLastNFiles(int n)
    {
        return Directory
            .EnumerateFiles(directoryBasePath)
            .OrderByDescending(File.GetLastWriteTime)
            .Take(n)
            .ToList();
    }
}
