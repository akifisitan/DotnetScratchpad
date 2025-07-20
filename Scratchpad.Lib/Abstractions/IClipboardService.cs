namespace Scratchpad.Lib.Abstractions;

public interface IClipboardService
{
    Task WriteToClipboard(string text, CancellationToken cancellationToken = default);
    Task<string> ReadFromClipboard(CancellationToken cancellationToken = default);
}
