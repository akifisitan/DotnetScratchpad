using Scratchpad.Lib.Abstractions;

namespace Scratchpad.ConsoleApp;

internal class Runner : IRunner
{
    private readonly IDirectoryPacker _projectSharer;

    public Runner(IDirectoryPacker projectSharer)
    {
        _projectSharer = projectSharer;
    }

    public async Task Run(string[] args)
    {
        await RunProjectSharer();
    }

    private async Task RunProjectSharer()
    {
        var directoryPathToClone = @"C:\Users\user\projects\demos\dotnet-demos\DotnetScratchpad";

        var filePath = Path.Combine(Environment.CurrentDirectory, "importData.json");

        if (!Directory.Exists(directoryPathToClone))
        {
            throw new Exception("Directory does not exist.");
        }

        await _projectSharer.Pack(directoryPathToClone, filePath);

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

        await _projectSharer.Unpack(filePath, outputDirectoryPath);
    }
}
