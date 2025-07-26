using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using Spectre.Console;

namespace Scratchpad.Lib.FileSearch;

public sealed class FileSearcher : IFileSearcher
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

        var matcher = new Matcher().AddInclude(includePattern);

        if (!string.IsNullOrWhiteSpace(excludePattern))
        {
            matcher.AddExclude(excludePattern);
        }

        var filePathEnumerator = FileEnumerator.EnumerateFiles(
            searchDirectory,
            includeFilePredicate: path =>
            {
                if (!matcher.Match(searchDirectory, path).HasMatches)
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
