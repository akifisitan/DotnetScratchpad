using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using Spectre.Console;

namespace Scratchpad.Lib;

public sealed record SearchResult(string FileName, int LineNumber, string LineContent);

public sealed record ZipFileSearchOptions(string? ExtractPath = null, bool StopWhenFound = true);

public static class FileSearcher
{
    public static void SearchLogFiles(
        string searchPattern,
        string searchDirectory,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        bool searchZip = false,
        string includePattern = "*",
        string? excludePattern = null,
        bool stopWhenFound = false
    )
    {
        var searchRegex = new Regex(
            searchPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(250)
        );

        var matcher = new Matcher(StringComparison.InvariantCultureIgnoreCase).AddInclude(
            includePattern
        );

        if (!string.IsNullOrWhiteSpace(excludePattern))
        {
            matcher.AddExclude(excludePattern);
        }

        var filePathEnumerator = FileEnumerator.EnumerateFiles(
            searchDirectory,
            includeFilePredicate: path =>
            {
                if (!matcher.Match(path).HasMatches)
                {
                    return false;
                }

                var lastWriteTime = File.GetLastWriteTime(path);

                //lastWriteTime.TimeOfDay >= startTime && lastWriteTime <= endTime

                //return lastWriteTime >= startDate && lastWriteTime <= endDate;

                return (startDate, endDate) switch
                {
                    (null, null) => true,
                    (not null, null) => startDate <= lastWriteTime,
                    (null, not null) => lastWriteTime <= endDate,
                    (not null, not null) => startDate <= lastWriteTime && lastWriteTime <= endDate,
                };
            }
        );

        var sw = Stopwatch.StartNew();

        var count = 0;

        foreach (var filePath in filePathEnumerator)
        {
            InternalSearch(filePath);
            count++;
        }

        Console.WriteLine(
            $"Finished searching through {count} files in {sw.Elapsed.TotalSeconds} seconds."
        );

        bool IsZip(string path) =>
            Path.GetExtension(path)?.Equals(".zip", StringComparison.InvariantCultureIgnoreCase)
                is true;

        void InternalSearch(string path)
        {
            var searchFileResult =
                searchZip && IsZip(path)
                    ? SearchInZip(
                        path,
                        searchRegex,
                        includeFilePredicate: entry =>
                            matcher.Match(entry.FullName).HasMatches
                            && entry.LastWriteTime >= startDate
                            && entry.LastWriteTime <= endDate
                    )
                    : SearchInFile(path, searchRegex);

            foreach (var searchResult in searchFileResult)
            {
                Log($"{searchResult}");
            }
        }
    }

    private static IEnumerable<SearchResult> SearchInFile(
        string filePath,
        Regex searchRegex,
        bool stopWhenFound = true
    )
    {
        if (!File.Exists(filePath))
        {
            Log($"Error: File not found at '{filePath}'");
            yield break;
        }

        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);

        string? line;
        var lineNumber = 0;
        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;
            if (searchRegex.IsMatch(line))
            {
                yield return new(Path.GetFileName(filePath), lineNumber, line);
                if (stopWhenFound)
                {
                    break;
                }
            }
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }

    private static string GetUserChoice(List<string> choices)
    {
        var prompt = new SelectionPrompt<string>()
            .Title("Pick one or more files to open.")
            .EnableSearch()
            .SearchPlaceholderText("Search: ")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
            .AddChoices(choices)
            .WrapAround();

        prompt.SearchHighlightStyle = new Style(
            foreground: Color.Blue,
            background: Color.Red,
            decoration: Decoration.Underline
        );

        return AnsiConsole.Prompt(prompt);
    }

    public static IEnumerable<SearchResult> SearchInZip(
        string zipFilePath,
        Regex searchRegex,
        Func<ZipArchiveEntry, bool>? includeFilePredicate = null,
        Func<ZipArchiveEntry, bool>? excludeFilePredicate = null,
        ZipFileSearchOptions? zipFileSearchOptions = null
    )
    {
        try
        {
            return SearchInZipInternal(
                zipFilePath,
                searchRegex,
                includeFilePredicate: includeFilePredicate,
                excludeFilePredicate: excludeFilePredicate,
                zipFileSearchOptions: zipFileSearchOptions
            );
        }
        catch (InvalidDataException)
        {
            Log($"Error: The file at '{zipFilePath}' is not a valid zip archive.");
            return [];
        }
        catch (Exception ex)
        {
            Log($"An unexpected error occurred: {ex.Message}");
            return [];
        }
    }

    private static IEnumerable<SearchResult> SearchInZipInternal(
        string zipFilePath,
        Regex searchRegex,
        Func<ZipArchiveEntry, bool>? includeFilePredicate = null,
        Func<ZipArchiveEntry, bool>? excludeFilePredicate = null,
        ZipFileSearchOptions? zipFileSearchOptions = null
    )
    {
        if (!File.Exists(zipFilePath))
        {
            Log($"Error: File not found at '{zipFilePath}'");
            yield break;
        }

        zipFileSearchOptions ??= new();
        includeFilePredicate ??= _ => true;
        excludeFilePredicate ??= _ => false;

        // Warning: This stream is not thread safe
        using var archive = ZipFile.OpenRead(zipFilePath);

        List<string> tp = [];

        foreach (var entry in archive.Entries)
        {
            if (excludeFilePredicate(entry) || !includeFilePredicate(entry))
            {
                continue;
            }

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            string? line;
            var lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                if (searchRegex.IsMatch(line))
                {
                    tp.Add(entry.FullName);

                    if (!string.IsNullOrWhiteSpace(zipFileSearchOptions.ExtractPath))
                    {
                        var sw = Stopwatch.StartNew();
                        entry.ExtractToFile($"{entry.FullName.Replace('/', '-')}", overwrite: true);
                        Console.WriteLine(
                            $"Time taken to extract {entry.Length} bytes: {sw.Elapsed.TotalMilliseconds} ms."
                        );
                    }

                    yield return new(Path.Combine(zipFilePath, entry.FullName), lineNumber, line);

                    if (zipFileSearchOptions.StopWhenFound)
                    {
                        break;
                    }
                }
            }
        }

        var a = GetUserChoice(tp);

        var e = archive.GetEntry(a)!;
        var f = $"{e.FullName.Replace('/', '-')}";

        e.ExtractToFile(f, overwrite: true);

        Open(f);
    }

    private static void Open(string path)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = @"C:\Program Files\Notepad++\notepad++.exe",
                Arguments = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        );
    }
}
