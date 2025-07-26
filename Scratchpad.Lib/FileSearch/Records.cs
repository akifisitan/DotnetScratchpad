namespace Scratchpad.Lib.FileSearch;

public sealed record SearchResult(string FileName, int LineNumber, string LineContent);

public sealed record ZipFileSearchOptions(string? ExtractPath = null, bool StopWhenFound = true);

public sealed record FileSearchOptions();
