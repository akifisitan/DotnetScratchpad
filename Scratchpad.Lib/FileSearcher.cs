using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Scratchpad.Lib;

public static class FileSearcher
{
    public static void SearchLogFiles(
        string searchPattern,
        string searchDirectory,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate
    )
    {
        var searchRegex = new Regex(
            searchPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(250)
        );

        startDate ??= DateTimeOffset.MinValue;
        endDate ??= DateTimeOffset.MaxValue;

        var filePaths = FileEnumerator.EnumerateFiles(
            searchDirectory,
            p =>
            {
                if (!p.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var lastWriteTime = File.GetLastWriteTime(p);

                return lastWriteTime >= startDate && lastWriteTime <= endDate;
            }
        );

        var sw = Stopwatch.StartNew();

        var count = 0;

        foreach (var zipFilePath in filePaths)
        {
            Idk(zipFilePath);
        }

        Console.WriteLine(
            $"Finished searching through {count} files in {sw.Elapsed.TotalSeconds} seconds."
        );

        void Idk(string zipFilePath)
        {
            var results = SearchInFile(zipFilePath, searchRegex);

            foreach (var filePath in results)
            {
                Log($"{filePath}");
            }

            count++;
        }
    }

    private static IEnumerable<(string FileName, int LineNumber, string LineContent)> SearchInFile(
        string filePath,
        Regex searchRegex
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
                yield return (Path.GetFileName(filePath), lineNumber, line);
                //break;
            }
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }
}
