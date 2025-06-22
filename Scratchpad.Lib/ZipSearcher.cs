using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Scratchpad.Lib;

internal sealed class Program
{
    private static ConcurrentBag<string> SearchInZipParallel(
        string zipFilePath,
        Regex searchRegex,
        DateTimeOffset startDate,
        DateTimeOffset endDate
    )
    {
        // Use a thread-safe collection to store results from multiple threads.
        var foundFiles = new ConcurrentBag<string>();

        try
        {
            if (!File.Exists(zipFilePath))
            {
                Log($"Error: File not found at '{zipFilePath}'");
                return foundFiles;
            }

            // Step 1: Get a list of entry names to process. This is a quick
            // metadata read and avoids sharing the ZipArchive instance.
            List<string> entries = [];
            using (var initialArchive = ZipFile.OpenRead(zipFilePath))
            {
                entries = initialArchive
                    .Entries.Where(entry =>
                        entry.FullName.EndsWith(".log", StringComparison.OrdinalIgnoreCase)
                        && startDate <= entry.LastWriteTime
                        && endDate >= entry.LastWriteTime
                    )
                    .Select(entry => entry.FullName)
                    .ToList();
            }

            // Step 2: Process the filtered list of entries in parallel.
            Parallel.ForEach(
                entries,
                new ParallelOptions { MaxDegreeOfParallelism = 16 },
                entryFullName =>
                {
                    // Each thread opens its own ZipArchive instance.
                    using var archive = ZipFile.OpenRead(zipFilePath);
                    var entry = archive.GetEntry(entryFullName);

                    if (entry is null)
                    {
                        return;
                    }

                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream);

                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (searchRegex.IsMatch(line))
                        {
                            foundFiles.Add(Path.Combine(zipFilePath, entry.FullName));
                            // Found a match, no need to read the rest of this file.
                            break;
                        }
                    }
                }
            );
        }
        catch (InvalidDataException)
        {
            Log($"Error: The file at '{zipFilePath}' is not a valid zip archive.");
        }
        catch (Exception ex)
        {
            Log($"An unexpected error occurred: {ex.Message}");
        }

        return foundFiles;
    }

    private static List<string> SearchInZip(
        string zipFilePath,
        Regex searchRegex,
        DateTimeOffset startDate,
        DateTimeOffset endDate
    )
    {
        var foundFiles = new List<string>();

        try
        {
            if (!File.Exists(zipFilePath))
            {
                Log($"Error: File not found at '{zipFilePath}'");
                return foundFiles;
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
                        foundFiles.Add(Path.Combine(zipFilePath, entry.FullName));
                        break;
                    }
                }
            }
        }
        catch (InvalidDataException)
        {
            Log($"Error: The file at '{zipFilePath}' is not a valid zip archive.");
        }
        catch (Exception ex)
        {
            Log($"An unexpected error occurred: {ex.Message}");
        }

        return foundFiles;
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }

    private static ConcurrentBag<string> EnumerateZipFilesInDirectory(string directoryPath)
    {
        var directoryPaths = Directory
            .EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
            .AsParallel();

        var bag = new ConcurrentBag<string>();

        foreach (var path in directoryPaths) { }

        return bag;
    }

    public static void Runner()
    {
        // Create a dummy zip file for demonstration
        string zipPath = @"C:\users\user\desktop\hello.zip";

        Console.Write("Enter pattern: ");

        var input = Console.ReadLine();

        var searchPattern = !string.IsNullOrWhiteSpace(input) ? input : "123456";

        var startDate = DateTimeOffset.Parse("2025-06-22 20:00:00");
        var endDate = DateTimeOffset.Parse("2025-06-22 21:00:00");

        var regex = new Regex(searchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var sw = Stopwatch.StartNew();
        //var results = SearchInZip(zipPath, regex, startDate, endDate);
        var results = SearchInZipParallel(zipPath, regex, startDate, endDate);
        Console.WriteLine(sw.Elapsed.TotalSeconds);
        sw.Restart();

        if (results.Count > 0)
        {
            Log("Found files:");
            foreach (string fileName in results)
            {
                Log($"- {fileName}");
            }
        }
        else
        {
            Log("No matching files found.");
        }
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
