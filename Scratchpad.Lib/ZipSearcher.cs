using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Scratchpad.Lib;

public sealed class ZipSearcher
{
    public static void Demo(string[]? args = null)
    {
        var searchDirectoryRoot = @"C:\users\user\desktop\zip-demo";

        //Console.Write("Enter pattern: ");

        //var input = Console.ReadLine();

        //var searchPattern = !string.IsNullOrWhiteSpace(input) ?? "";
        var searchPattern = "123456";

        var startDate = DateTimeOffset.Parse("2025-06-22 20:00:00");
        var endDate = DateTimeOffset.Parse("2025-06-22 21:00:00");

        startDate = DateTimeOffset.MinValue;
        endDate = DateTimeOffset.MaxValue;

        var regex = new Regex(searchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var zipFilePaths = FileEnumerator.EnumerateFiles(
            searchDirectoryRoot,
            (p) => p.EndsWith("myzip1.zip")
        );

        var zipFileSearchOptions = new ZipFileSearchOptions(true, "");

        var sw = Stopwatch.StartNew();

        foreach (var zipFilePath in zipFilePaths)
        {
            Idk(zipFilePath);
        }

        Console.WriteLine(sw.Elapsed.TotalSeconds);

        void Idk(string zipFilePath)
        {
            var results = SearchInZip(zipFilePath, regex, startDate, endDate, zipFileSearchOptions);

            foreach (var filePath in results)
            {
                //OpenIn7Zip(filePath);
                //Log($"-> {filePath}");
            }
        }
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

    public record ZipFileSearchOptions(bool Save, string SavePath);

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
                    if (zipFileSearchOptions.Save)
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
    }

    private static void OpenIn7Zip(string path)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = @"C:\Program Files\7-Zip\7zFM.exe",
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

    private static ConcurrentBag<string> EnumerateZipFilesInDirectory(string directoryPath)
    {
        var directoryPaths = FileEnumerator.EnumerateFiles(
            directoryPath,
            (p) => p.EndsWith(".zip")
        );

        return new();
    }

    private static void CreateTestZip(string path)
    {
        using var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            archive.CreateEntry("documents/report.docx");
            archive.CreateEntry("documents/notes.txt");
            archive.CreateEntry("images/photo.jpg");
            archive.CreateEntry("logs/2025-06-20.log.txt");
        }

        File.WriteAllBytes(path, memoryStream.ToArray());
    }
}
