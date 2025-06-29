using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace Scratchpad.Lib;

public static class ZipSearcher
{
    public static void SearchLogFiles(
        string searchPattern,
        string searchDirectory,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate
    )
    {
        //var searchDirectoryRoot = @"C:\users\user\desktop\zip-demo";
        //var searchPattern = "123456";
        //var startDate = DateTimeOffset.Parse("2025-06-22 20:00:00");
        //var endDate = DateTimeOffset.Parse("2025-06-22 21:00:00");

        var searchRegex = new Regex(
            searchPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(1)
        );

        var zipFilePaths = FileEnumerator.EnumerateFiles(
            searchDirectory,
            (p) => p.EndsWith("myzip1.zip")
        );

        var zipFileSearchOptions = new ZipFileSearchOptions();

        var sw = Stopwatch.StartNew();

        foreach (var zipFilePath in zipFilePaths)
        {
            Idk(zipFilePath);
        }

        Console.WriteLine(sw.Elapsed.TotalSeconds);

        void Idk(string zipFilePath)
        {
            var results = SearchInZip(
                zipFilePath,
                searchRegex,
                startDate ?? DateTimeOffset.MinValue,
                endDate ?? DateTimeOffset.MaxValue,
                zipFileSearchOptions
            );

            foreach (var filePath in results)
            {
                //OpenIn7Zip(filePath);
                Log($"-> {filePath}");
            }
        }
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

    public static IEnumerable<string> SearchInZip(
        string zipFilePath,
        Regex searchRegex,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        ZipFileSearchOptions zipFileSearchOptions
    )
    {
        try
        {
            return SearchInZipInternal(
                zipFilePath,
                searchRegex,
                startDate,
                endDate,
                zipFileSearchOptions
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

    public record ZipFileSearchOptions(string? ExtractPath = null, bool ExitEarly = false);

    // Choice 1: Extract and open .log files from zip automatically as they are found
    // Choice 2: Find all .log files and display them as a list to the user, when the user presses enter on them extract and open them
    // Choice 3: Switches for both?
    // Things to consider:
    // 1. What if there are too many files found? Unzipping them would take long and could be a problem. Choice 2 is better for it.
    // 2.
    private static IEnumerable<string> SearchInZipInternal(
        string zipFilePath,
        Regex searchRegex,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        ZipFileSearchOptions zipFileSearchOptions
    )
    {
        if (!File.Exists(zipFilePath))
        {
            Log($"Error: File not found at '{zipFilePath}'");
            yield break;
        }

        // This stream is not thread safe
        using var archive = ZipFile.OpenRead(zipFilePath);

        List<string> tp = [];

        foreach (var entry in archive.Entries)
        {
            if (
                !entry.FullName.EndsWith(".log", StringComparison.OrdinalIgnoreCase)
                || startDate > entry.LastWriteTime
                || endDate < entry.LastWriteTime
            )
            {
                continue;
            }

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            string? line;

            while ((line = reader.ReadLine()) != null)
            {
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
                    yield return Path.Combine(zipFilePath, entry.FullName);
                    break;
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

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }
}
