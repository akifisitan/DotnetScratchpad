using System.Text.Json;
using Scratchpad.Lib.Abstractions;
using Scratchpad.Lib.Model;

namespace Scratchpad.Lib;

public class ProjectSharer : IProjectSharer
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true,
        IndentSize = 2,
    };

    public async Task Export(
        string basePath,
        string outputFilePath,
        CancellationToken cancellationToken = default
    )
    {
        List<FileData> importData = [];

        foreach (
            var filePath in FileEnumerator.EnumerateFiles(
                basePath,
                IncludeFiles,
                IgnoreDirectories,
                cancellationToken: cancellationToken
            )
        )
        {
            importData.Add(
                new FileData(
                    Path.GetRelativePath(basePath, filePath),
                    await File.ReadAllTextAsync(filePath, cancellationToken)
                )
            );
        }

        await File.WriteAllTextAsync(
            outputFilePath,
            JsonSerializer.Serialize(importData, jsonSerializerOptions),
            cancellationToken
        );
    }

    public async Task Import(
        string importFilePath,
        string outputDirectoryPath,
        CancellationToken cancellationToken = default
    )
    {
        var importFileContent = await File.ReadAllTextAsync(importFilePath, cancellationToken);
        var importData =
            JsonSerializer.Deserialize<List<FileData>>(importFileContent)
            ?? throw new Exception("Import data not found or not valid json.");

        foreach (var fileData in importData)
        {
            var filePath = Path.Combine(outputDirectoryPath, fileData.Path);

            var dirPath = Path.GetDirectoryName(filePath)!;

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            await File.WriteAllTextAsync(filePath, fileData.Content, cancellationToken);
        }
    }

    private static bool IncludeFiles(string path)
    {
        var ext = Path.GetExtension(path);

        return ext == ".cs"
            || ext == ".csproj"
            || ext == ".sln"
            || ext == ".editorconfig"
            || ext == ".xaml"
            || ext == ".vsct"
            || ext == ".vsixmanifest";
    }

    private static bool IgnoreDirectories(string path)
    {
        return path.Contains("bin") || path.Contains("obj");
    }
}
