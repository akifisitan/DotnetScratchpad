namespace Scratchpad.Lib.Clipboard;

public interface IClipboardService
{
    Task WriteToClipboard(string text, CancellationToken cancellationToken = default);
    Task<string> ReadFromClipboard(CancellationToken cancellationToken = default);
}
