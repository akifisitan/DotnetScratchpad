namespace RemoteClipboard.Abstractions;

public interface IClipboardService
{
    Task WriteToClipboard(string text, CancellationToken cancellationToken = default);
    Task<string> ReadFromClipboard(CancellationToken cancellationToken = default);
    Task<List<string>> ReadLastNEntriesFromClipboard(
        int n,
        CancellationToken cancellationToken = default
    );
}
