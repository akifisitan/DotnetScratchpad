using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Scratchpad.Lib.FileSearch;

public sealed class ZipFileSearcher : IFileSearcher
{
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
            //Log($"Error: The file at '{zipFilePath}' is not a valid zip archive.");
            return [];
        }
        catch (Exception ex)
        {
            //Log($"An unexpected error occurred: {ex.Message}");
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
            //Log($"Error: File not found at '{zipFilePath}'");
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
    }
}
