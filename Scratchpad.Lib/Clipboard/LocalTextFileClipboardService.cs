namespace Scratchpad.Lib.Clipboard;

internal class LocalTextFileClipboardService : IClipboardService
{
    private static readonly string remoteFilePath = Path.Combine(
        Environment.CurrentDirectory,
        "file.txt"
    );

    public async Task WriteToClipboard(
        string? text = null,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(1000, cancellationToken);
        await File.WriteAllTextAsync(remoteFilePath, text, cancellationToken);
    }

    public async Task<string> ReadFromClipboard(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken);
        var text = await File.ReadAllTextAsync(remoteFilePath, cancellationToken);

        return text;
    }
}
