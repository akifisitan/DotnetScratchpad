using Scratchpad.Lib.Abstractions;

namespace Scratchpad.ConsoleApp;

internal class Runner : IRunner
{
    private readonly IProjectSharer _projectSharer;

    public Runner(IProjectSharer projectSharer)
    {
        _projectSharer = projectSharer;
    }

    public async Task Run(string[] args)
    {
        var directoryPathToClone = @"C:\Users\user\projects\demos\dotnet-demos\DotnetScratchpad";

        var filePath = Path.Combine(Environment.CurrentDirectory, "importData.json");

        await _projectSharer.Export(directoryPathToClone, filePath);

        if (!Directory.Exists(directoryPathToClone))
        {
            throw new Exception("Directory does not exist.");
        }

        var outputDirectoryPath =
            @"C:\Users\user\projects\demos\dotnet-demos\DotnetScratchpad-clone";

        if (!File.Exists(filePath))
        {
            throw new Exception("Import file does not exist.");
        }

        if (Directory.Exists(outputDirectoryPath))
        {
            throw new Exception("Directory already exists. Exiting.");
        }

        Directory.CreateDirectory(outputDirectoryPath);

        await _projectSharer.Import(filePath, outputDirectoryPath);
    }
}
